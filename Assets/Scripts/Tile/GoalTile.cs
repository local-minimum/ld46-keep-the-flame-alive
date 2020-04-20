using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTile : MonoBehaviour
{
    public delegate void GoalEvent();
    public static event GoalEvent OnGoalReached;

    [SerializeField] ParticleSystem firstFire;
    [SerializeField] ParticleSystem secondFire;
    [SerializeField] ParticleSystem thirdFire;
    [SerializeField] ParticleSystem[] circles;

    private void Start()
    {
        var emission = firstFire.emission;
        emission.enabled = false;
        emission = secondFire.emission;
        emission.enabled = false;
        emission = thirdFire.emission;
        emission.enabled = false; 
        for (int i =0; i<circles.Length;i++)
        {
            emission = circles[i].emission;
            emission.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<RobotController>())
        {
            OnGoalReached?.Invoke();
            StartCoroutine(Outro());
        }
    }

    IEnumerator<WaitForSeconds> Outro()
    {
        var emisson = firstFire.emission;
        emisson.enabled = true;
        yield return new WaitForSeconds(0.5f);
        emisson = secondFire.emission;
        emisson.enabled = true;
        yield return new WaitForSeconds(0.5f);
        emisson = thirdFire.emission;
        emisson.enabled = true;
        yield return new WaitForSeconds(1f);
        for (int i = 0; i<circles.Length; i++)
        {
            emisson = circles[i].emission;
            emisson.enabled = true;
        }
    }
}
