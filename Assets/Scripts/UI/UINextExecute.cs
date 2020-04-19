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
        RobotController.OnRobotDeath += RobotController_OnRobotDeath;
        RobotFactory.OnSpawnRobot += RobotFactory_OnSpawnRobot;
    }

    private void OnDisable()
    {
        RemoteController.OnSendCommand -= RemoteController_OnSendCommand;
        RobotController.OnRobotDeath -= RobotController_OnRobotDeath;
        RobotFactory.OnSpawnRobot -= RobotFactory_OnSpawnRobot;
    }

    bool robotAlive = false;
    private void RobotController_OnRobotDeath(RobotController robot)
    {
        robotAlive = false;
        Image img = GetComponent<Image>();
        img.fillAmount = 0;
    }
    private void RobotFactory_OnSpawnRobot(RobotController robot)
    {
        robotAlive = true;
    }

    IEnumerator<WaitForSeconds> Progress(float nextCommandInSeconds)
    {
        Image img = GetComponent<Image>();
        float start = Time.timeSinceLevelLoad;
        float delta = 0;
        while (robotAlive && delta < nextCommandInSeconds)
        {
            delta = Time.timeSinceLevelLoad - start;
            img.fillAmount = delta / nextCommandInSeconds;
            yield return new WaitForSeconds(0.02f);
        }
        img.fillAmount = robotAlive ? 1 : 0;
    }


    private void RemoteController_OnSendCommand(RobotCommand command, float nextCommandInSeconds)
    {
        if (command == RobotCommand.NONE) return;
        StartCoroutine(Progress(nextCommandInSeconds));
    }
}
