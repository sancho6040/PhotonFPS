using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInputHandler : MonoBehaviour
{
    Vector2 moveInputVector = Vector2.zero;
    Vector2 viewInputVector = Vector2.zero;
    bool isJumpButtonPressed;
    bool isFireButtonPressed;


    LocalCameraHandler localCameraHandler;
    CharacterMovementHandler characterMovementHandler;

    private void Awake()
    {
        localCameraHandler = GetComponentInChildren<LocalCameraHandler>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!characterMovementHandler.Object.HasInputAuthority) return;


        //input player Rotation
        viewInputVector.x = Input.GetAxis("Mouse X");
        viewInputVector.y = Input.GetAxis("Mouse Y") * -1;

        //input player movement
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if(Input.GetButton("Jump"))
        {
            isJumpButtonPressed = true;
        }

        if (Input.GetButton("Fire1"))
        {
            isFireButtonPressed = true;
        }

        //set view rotation
        localCameraHandler.SetViewInputVector(viewInputVector);
    }

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        //view data
        networkInputData.aimForwardVector = localCameraHandler.transform.forward;
        //Move data
        networkInputData.movementInput = moveInputVector;
        //Jump data
        networkInputData.isJumpPressed = isJumpButtonPressed;
        //Fire data
        networkInputData.isFirePressed = isFireButtonPressed;

        //reset values
        isJumpButtonPressed = false;
        isFireButtonPressed = false;


        return networkInputData;
    }
}
