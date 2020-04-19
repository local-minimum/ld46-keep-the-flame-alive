﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : MonoBehaviour
{

    [SerializeField, Range(1, 5)] int maxFlame = 3;
    [SerializeField] int[] emissionRates;
    [SerializeField] float[] startSpeed;
    [SerializeField] float[] startLife;
    int flame;

    private void Start()
    {
        flame = maxFlame;
        SetFlameIntensity();
    }

    public bool Burning
    {
        get
        {
            return flame > 0;
        }
    }

    void SetFlameIntensity()
    {
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        var emission = ps.emission;
        if (flame == 0)
        {
            emission.enabled = false;
            return;
        } else
        {
            emission.rateOverTime = emissionRates[flame - 1];
        }


        var main = ps.main;
        main.startSpeedMultiplier = startSpeed[flame - 1];
        main.startLifetimeMultiplier = startLife[flame - 1];
    }

    public void Douse()
    {
        flame = 0;
        SetFlameIntensity();
    }

    public void Blow()
    {
        flame = Mathf.Max(0, flame - 1);
        SetFlameIntensity();
    }

    public void Inflame()
    {
        flame = Mathf.Min(maxFlame, flame + 1);
    }
}