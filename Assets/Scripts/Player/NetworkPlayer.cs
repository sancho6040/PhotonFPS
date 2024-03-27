using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set; }

    public TextMeshProUGUI playerNickNameTM;
    public Transform playerModel;

    [Networked(OnChanged = nameof(OnNickNameChanged))]
    public NetworkString<_16> nickName { get; set; }

    bool isPublicJoinMessageSent = false;

    public LocalCameraHandler localCameraHandler;
    public GameObject localUI;
    public GameObject nicknameUI;


    //Camera mode
    public bool is3rdPersonCamera { get; set; } = true;

    //Other components
    NetworkInGameMessages networkInGameMessages;

    void Awake()
    {
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;

            //Sets the layer of the local players model
            Utils.SetRenderLayersInChildren(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));

            //Disable main camera
            Camera.main.gameObject.SetActive(false);
            nicknameUI.SetActive(false);

            RPC_SetNickName(PlayerPrefs.GetString("PlayerNickname"));

            Debug.Log("Spawned local player");
        }
        else
        {
            //Disable the camera if we are not the local player
            Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;

            //Only 1 audio listner is allowed in the scene so disable remote players audio listner
            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;

            //Disable UI for remote player
            localUI.SetActive(false);

            Debug.Log("Spawned remote player");
        }

        //Set the player as a player object
        Runner.SetPlayerObject(Object.InputAuthority, Object);

        //Make it easier to tell which player is which.
        transform.name = $"P_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Object.HasStateAuthority)
        {
            if (Runner.TryGetPlayerObject(player, out NetworkObject playerLeftNetworkObject))
            {
                if (playerLeftNetworkObject == Object)
                    Local.GetComponent<NetworkInGameMessages>().SendInGameRPCMessage(playerLeftNetworkObject.GetComponent<NetworkPlayer>().nickName.ToString(), "left");
            }

        }


        if (player == Object.InputAuthority)
            Runner.Despawn(Object);

    }
    static void OnNickNameChanged(Changed<NetworkPlayer> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.nickName}");

        changed.Behaviour.OnNickNameChanged();
    }

    private void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickName} for player {gameObject.name}");

        playerNickNameTM.text = nickName.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickName, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickName}");
        this.nickName = nickName;

        if (!isPublicJoinMessageSent)
        {
            networkInGameMessages.SendInGameRPCMessage(nickName, "joined");

            isPublicJoinMessageSent = true;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetCameraMode(bool is3rdPersonCamera, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetCameraMode. is3rdPersonCamera  {is3rdPersonCamera}");

        this.is3rdPersonCamera = is3rdPersonCamera;
    }
}
