using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementHandler : NetworkBehaviour
{

    NetworkCharacterControllerPrototypeCustom characterController;
    Camera playerCamera;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out NetworkInputData networkInputData))
        {
            //rotate view
            transform.forward = networkInputData.aimForwardVector;

            //prevent the character from tilt
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(0, rotation.eulerAngles.y, rotation.eulerAngles.z);
            transform.rotation = rotation;

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

    void CheckfallRespawn()
    {
        if(transform.position.y < -12)
        {
            transform.position = Utils.GetRandomSpawnPoint();
        }
    }


}
