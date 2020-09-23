using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 10.0f; //more is faster
    public float accelerateSpeed = 10f; //more is faster to reach top speed
    public float timeToStop = 1f; //max time for speed to reach 0; calculate in seconds; 0 should remove drag; will create drag when turning in cam dependent mode
    public float drag = 20f; //more is faster stoping
    public float gravity = -10f; //more mean faster falling
    public float jumpHeight=2f; //more mean higher jump
    public float jumpStep = 0.2f; //for hold = height
    public float peakTime = 0.5f; //time to reach peak jump height
    public float sprintMultiplier = 2f; //more mean faster when run, <1 will result in slower when use run (can be used for walk)
    public float crouchTime=0.5f; //time to enter and exit crouch, lower mean faster
    public float crouchMultiplier = 0.7f; //speed reduce when crouch, >1 will result in faster
    public float crouchHeight = 1f; //object height when crouch
    public float originalHeight = 2f;   //object height when stand (init height)
    public int jumpTime = 2;    //number of jumps (double jumps)
    public float delayJumpTimeCheck =0.5f; //time in seconds, how long will the player be able to jump after leaving the ground (usually used in platformer)
    public float airJumpModifier = 0.5f;    //strength of second jump and above (=1 is same strength with initial jump, lower is shorter)
    public float airGlideSlowModifier = 0.5f; //glide slow the landing (reduce gravity);
    public float slideTime = 0.5f; //time slide (time to remove sprint speed when crouch during sprinting)
    public float dashTime = 3f; //duration of a dash in second
    public float dashSpeed = 1f;    //how fast the dash will go in the duration; distance depend on both values
    public bool canAirDash = false;
    public bool isCamDependent = false;
    public Transform cam;
    public float smoothRotationTime = 0.1f;
    public float smoothSpeed = 1f;
    private float x;
    private float z;
    float dashX;
    float dashZ;
    float upTime;
    float downTime;
    float rightTime;
    float leftTime;
    int jumpLeft;
    float currTime;
    float dashTimeLeft;

    

    public CharacterController characterController; //character controller (required for the script to work)

    public LayerMask groundMask;    //ground layer (for ground checking)
    public float groundCheckDistance = 0.4f;    //distance to calculate from the point of checking to the ground
    public Transform groundCheck;   //child object at the bottom of the player

    private bool isGrounded;
    private Vector3 groundVelocity;
    private bool isCrouching = false;
    private bool isSprinting = false;
    bool isDashing = false;

    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode dashKey = KeyCode.R;
    // Use this for initialization
    void Start()
    {
        characterController.height = originalHeight;
        // turn off the cursor
        //Cursor.lockState = CursorLockMode.Locked;
    }
    private void FixedUpdate() {
        RaycastHit hit;
        if(Physics.Raycast(groundCheck.position, -Vector3.up, out hit, groundCheckDistance)){
            isGrounded = true;
            if (delayJumpTimeCheck > 0)
            {
                CancelInvoke("DelayJump");
            }
            
        }else{
            if(delayJumpTimeCheck > 0){
                Invoke("DelayJump", delayJumpTimeCheck);
            }else{
                isGrounded = false;
                if(jumpLeft == jumpTime){
                    jumpLeft = jumpTime - 1;
                }
            }
        }
        //Debug.Log(isGrounded);
        Debug.DrawLine(groundCheck.position, groundCheck.position - Vector3.up * groundCheckDistance);
    }

    // Update is called once per frame
    void Update()
    {
        if(isGrounded && groundVelocity.y < 0){
            groundVelocity.y = -2f; 
            jumpLeft = jumpTime;
            currTime = peakTime;
        }

        CheckUpTime();
        CheckDownTime();
        CheckLeftTime();
        CheckRightTime();

        CrouchTranform();

        CheckMoveKeyDown();
        CheckMoveKeyHold();
        CheckMoveKeyUp();

        Dashing();
        // z = Input.GetAxis("Vertical");
        // x = Input.GetAxis("Horizontal");
        // Debug.Log(x +" "+ z);
        

        Sprinting();

        Jumping();

        Crouching();

        Move();
    }

    private void Move()
    {
        Vector3 move = z * transform.forward + x * transform.right;

        if (isSprinting && move == Vector3.zero)
        {
            isSprinting = false;
            speed = speed / sprintMultiplier;
        }
        if (!isDashing)
        {
            if(isCamDependent){
            
                    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, Mathf.Atan2(x, z) * Mathf.Rad2Deg + cam.eulerAngles.y, ref smoothSpeed, smoothRotationTime);
                    Quaternion q = Quaternion.Euler(0f, angle, 0f);
                    if(x!= 0 || z != 0){
                        transform.rotation = q;
                    }
                    float velocity = 0f;
                    if(Mathf.Abs(x)<Mathf.Abs(z)){
                        velocity = Mathf.Abs(z);
                    }else{
                        velocity = Mathf.Abs(x);
                    }    
                        move = velocity * transform.forward;

                characterController.Move(move * speed * Time.deltaTime);
            }else{
                characterController.Move(move * speed * Time.deltaTime); 
            }
            characterController.Move(groundVelocity * Time.deltaTime);
        }
        else
        {
            if (dashTimeLeft > 0)
            {
                Vector3 moveDist;
                if(isCamDependent){
                    moveDist = (transform.forward) * dashSpeed * Time.deltaTime;
                }
                else{
                    moveDist = (dashZ * transform.forward + dashX * transform.right) * dashSpeed * Time.deltaTime;
                }
                dashTimeLeft -= Time.deltaTime;
                characterController.Move(moveDist);
            }
            else
            {
                isDashing = false;
                x = 0;
                z = 0;
            }
            if (isGrounded)
            {
                characterController.Move(groundVelocity * Time.deltaTime);
            }
        }
    }

    void SlideSpeedReduce(){
        speed = speed/sprintMultiplier;
    }

    void CheckUpTime(){
        if (upTime >= 0)
        {
            upTime -= Time.deltaTime;
            z = Mathf.Clamp(z - Time.deltaTime * drag, 0, 1);
            if (upTime <= 0)
            {
                z = 0;
                return;
            }
            if (z <= 0)
            {
                upTime = -1;
            }
        }
    }

    void CheckDownTime(){
        if (downTime >= 0)
        {
            downTime -= Time.deltaTime;
            z = Mathf.Clamp(z + Time.deltaTime * drag, -1, 0);
            if (downTime <= 0)
            {
                z = 0;
                return;
            }
            if (z >= 0)
            {
                downTime = -1;
            }
        }
    }

    void CheckRightTime(){
        if (rightTime >= 0)
        {
            rightTime -= Time.deltaTime;
            x = Mathf.Clamp(x - Time.deltaTime * drag, 0, 1);
            if (rightTime <= 0)
            {
                x = 0;
                return;
            }
            if (x <= 0)
            {
                rightTime = -1;
            }
        }
    }

    void CheckLeftTime(){
        if (leftTime >= 0)
        {
            leftTime -= Time.deltaTime;
            x = Mathf.Clamp(x + Time.deltaTime * drag, -1, 0);
            if (leftTime <= 0)
            {
                x = 0;
                return;
            }
            if (x >= 0)
            {
                leftTime = -1;
            }  
        }
    }

    void CrouchTranform(){
        if (!isCrouching)
        {
            if (characterController.height < originalHeight)
                characterController.height = Mathf.MoveTowards(characterController.height, originalHeight, Time.deltaTime / crouchTime);
        }
        else
        {
            if (characterController.height > crouchHeight)
                characterController.height = Mathf.MoveTowards(characterController.height, crouchHeight, Time.deltaTime / crouchTime);
        }
    }

    void CheckMoveKeyDown(){
        if (Input.GetKeyDown(forwardKey))
        {
            upTime = -1;
        }
        if (Input.GetKeyDown(backwardKey))
        {
            downTime = -1;
        }
        if (Input.GetKeyDown(rightKey))
        {
            rightTime = -1;
        }
        if (Input.GetKeyDown(leftKey))
        {
            leftTime = -1;
        }
    }
    void CheckMoveKeyHold(){
        if (Input.GetKey(forwardKey))
        {
            z = Mathf.Clamp(z + Time.deltaTime * accelerateSpeed, -1, 1);
        }
        if (Input.GetKey(backwardKey))
        {
            z = Mathf.Clamp(z - Time.deltaTime * accelerateSpeed, -1, 1);
        }
        if (Input.GetKey(rightKey))
        {
            x = Mathf.Clamp(x + Time.deltaTime * accelerateSpeed, -1, 1);
        }
        if (Input.GetKey(leftKey))
        {
            x = Mathf.Clamp(x - Time.deltaTime * accelerateSpeed, -1, 1);
        }
    }

    void CheckMoveKeyUp(){
        if (Input.GetKeyUp(forwardKey))
        {
            upTime = timeToStop;
        }
        if (Input.GetKeyUp(backwardKey))
        {
            downTime = timeToStop;
        }
        if (Input.GetKeyUp(rightKey))
        {
            rightTime = timeToStop;
        }
        if (Input.GetKeyUp(leftKey))
        {
            leftTime = timeToStop;
        }
    }

    void Sprinting(){
        if (isGrounded && Input.GetKeyDown(sprintKey))
        {
            if (isCrouching)
            {
                StandUp();
            }
            if (isSprinting)
            {
                isSprinting = false;
                speed = speed / sprintMultiplier;
            }
            else
            {
                isSprinting = true;
                speed = speed * sprintMultiplier;
            }
        }
    }

    void StandUp(){
        isCrouching = false;
        speed = speed / crouchMultiplier;
        groundCheck.position = new Vector3(groundCheck.position.x, groundCheck.position.y - ((originalHeight - crouchHeight) / 2), groundCheck.position.z);
    }

    void CrouchDown(){
        isCrouching = true;
        speed = speed * crouchMultiplier;
        groundCheck.position = new Vector3(groundCheck.position.x, groundCheck.position.y + ((originalHeight - crouchHeight) / 2), groundCheck.position.z);
    }

    void Jumping(){
        if (Input.GetKeyDown(jumpKey) && !isGrounded && jumpLeft > 0)
        {
            groundVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity * airJumpModifier);
            jumpLeft--;
        }

        if (Input.GetKeyDown(jumpKey) && isGrounded && jumpLeft > 0)
        {
            if (isCrouching)
            {
                StandUp();
            }
            else
            {
                isGrounded = false;
                if (peakTime > 0)
                    groundVelocity.y = Mathf.Sqrt(jumpHeight * jumpStep * -2f * gravity);
                else
                    groundVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpLeft--;
            }
        }

        

        float v = gravity * Time.deltaTime;

        if (Input.GetKey(jumpKey) && !isGrounded)
        {
            if (groundVelocity.y < 0)
            {
               v *= airGlideSlowModifier;
            }
            if(currTime > 0 && groundVelocity.y> 0){
                groundVelocity.y = Mathf.Sqrt(jumpHeight * jumpStep * -2f * gravity);
                currTime -=Time.deltaTime;
            }
        }
        groundVelocity.y += v;

        if(Input.GetKeyUp(jumpKey)){
            currTime = -1;
        }
    }

    void Crouching(){
        if (Input.GetKeyDown(crouchKey) && isGrounded)
        {
            if (isSprinting)
            {
                if(slideTime > 0){
                Invoke("SlideSpeedReduce", slideTime);
                isSprinting = false;
                }
                else{
                    StandUp();
                }
            }
            if (isCrouching)
            {
                StandUp();
            }
            else
            {
                CrouchDown();
            }
        }
    }
    void DelayJump(){
        isGrounded = false;
        if(jumpLeft == jumpTime){
            jumpLeft = jumpTime - 1;
        }
        //Debug.Log("delay");
    }

    void Dashing(){
        if(Input.GetKeyDown(dashKey) && !isDashing){
            if(canAirDash){
                Dash();
            }else{
                if(!isGrounded){
                    return;
                }
                Dash();
            }
        }
    }

    void Dash(){
        dashX = 0;
        dashZ = 1;
        if(Input.GetKey(rightKey)){
            dashX = 1;
            dashZ = 0;
        }else if (Input.GetKey(leftKey)){
            dashX = -1;
            dashZ = 0;
        }
        if(Input.GetKey(forwardKey)){
            dashZ = 1;
        }else if(Input.GetKey(backwardKey)){
            dashZ = -1;
        }

        dashTimeLeft = dashTime;
        isDashing = true;
    }
}
