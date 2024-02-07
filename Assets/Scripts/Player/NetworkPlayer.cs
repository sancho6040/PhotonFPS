using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set; }

    private void Start()
    {
        
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
            Debug.Log("Spawn local Player");

            Camera.main.gameObject.SetActive(false);
        }
        else
        {
            //deactivate other player components
            Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;

            AudioListener audiolistener = GetComponentInChildren<AudioListener>();
            audiolistener.enabled = false;

            Debug.Log("Spawn remote Player");
        }

        transform.name = $"Player_{Object.Id}";
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player.IsValid == Object.HasInputAuthority)
        {
            Runner.Despawn(Object);
        }
    }

}
