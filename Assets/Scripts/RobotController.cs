using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        RemoteController.OnSendCommand += RemoteController_OnSendCommand;
    }

    private void OnDisable()
    {
        RemoteController.OnSendCommand -= RemoteController_OnSendCommand;
    }

    private void RemoteController_OnSendCommand(RobotCommand command, float nextCommandInSeconds)
    {
        Debug.Log(string.Format("Robot: {0}", command));
    }
}
