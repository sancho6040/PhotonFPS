using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnFireChanged))]
    public bool isFiring { get; set; }

    public ParticleSystem fireParticleSystem;
    public Transform aimPoint;
    public LayerMask collisionLayer;

    float lastTimeFired = 0f;

    HpHandler HpHandler;

    private void Awake()
    {
        HpHandler = GetComponent<HpHandler>();
    }

    public override void FixedUpdateNetwork()
    {
        if(HpHandler.isDead)
        {
            return;
        }

        //get input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            if (networkInputData.isFirePressed)
            {
                Fire(networkInputData.aimForwardVector);
            }
        }
    }

    void Fire(Vector3 aimForwardVector)
    {
        if (Time.time - lastTimeFired < 0.15f) return;

        StartCoroutine(FireEffectCO());

        //hit scan
        Runner.LagCompensation.Raycast(aimPoint.position, aimForwardVector, 100f, Object.InputAuthority, out var hitInfo, collisionLayer, HitOptions.IncludePhysX);
        float hitDistance = 100;
        bool isHitOtherPlayer = false;

        if (hitInfo.Distance > 0) hitDistance = hitInfo.Distance;

        if (hitInfo.Hitbox != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit hitbox {hitInfo.Hitbox.transform.root.name}");

            if(Object.HasInputAuthority)
            {
                hitInfo.Hitbox.transform.root.GetComponent<HpHandler>().OnTakeDamage();
            }

            isHitOtherPlayer = true;
        }
        else if (hitInfo.Collider != null)
        {
            Debug.Log($"{Time.time} {transform.name} hit physX collider {hitInfo.Collider.transform.name}");

        }

        //debug
        if (isHitOtherPlayer)
        {
            Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.red, 1f);
        }
        else Debug.DrawRay(aimPoint.position, aimForwardVector * hitDistance, Color.green, 1f);

        lastTimeFired = Time.time;
    }

    IEnumerator FireEffectCO()
    {
        isFiring = true;
        fireParticleSystem.Play();

        yield return new WaitForSeconds(0.09f);

        isFiring = false;
    }

    static void OnFireChanged(Changed<WeaponHandler> changed)
    {
        //Debug.Log($"{Time.time} OnFireChanged value {changed.Behaviour.isFiring}");

        bool isFiringCurrent = changed.Behaviour.isFiring;

        //load the old state
        changed.LoadOld();

        bool isFiringOld = changed.Behaviour.isFiring;

        if (isFiringCurrent && !isFiringOld)
        {
            changed.Behaviour.OnFireRemote();
        }
    }

    void OnFireRemote()
    {
        if (!Object.HasInputAuthority)
        {
            fireParticleSystem.Play();
        }
    }
}
