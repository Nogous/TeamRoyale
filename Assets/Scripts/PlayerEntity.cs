﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public enum Power
{
    Shield,
    Time,
    Invisibility,
}

public class PlayerEntity : MonoBehaviour
{
    public Power powerType;
    public int healthPoint = 1;
    [HideInInspector]
    public float powerCooldown = 0f;

    private Action DoAction;
    private float defaultPlayerSpeed;
    private Vector3 defaultPlayerScale;

    [Header("Shield Power")]
    public Transform ShieldObj;
    public float shieldDuration = 2f;
    public float shieldCooldown = .1f;
    [Range(1f, 80f)]
    public float shieldMaxSise = 50f;
    public float shieldSpawnSpeed = 1f;
    public bool cooldownResetWhenBulletIsDetected = true;
    public float growthDurationForEachBulletDetected = 2f;
    public float stayGrowthDuration = 1f;
    public float shrinkDuration = 0.5f;
    [Range(1.001f, 2f)]
    public float maxGrowthMultiplicator = 1.3f;
    public int growthPhaseNumber = 3;

    private int activeGrowthPhasesNumber = 0;
    private int activeShrinkPhasesNumber = 0;
    private float shieldDurTime = 0f;
    private float growthDurTime = 0f;
    private float stayGrowthTime = 0f;
    private float shrinkDurTime = 0f;

    [Header("Time Power")]
    public bool alsoSlowDownPlayer = false;
    public bool alsoSpeedUpPlayer = true;
    [Range(0.001f, 0.999f)]
    public float slowSpeedPlayerMultiplicator = 0.9f;
    [Range(1.001f, 5f)]
    public float fastSpeedPlayerMultiplicator = 1.5f;
    [Range(0.001f, 0.999f)]
    public float slowSpeedMultiplicator = 0.5f;
    [Range(1.001f, 5f)]
    public float fastSpeedMultiplicator = 1.5f;
    public float slowDuration = 5f;
    public float slowCooldown = 5f;
    public float fastAfterSlowDuration = 5f;

    private float slowDurTime = 0f;
    private float fastDurTime = 0f;

    [Header("Invisibility Power")]
    public float invisibilityDuration = 3f;
    public float invisibilityCooldown = 6f;

    private float invisibilityDurTime = 0f;
    private float lerpTime = 1f;
    private bool isInvisible = false;
    public List<FieldOfView> enemies;

    private Slider cooldownBar;

    public Animator sab;

