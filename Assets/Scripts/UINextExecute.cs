using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UINextExecute : MonoBehaviour
{
    private void OnEnable()
    {
        RemoteController.OnSendCommand += RemoteController_OnSendCommand;
    }

    private void OnDisable()
    {
        RemoteController.OnSendCommand -= RemoteController_OnSendCommand;
    }

    IEnumerator<WaitForSeconds> Progress(float nextCommandInSeconds)
    {
        Image img = GetComponent<Image>();
        float start = Time.timeSinceLevelLoad;
        float delta = 0;
        while (delta < nextCommandInSeconds)
        {
            delta = Time.timeSinceLevelLoad - start;
            img.fillAmount = delta / nextCommandInSeconds;
            yield return new WaitForSeconds(0.02f);
        }
        img.fillAmount = 1;
    }


    private void RemoteController_OnSendCommand(RobotCommand command, float nextCommandInSeconds)
    {
        StartCoroutine(Progress(nextCommandInSeconds));
    }
}
