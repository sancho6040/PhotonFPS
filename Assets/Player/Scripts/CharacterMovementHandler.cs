using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementHandler : NetworkBehaviour
{
    Vector2 viewinput;

    float cameraRotationX = 0;

    NetworkCharacterControllerPrototypeCustom characterController;
    Camera playerCamera;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        cameraRotationX += viewinput.x * Time.deltaTime * characterController.viewRotationSpeedY;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData networkInputData))
        {
            //Rotate the view
            characterController.Rotate(networkInputData.rotationInput);

            //move
            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            characterController.Move(moveDirection);

            //jump
            if(networkInputData.isJumpPressed)
            {
                characterController.Jump();
            }
        }
    }

    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewinput = viewInput;
    }

}
