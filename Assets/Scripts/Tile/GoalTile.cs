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

    private void Start()
    {
        var emisson = firstFire.emission;
        emisson.enabled = false;
        emisson = secondFire.emission;
        emisson.enabled = false;
        emisson = thirdFire.emission;
        emisson.enabled = false;            
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
    }
}
