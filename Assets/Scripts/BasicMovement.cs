using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 10.0f;
    public float gravity = -10f;
    public float jumpHeight=2f;
    public float sprintMultiplier = 2f;
    public float crouchTime=0.5f;
    public float crouchMultiplier = 0.7f;
    public float crouchHeight = 1f;
    public float originalHeight = 2f;
    private float x;
    private float z;
    public GameObject cam;

    public CharacterController characterController;

    public LayerMask groundMask;
    public float groundCheckDistance = 0.4f;
    public Transform groundCheck;

    private bool isGrounded;
    private Vector3 groundVelocity;
    private bool isCrouching = false;
    private bool isSprinting = false;
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

        if(!isCrouching){
            if(characterController.height<originalHeight)
            characterController.height = Mathf.MoveTowards(characterController.height, originalHeight, Time.deltaTime / crouchTime);
        }else{
            if (characterController.height >crouchHeight)
            characterController.height = Mathf.MoveTowards(characterController.height, crouchHeight, Time.deltaTime / crouchTime);
        }

        z = Input.GetAxis("Vertical");
        x = Input.GetAxis("Horizontal");
        Vector3 move = z * transform.forward + x * transform.right;

        if(isSprinting && move == Vector3.zero){
            isSprinting = false;
            speed = speed / sprintMultiplier;
        }

        if(isGrounded && Input.GetKeyDown(KeyCode.LeftShift)){
            if(isCrouching){
                isCrouching = false;
                characterController.height = originalHeight;
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

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            if(isCrouching){
                isCrouching = false;
                characterController.height = originalHeight;
                speed = speed / crouchMultiplier;
                groundCheck.position = new Vector3(groundCheck.position.x, groundCheck.position.y - 0.4f, groundCheck.position.z);
            }else{
                groundVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        if(Input.GetKeyDown(KeyCode.C) && isGrounded){
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
