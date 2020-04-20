using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIIntro : MonoBehaviour
{
    public void HandlePlay()
    {
        SceneManager.LoadScene("Game");
    }
}
