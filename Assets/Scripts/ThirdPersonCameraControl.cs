﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCameraControl : AbstractCamera
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
     public float zoomValue = 1f;
     public float shoulderOffset = 0f; // neg for left
     public float heightOffset = 0f;
    public float shakeDuration = 0.5f;
    public float shakeMagitude = 0.5f;
    public float transformRange = 1f;
    public bool controlPlayerRotation = true;
    public KeyCode testShakeKey = KeyCode.P;
     float rotationYAxis = 0.0f;
     float rotationXAxis = 0.0f;
     float velocityX = 0.0f;
     float velocityY = 0.0f;
     float currentZoom = 0f;

     float distanceOffset;
     bool isShaking= false;
     float shakeElapsed = 0f;
     float shakeX;
     float shakeY;
     Vector3 shakePos = Vector3.zero;
     // Use this for initialization
     void Start()
     {
         Vector3 angles = transform.eulerAngles;
         rotationYAxis = angles.y;
         rotationXAxis = angles.x;
         if(target == null){
         
            GameObject gObject = GameObject.FindWithTag("Player");
            if(gObject != null){
                Debug.Log("found player");
                target = gObject.transform;
            }
         }
         // Make the rigid body not change rotation
     }
     void Update(){
        if (Input.GetKeyDown(testShakeKey) && isShaking == false)
        {
            isShaking = true;
            shakeX = transform.position.x;
            shakeY = transform.position.y;
        }
     }
     void FixedUpdate(){
        //Debug.DrawRay(transform.position, target.position, Color.cyan, 5, false);
        Vector3 relativePos = transform.position - (target.position);
        
        RaycastHit hit;
        if (Physics.Raycast(target.position, relativePos,  out hit, distance))
        {
            // Debug.DrawLine(target.position, hit.point);
            // distance -= hit.distance;
            // //distance = Mathf.Clamp(distance, 0, distance);
            // Debug.Log("raycast");
            
            distanceOffset = distance - hit.distance;
            distanceOffset = Mathf.Clamp(distanceOffset, 0, distance);
            if (hit.transform.tag == "Player")
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
             if(Input.GetMouseButton(1)){
                 currentZoom = Mathf.MoveTowards(currentZoom, zoomValue, Time.deltaTime/0.2f);
             }else{
                currentZoom = Mathf.MoveTowards(currentZoom, 0f, Time.deltaTime/0.2f);
             }
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
            
             Vector3 negDistance = new Vector3(shoulderOffset, heightOffset, -distance+distanceOffset+currentZoom);
             Vector3 position = rotation * negDistance + target.position;
 
             transform.rotation = rotation;
             if(isShaking){
                 cameraShake(position);
             }else{
                transform.position = position;
             }
             if(controlPlayerRotation){
                target.transform.Rotate(Vector3.up * velocityX);
             }
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
     public override void CameraController(){

     }

     void cameraShake(Vector3 position){
        if (shakePos == Vector3.zero)
        {
            shakePos = position;
        }
        if (shakePos.x != shakeX)
        {
            shakePos.x = Mathf.MoveTowards(shakePos.x, shakeX, Time.deltaTime / 0.1f);
        }
        else
        {
            shakeX = position.x + Random.Range(-transformRange, transformRange) * shakeMagitude;
        }
        if (shakePos.y != shakeY)
        {
            shakePos.y = Mathf.MoveTowards(shakePos.y, shakeY, Time.deltaTime / 0.1f);
        }
        else
        {
            shakeY = position.y + Random.Range(-transformRange, transformRange) * shakeMagitude;
        }

        transform.position = shakePos;

        if (shakeElapsed < shakeDuration)
        {
            shakeElapsed += Time.deltaTime;
        }
        else
        {
            shakeElapsed = 0f;
            isShaking = false;
            shakePos = Vector3.zero;
        }
     }
 }
