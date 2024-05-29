using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity;

    [SerializeField] private Transform playerOrientation;

    private float cameraRotationX;
    private float cameraRotationY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        //get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivity * 0.01f;// * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivity * 0.01f;// * Time.deltaTime;

        cameraRotationY += mouseX;
        cameraRotationX -= mouseY;

        cameraRotationX = Mathf.Clamp(cameraRotationX, -90f, 90f);
    }


    private void LateUpdate()
    {
        //rotate camera and orientation
        transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
        playerOrientation.rotation = Quaternion.Euler(0, cameraRotationY, 0);
    }


}
