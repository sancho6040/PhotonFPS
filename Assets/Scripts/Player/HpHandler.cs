using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))]
    byte HP { get; set; }

    [Networked(OnChanged = nameof(OnStateChanged))]
    public bool isDead { get; set; }

    bool isInitialized = false;

    const byte startHP = 5;

    public Color uiHitColor;
    public Image uiHitImage;

    public MeshRenderer bodyMeshRenderer;
    Color defaultMeshBodyColor;

    public GameObject playerModel;
    public GameObject deathGameobjectPrefab;

    HitboxRoot hitboxRoot;

    CharacterMovementHandler characterMovementHandler;

    private void Awake()
    {
        hitboxRoot = GetComponentInChildren<HitboxRoot>();
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
    }

    private void Start()
    {
        HP = startHP;
        isDead = false;

        defaultMeshBodyColor = bodyMeshRenderer.material.color;

        isInitialized = true;
    }

    IEnumerator onHitCO()
    {
        bodyMeshRenderer.material.color = Color.white;

        if (Object.HasInputAuthority)
        {
            uiHitImage.color = uiHitColor;
        }

        yield return new WaitForSeconds(0.2f);

        bodyMeshRenderer.material.color = defaultMeshBodyColor;

        if (Object.HasInputAuthority && !isDead)
        {
            uiHitImage.color = new Color(0, 0, 0, 0);
        }
    }

    IEnumerator ServerReviveCO()
    {
        yield return new WaitForSeconds(2.0f);

        characterMovementHandler.RequestRespawn();
    }

    //function only called on the server
    public void OnTakeDamage()
    {
        if (isDead) return;

        HP -= 1;

        if (HP <= 0)
        {
            StartCoroutine(ServerReviveCO());
            isDead = true;
        }
    }


    static void OnHPChanged(Changed<HpHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.HP}");

        //method to access non static funtions
        byte newHP = changed.Behaviour.HP;

        changed.LoadOld();
        byte oldHP = changed.Behaviour.HP;

        if (newHP < oldHP)
        {
            changed.Behaviour.OnHPReduced();
        }
    }

    private void OnHPReduced()
    {
        if (!isInitialized) return;

        StartCoroutine(onHitCO());
    }

    static void OnStateChanged(Changed<HpHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged value {changed.Behaviour.isDead}");

        //method to access non static funtions
        bool isDeadCurrent = changed.Behaviour.isDead;

        changed.LoadOld();
        bool isDeadOld = changed.Behaviour.isDead;

        if (isDeadCurrent)
        {
            changed.Behaviour.OnDeath();
        }
        else if (!isDeadCurrent && isDeadOld)
        {
            changed.Behaviour.OnRevive();
        }
    }

    private void OnDeath()
    {
        playerModel.gameObject.SetActive(false);
        hitboxRoot.HitboxRootActive = false;
        characterMovementHandler.SerCharacterControllerEnabled(false);

        Instantiate(deathGameobjectPrefab, transform.position, Quaternion.identity);
    }
    private void OnRevive()
    {
        if(Object.HasInputAuthority)
        {
            uiHitImage.color = new Color(0, 0, 0, 0);
        }

        playerModel.gameObject.SetActive(true);
        hitboxRoot.HitboxRootActive = true;
        characterMovementHandler.SerCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        //reset variables
        HP = startHP;
        isDead = false;
    }
}
