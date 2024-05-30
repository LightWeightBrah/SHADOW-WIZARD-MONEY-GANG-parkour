using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity;

    [SerializeField] private Transform playerOrientation;
    [SerializeField] private Transform cameraHolder;

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
        cameraHolder.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);
        playerOrientation.rotation = Quaternion.Euler(0, cameraRotationY, 0);
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }

}
