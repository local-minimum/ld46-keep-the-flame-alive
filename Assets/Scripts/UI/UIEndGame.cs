using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIEndGame : MonoBehaviour
{
    private void OnEnable()
    {
        GoalTile.OnGoalReached += GoalTile_OnGoalReached;
    }

    private void OnDisable()
    {
        GoalTile.OnGoalReached -= GoalTile_OnGoalReached;
    }

    private void Start()
    {
        for (int i = 0, l = transform.childCount; i<l; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void GoalTile_OnGoalReached()
    {
        StartCoroutine(Victory());
    }

    IEnumerator<WaitForSeconds> Victory()
    {
        yield return new WaitForSeconds(6f);
        for (int i = 0, l = transform.childCount; i < l; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void HandleClickMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
