using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFire : MonoBehaviour
{
    [SerializeField] bool startOnFire = false;

    private void Start()
    {
        var ps = GetComponentInChildren<ParticleSystem>();
        var emission = ps.emission;
        emission.enabled = startOnFire;
    }

    public void StartFire()
    {
        var ps = GetComponentInChildren<ParticleSystem>();
        var emission = ps.emission;
        emission.enabled = true;
    }
}
