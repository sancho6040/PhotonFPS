using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalCameraHandler : MonoBehaviour
{
    //this script move and rotate the camera to the player, it helps to remove jumping camera because of lag
    public Transform cameraAnchorPoint;
    public Camera localCamera;
    public GameObject localGun;

    //Input
    Vector2 viewInput;

    //Rotation
    float cameraRotationX = 0;
    float cameraRotationY = 0;

    //Other components
    NetworkCharacterControllerPrototypeCustom networkCharacterControllerPrototypeCustom;
    CinemachineVirtualCamera cinemachineVirtualCamera;


    private void Awake()
    {
        localCamera = GetComponent<Camera>();
        networkCharacterControllerPrototypeCustom = GetComponentInParent<NetworkCharacterControllerPrototypeCustom>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Detach camera if enabled
        if (localCamera.enabled)
            localCamera.transform.parent = null;
    }

    void LateUpdate()
    {
        if (cameraAnchorPoint == null)
            return;

        if (!localCamera.enabled)
            return;

        //Find the Chinemachine camera if we haven't already. 
        if (cinemachineVirtualCamera == null)
            cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        else
        {
            if (NetworkPlayer.Local.is3rdPersonCamera)
            {
                if (!cinemachineVirtualCamera.enabled)
                {
                    cinemachineVirtualCamera.Follow = NetworkPlayer.Local.playerModel;
                    cinemachineVirtualCamera.LookAt = NetworkPlayer.Local.playerModel;
                    cinemachineVirtualCamera.enabled = true;

                    //Sets the layer of the local players model
                    Utils.SetRenderLayersInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("Default"));

                    //Disable the local gun
                    localGun.SetActive(false);
                }

                //Let the camer be handled by cinemachine
                return;
            }
            else
            {
                if (cinemachineVirtualCamera.enabled)
                {
                    cinemachineVirtualCamera.enabled = false;

                    //Sets the layer of the local players model
                    Utils.SetRenderLayersInChildren(NetworkPlayer.Local.playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

                    //Enable the local gun
                    //localGun.SetActive(true);
                }
            }
        }

        //Move the camera to the position of the player
        localCamera.transform.position = cameraAnchorPoint.position;

        //Calculate rotation
        cameraRotationX += viewInput.y * Time.deltaTime * networkCharacterControllerPrototypeCustom.viewUpDownRotationSpeed;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        cameraRotationY += viewInput.x * Time.deltaTime * networkCharacterControllerPrototypeCustom.rotationSpeed;

        //Apply rotation
        localCamera.transform.rotation = Quaternion.Euler(cameraRotationX, cameraRotationY, 0);

    }
    public void SetViewInputVector(Vector2 viewInput)
    {
        this.viewInput = viewInput;
    }
}
