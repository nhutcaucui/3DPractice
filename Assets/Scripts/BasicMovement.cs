using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 10.0f;
    public float accelerateSpeed = 10f;
    public float timeToStop = 1f;
    public float drag = 20f;
    public float gravity = -10f;
    public float jumpHeight=2f;
    public float sprintMultiplier = 2f;
    public float crouchTime=0.5f;
    public float crouchMultiplier = 0.7f;
    public float crouchHeight = 1f;
    public float originalHeight = 2f;
    private float x;
    private float z;
    float upTime;
    float downTime;
    float rightTime;
    float leftTime;
    public GameObject cam;

    public CharacterController characterController;

    public LayerMask groundMask;
    public float groundCheckDistance = 0.4f;
    public Transform groundCheck;

    private bool isGrounded;
    private Vector3 groundVelocity;
    private bool isCrouching = false;
    private bool isSprinting = false;

    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode crouchKey = KeyCode.C;
    public KeyCode jumpKey = KeyCode.Space;
    // Use this for initialization
    void Start()
    {
        characterController.height = originalHeight;
        // turn off the cursor
        //Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckDistance, groundMask);

        if(isGrounded && groundVelocity.y < 0){
            groundVelocity.y = -2f; 
        }

        if(upTime > 0){
            upTime -= Time.deltaTime;
            z = Mathf.Clamp(z - Time.deltaTime * drag, 0, 1);
            if(upTime <= 0){
                z = 0;
            }
            if (z <= 0)
            {
                upTime = -1;
            }
        }
        if(downTime > 0){
            downTime -= Time.deltaTime;
            z = Mathf.Clamp(z + Time.deltaTime * drag, -1, 0);
            if (downTime <= 0)
            {
                z = 0;
            }
            if (z >= 0)
            {
                downTime = -1;
            }
        }
        if (rightTime > 0)
        {
            rightTime -= Time.deltaTime;
            x = Mathf.Clamp(x - Time.deltaTime * drag, 0, 1);
            if (rightTime <= 0)
            {
                x = 0;
            }
            if(x <= 0){
                rightTime = -1;
            }
        }
        if (leftTime > 0)
        {
            leftTime -= Time.deltaTime;
            x = Mathf.Clamp(x + Time.deltaTime * drag, -1, 0);
            if (leftTime <= 0)
            {
                x = 0;
            }
            if (x >= 0)
            {
                leftTime = -1;
            }
        }

        if(!isCrouching){
            if(characterController.height<originalHeight)
            characterController.height = Mathf.MoveTowards(characterController.height, originalHeight, Time.deltaTime / crouchTime);
        }else{
            if (characterController.height >crouchHeight)
            characterController.height = Mathf.MoveTowards(characterController.height, crouchHeight, Time.deltaTime / crouchTime);
        }

        //x=0;
        if(Input.GetKey(forwardKey)){
            z= Mathf.Clamp(z + Time.deltaTime * accelerateSpeed, -1,1);
        }else{}
        if(Input.GetKey(backwardKey)){
            z = Mathf.Clamp(z - Time.deltaTime * accelerateSpeed, -1, 1);
        }
        //z=0;
        if(Input.GetKey(rightKey)){
            x = Mathf.Clamp(x + Time.deltaTime * accelerateSpeed, -1, 1);
        }
        if (Input.GetKey(leftKey))
        {
            x = Mathf.Clamp(x - Time.deltaTime * accelerateSpeed, -1, 1);
        }

        if(Input.GetKeyUp(forwardKey)){
            upTime = timeToStop;
        }
        if (Input.GetKeyUp(backwardKey))
        {
            downTime = timeToStop;
        }
        if(Input.GetKeyUp(rightKey)){
            rightTime = timeToStop;
        }
        if (Input.GetKeyUp(leftKey))
        {
            leftTime = timeToStop;
        }
        // z = Input.GetAxis("Vertical");
        // x = Input.GetAxis("Horizontal");
        Debug.Log(x +" "+ z);
        Vector3 move = z * transform.forward + x * transform.right;

        if(isSprinting && move == Vector3.zero){
            isSprinting = false;
            speed = speed / sprintMultiplier;
        }

        if(isGrounded && Input.GetKeyDown(KeyCode.LeftShift)){
            if(isCrouching){
                isCrouching = false;
                speed = speed / crouchMultiplier;
                groundCheck.position = new Vector3(groundCheck.position.x, groundCheck.position.y - 0.4f, groundCheck.position.z);
            }
            if(isSprinting){
                isSprinting = false;
                speed = speed/sprintMultiplier;
            }else{
                isSprinting = true;
                speed = speed*sprintMultiplier;
            }
        }

        characterController.Move(move * speed * Time.deltaTime);

        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            if(isCrouching){
                isCrouching = false;
                speed = speed / crouchMultiplier;
                groundCheck.position = new Vector3(groundCheck.position.x, groundCheck.position.y - 0.4f, groundCheck.position.z);
            }else{
                groundVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        if(Input.GetKeyDown(crouchKey) && isGrounded){
            if(isSprinting){
                speed = speed/sprintMultiplier;
                isSprinting = false;
            }
            if(isCrouching){
                isCrouching = false;
                speed = speed / crouchMultiplier;
                groundCheck.position = new Vector3(groundCheck.position.x, groundCheck.position.y - 0.4f, groundCheck.position.z);
                // cam.transform.position = Vector3.Lerp()
            }else{
                isCrouching = true;
                
                speed = speed*crouchMultiplier;
                groundCheck.position = new Vector3(groundCheck.position.x, groundCheck.position.y + 0.4f, groundCheck.position.z);
            }
        }

        groundVelocity.y += gravity* Time.deltaTime;
        characterController.Move(groundVelocity *Time.deltaTime);
    }

    void ReduceHeight(){

    }
    void IncreaseHeight(){
        
    }
}
