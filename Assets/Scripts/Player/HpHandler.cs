using Fusion;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))]
    float HP { get; set; }
    const float startingHP = 5;


    [Networked(OnChanged = nameof(OnStateChanged))]
    public bool isDead { get; set; }

    bool isInitialized = false;

    public Color uiOnHitColor;
    public Image uiOnHitImage;

    public MeshRenderer bodyMeshRenderer;
    Color defaultMeshBodyColor;

    public GameObject playerModel;
    public GameObject deathGameObjectPrefab;

    //Other components
    HitboxRoot hitboxRoot;
    CharacterMovementHandler characterMovementHandler;
    NetworkInGameMessages networkInGameMessages;
    NetworkPlayer networkPlayer;

    //--------------------------------------------------
    [Header("Barra de vida")]

    public Slider hpSlider;
    public Image hpColor;
    private float lerpSpeed;


    private void Awake()
    {
        characterMovementHandler = GetComponent<CharacterMovementHandler>();
        hitboxRoot = GetComponentInChildren<HitboxRoot>();
        networkInGameMessages = GetComponent<NetworkInGameMessages>();
        networkPlayer = GetComponent<NetworkPlayer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        HP = startingHP;
        isDead = false;

        defaultMeshBodyColor = bodyMeshRenderer.material.color;

        restartHealthUI();

        isInitialized = true;
    }
    private void Update()
    {
        lerpSpeed = 3f * Time.deltaTime;

        hpSlider.value = Mathf.Lerp(hpSlider.value, HP / startingHP, lerpSpeed);

        Color nuevoColor = Color.Lerp(Color.red, Color.green, (HP / startingHP));
        hpColor.color = nuevoColor;
    }

    IEnumerator OnHitCO()
    {
        bodyMeshRenderer.material.color = Color.white;

        if (Object.HasInputAuthority)
            uiOnHitImage.color = uiOnHitColor;

        yield return new WaitForSeconds(0.2f);

        bodyMeshRenderer.material.color = defaultMeshBodyColor;

        if (Object.HasInputAuthority && !isDead)
            uiOnHitImage.color = new Color(0, 0, 0, 0);
    }

    IEnumerator ServerReviveCO()
    {
        yield return new WaitForSeconds(2.0f);

        characterMovementHandler.RequestRespawn();
    }


    //Function only called on the server
    public void OnTakeDamage(string damageCausedByPlayerNickname, float damageAmount)
    {
        //Only take damage while alive
        if (isDead)
            return;

        if (damageAmount > HP)
            damageAmount = HP;

        HP -= damageAmount;

        Debug.Log($"{Time.time} {transform.name} took damage got {HP} left ");

        //Player died
        if (HP <= 0)
        {
            networkInGameMessages.SendInGameRPCMessage(damageCausedByPlayerNickname, $"Killed <b>{networkPlayer.nickName.ToString()}</b>");

            Debug.Log($"{Time.time} {transform.name} died");

            StartCoroutine(ServerReviveCO());

            isDead = true;
        }
    }

    static void OnHPChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnHPChanged value {changed.Behaviour.HP}");

        float newHP = changed.Behaviour.HP;

        //Load the old value
        changed.LoadOld();

        float oldHP = changed.Behaviour.HP;

        //Check if the HP has been decreased
        if (newHP < oldHP)
            changed.Behaviour.OnHPReduced();
    }

    private void OnHPReduced()
    {
        if (!isInitialized)
            return;

        StartCoroutine(OnHitCO());
    }

    static void OnStateChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged isDead {changed.Behaviour.isDead}");

        bool isDeadCurrent = changed.Behaviour.isDead;

        //Load the old value
        changed.LoadOld();

        bool isDeadOld = changed.Behaviour.isDead;

        //Handle on death for the player. Also check if the player was dead but is now alive in that case revive the player.
        if (isDeadCurrent)
            changed.Behaviour.OnDeath();
        else if (!isDeadCurrent && isDeadOld)
            changed.Behaviour.OnRevive();
    }

    private void OnDeath()
    {
        Debug.Log($"{Time.time} OnDeath");

        playerModel.gameObject.SetActive(false);
        hitboxRoot.HitboxRootActive = false;
        characterMovementHandler.SetCharacterControllerEnabled(false);

        Instantiate(deathGameObjectPrefab, transform.position, Quaternion.identity);
    }

    private void OnRevive()
    {
        Debug.Log($"{Time.time} OnRevive");

        if (Object.HasInputAuthority)
            uiOnHitImage.color = new Color(0, 0, 0, 0);

        playerModel.gameObject.SetActive(true);
        hitboxRoot.HitboxRootActive = true;
        characterMovementHandler.SetCharacterControllerEnabled(true);
    }

    public void OnRespawned()
    {
        //Reset variables
        HP = startingHP;
        isDead = false;

        restartHealthUI();
    }

    public void restartHealthUI()
    {
        hpSlider.value = HP / startingHP;
        hpColor.color = Color.green;
    }
}