    private void Awake()
    {
        defaultPlayerSpeed = gameObject.GetComponent<PlayerMovement>().moveSpeed;
        defaultPlayerScale = transform.localScale;
    }
    private void Start()
    {
        enemies.AddRange(GameObject.FindObjectsOfType<FieldOfView>());
        cooldownBar = GetComponentInChildren<Slider>();
        DoAction = DoActionVoid;
        if (gameObject.name.Contains("shield"))
        {
            ShieldObj.localScale = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isInvisible)
        {
            lerpTime -= Time.deltaTime;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, lerpTime);
        }
        else
        {
            lerpTime += Time.deltaTime;
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, lerpTime);
        }

        if (!GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>().inPaused)
        {
            PowerInput();
            DoAction();
            GrowthPlayer();
            StayGrowth();
            ShrinkPlayer();
        }

        cooldownBar.value = - powerCooldown;

        if(cooldownBar.value == cooldownBar.maxValue)
        {
            cooldownBar.gameObject.SetActive(false);
        }
        else
        {
            cooldownBar.gameObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.name.Contains("shield") && collision.CompareTag("bullet") && GetComponentInChildren<Shield>().isTrigger)
        {
            if (cooldownResetWhenBulletIsDetected)
                gameObject.GetComponentInParent<PlayerEntity>().powerCooldown = 0f;
            GetComponentInChildren<Shield>().isTrigger = false;
        }
        else if (collision.CompareTag("bullet"))
            healthPoint -= collision.GetComponent<Bullet>().damage;
        else if (collision.CompareTag("Laser"))
            healthPoint = 0;
    }

    #region Powers
    public void DoActionVoid()
    {
    }

    public void PowerInput()
    {
        powerCooldown -= Time.unscaledDeltaTime;
        if (Input.GetMouseButtonDown(0) && powerCooldown <= 0f)
        {
            Debug.Log("Click Gauche");
            gameObject.GetComponent<PlayerMovement>().moveSpeed = defaultPlayerSpeed;

            switch (powerType)
            {
                case global::Power.Shield:
                    StartShield();
                    break;
                case global::Power.Time:
                    StartTime();
                    break;
                case global::Power.Invisibility:
                    StartInvisibility();
                    break;
                default:
                    break;
            }
        }
    }

    #region Shild Power
    public void StartShield()
    {
        SoundManager.instance.ShieldPop();
        powerCooldown = shieldCooldown;
        shieldDurTime = shieldDuration;
        if (activeGrowthPhasesNumber < growthPhaseNumber)
        {
            growthDurTime += growthDurationForEachBulletDetected;
            activeGrowthPhasesNumber++;
        }
        DoAction = DoShield;
    }

    public void DoShield()
    {
        shieldDurTime -= Time.deltaTime;
        if (shieldDurTime <= 0f)
        {
            ShieldObj.localScale = Vector3.zero;
            DoAction = DoActionVoid;
            return;
        }

        // grow shild
        ShieldObj.localScale += shieldSpawnSpeed * Vector3.one * Time.deltaTime * shieldMaxSise;
        if (ShieldObj.localScale.x >=shieldMaxSise)
        {
            ShieldObj.localScale = Vector3.one * shieldMaxSise;
        }
    }

    public void GrowthPlayer()
    {
        if (growthDurTime <= 0f || activeGrowthPhasesNumber == 0)
            return;

        growthDurTime -= Time.deltaTime;

        Vector3 scaleObjectif = defaultPlayerScale * (1 + ((maxGrowthMultiplicator - 1) / growthPhaseNumber * activeGrowthPhasesNumber));
        if (growthDurTime <= 0f)
        {
            growthDurTime = 0f;
            transform.localScale = scaleObjectif;
        }
        else
            transform.localScale = Vector3.Lerp(transform.localScale, scaleObjectif, Time.deltaTime / growthDurTime);
        
        if (transform.localScale.x >= scaleObjectif.x)
        {
            transform.localScale = scaleObjectif;
            activeGrowthPhasesNumber--;
            stayGrowthTime += stayGrowthDuration;
        }
    }

    public void StayGrowth()
    {
        if (stayGrowthTime <= 0f)
            return;

        stayGrowthTime -= Time.deltaTime;

        if (stayGrowthTime <= 0f)
        {
            stayGrowthTime = 0f;
            shrinkDurTime += shrinkDuration;
            activeShrinkPhasesNumber++;
        }
    }

    public void ShrinkPlayer()
    {
        if (shrinkDurTime <= 0f || activeShrinkPhasesNumber == 0)
            return;

        shrinkDurTime -= Time.deltaTime;

        if (shrinkDurTime <= 0f)
        {
            shrinkDurTime = 0f;
            transform.localScale = defaultPlayerScale;
        }
        else
            transform.localScale = Vector3.Lerp(transform.localScale, defaultPlayerScale, Time.deltaTime / shrinkDurTime);

        if (transform.localScale.x <= defaultPlayerScale.x)
        {
            transform.localScale = defaultPlayerScale;
            activeShrinkPhasesNumber--;
        }
    }
    #endregion Shild Power

    #region Time Power
    public void StartTime()
    {
        SoundManager.instance.SlowTime();
        sab.SetInteger("feedsab", 1);
        powerCooldown = slowCooldown;

        if (!alsoSlowDownPlayer)
            gameObject.GetComponent<PlayerMovement>().moveSpeed = defaultPlayerSpeed * (1 / slowSpeedMultiplicator);
        else
            gameObject.GetComponent<PlayerMovement>().moveSpeed = defaultPlayerSpeed * slowSpeedPlayerMultiplicator;
        Time.timeScale = slowSpeedMultiplicator;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        slowDurTime = slowDuration;
        DoAction = DoSlowTime;
    }

    public void DoSlowTime()
    {
        slowDurTime -= Time.unscaledDeltaTime;
        if (slowDurTime <= 0f)
        {
            if (!alsoSpeedUpPlayer)
                gameObject.GetComponent<PlayerMovement>().moveSpeed = defaultPlayerSpeed * (fastSpeedMultiplicator - 1);
            else
                gameObject.GetComponent<PlayerMovement>().moveSpeed = defaultPlayerSpeed * fastSpeedPlayerMultiplicator;
            Time.timeScale = fastSpeedMultiplicator;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            fastDurTime = fastAfterSlowDuration;
            SoundManager.instance.SpeedTime();
            sab.SetInteger("feedsab", 0);
            DoAction = DoFastTime;
        }
    }

    public void DoFastTime()
    {
        fastDurTime -= Time.unscaledDeltaTime;
        if (fastDurTime <= 0f)
        {
            gameObject.GetComponent<PlayerMovement>().moveSpeed = defaultPlayerSpeed;
            Time.timeScale = 1;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            DoAction = DoActionVoid;
        }
    }
    #endregion Time Power

    #region Invisibility Power
    public void StartInvisibility()
    {
        SoundManager.instance.Invisibility();
        powerCooldown = invisibilityCooldown;
        invisibilityDurTime = invisibilityDuration;
        lerpTime = 1f;
        isInvisible = true;
        GetComponent<PlayerMovement>().moveSpeed = -GetComponent<PlayerMovement>().moveSpeed;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.tag = "Untagged";
        for(int i = 0; i < enemies.Count; i++)
        {
            enemies[i].lookAt = false;
            enemies[i].GetComponentInParent<Turret>().enabled = false;
            enemies[i].GetComponentInParent<Patrol>().enabled = true;
            enemies[i].gameObject.transform.parent.GetChild(2).GetChild(0).gameObject.SetActive(false);
        }
        DoAction = DoInvisibility;
    }

    public void DoInvisibility()
    {
        invisibilityDurTime -= Time.deltaTime;
        if(invisibilityDurTime <= 0f)
        {
            SoundManager.instance.Visibility();
            lerpTime = 0;
            isInvisible = false;
            GetComponent<PlayerMovement>().moveSpeed = -GetComponent<PlayerMovement>().moveSpeed;
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            gameObject.tag = "Player";
            DoAction = DoActionVoid;
        }
    }
    #endregion Invisibility Power
    #endregion Powers
}
