using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFire : MonoBehaviour
{
    [SerializeField] bool onFire = false;
    [SerializeField] ParticleSystem fire;
    [SerializeField] ParticleSystem smoke;

    private void Start()
    {
        SetParticles();
    }

    public void StartFire()
    {
        onFire = true;
        SetParticles();
    }

    void SetParticles()
    {
        var emission = fire.emission;
        emission.enabled = onFire;
        emission = smoke.emission;
        emission.enabled = !onFire;
    }
}
