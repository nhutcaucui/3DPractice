using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraControl : MonoBehaviour
{
    // Start is called before the first frame update
     public Transform target;
     public float distance = 2.0f;
     public float xSpeed = 5.0f;
     public float ySpeed = 5.0f;
     public float yMinLimit = -90f;
     public float yMaxLimit = 90f;
     public float distanceMin = 1f;
     public float distanceMax = 5f;
     public float smoothTime = 10;
     float rotationYAxis = 0.0f;
     float rotationXAxis = 0.0f;
     float velocityX = 0.0f;
     float velocityY = 0.0f;

     float distanceOffset;
     // Use this for initialization
     void Start()
     {
         Vector3 angles = transform.eulerAngles;
         rotationYAxis = angles.y;
         rotationXAxis = angles.x;
         // Make the rigid body not change rotation
     }
     void FixedUpdate(){
        //Debug.DrawRay(transform.position, target.position, Color.cyan, 5, false);
        Vector3 relativePos = transform.position - (target.position);
        
        RaycastHit hit;
        if (Physics.Raycast(target.position, relativePos,  out hit, distance-1f))
        {
            // Debug.DrawLine(target.position, hit.point);
            // distance -= hit.distance;
            // //distance = Mathf.Clamp(distance, 0, distance);
            // Debug.Log("raycast");
            
            distanceOffset = distance - hit.distance;
            distanceOffset = Mathf.Clamp(distanceOffset, 0, distance);
            if (hit.transform.tag == "Character")
            {
                distanceOffset = 0;
            }
        }
        else
        {
            distanceOffset = 0;
        }
        
     }
     void LateUpdate()
     {
         if (target)
         {
            //distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);
            //  if (Input.GetMouseButton(0))
            //  {
                 velocityX += xSpeed * Input.GetAxis("Mouse X") * distance * Time.deltaTime;
                 velocityY += ySpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
            //  }
             rotationYAxis += velocityX;
             rotationXAxis -= velocityY;
             rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);

             //Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
             Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
             Quaternion rotation = toRotation;
            
             Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance+distanceOffset);
             Vector3 position = rotation * negDistance + target.position;
 
             transform.rotation = rotation;
             transform.position = position;
            target.transform.Rotate(Vector3.up * velocityX);
             velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
             velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
             //Debug.Log(Time.deltaTime * smoothTime);
            // velocityX = 0f;
            // velocityY = 0f;
            //transform.LookAt(target);
         }
     }
     public static float ClampAngle(float angle, float min, float max)
     {
         if (angle < -360F)
             angle += 360F;
         if (angle > 360F)
             angle -= 360F;
         return Mathf.Clamp(angle, min, max);
     }
 }
