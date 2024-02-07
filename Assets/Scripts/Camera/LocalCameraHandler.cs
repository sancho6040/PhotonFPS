using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    //this script move and rotate the camera to the player, it helps to remove jumping camera because of lag
    public Transform cameraAnchorPoint;

    Vector2 viewInput = Vector2.zero;

    float cameraRotationX = 0f;
    float cameraRotationY = 0f;


    Camera localCamera;
    NetworkCharacterControllerPrototypeCustom networkCharacterController;

    private void Awake()
    {
        localCamera = GetComponent<Camera>();
        networkCharacterController = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
    }

    private void Start()
    {
        if (localCamera.enabled)
        {
            localCamera.transform.parent = null;
        }
    }
    private void LateUpdate()
    {
        if (cameraAnchorPoint == null && localCamera.enabled) return;


        //move the camera to the position of the player
        localCamera.transform.position = cameraAnchorPoint.position;

        //rotate camera
        cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterController.viewRotationSpeedY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterController.rotationSpeed;

        localCamera.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0f);

    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }
}
