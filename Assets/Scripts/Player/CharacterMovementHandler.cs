using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementHandler : NetworkBehaviour
{

    NetworkCharacterControllerPrototypeCustom characterController;
    HpHandler hpHandler;

    bool isSpawnedRequested = false;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototypeCustom>();
        hpHandler = GetComponent<HpHandler>();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority)
        {
            if( isSpawnedRequested) 
            {
                Respawn();
                return;
            }

            if (hpHandler.isDead) return;
        }

        //Get the input from the network
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
            if(Object.HasInputAuthority) 
            {
                Respawn();
            }
        }
    }

    public void RequestRespawn()
    {
        isSpawnedRequested = true;
    }

    void Respawn()
    {
        characterController.TeleportToPosition(Utils.GetRandomSpawnPoint());
        hpHandler.OnRespawned();
        isSpawnedRequested = false;
    }

    public void SerCharacterControllerEnabled(bool isEnabled)
    {
        characterController.Controller.enabled = isEnabled;
    }


}
