using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDisconnect : MonoBehaviour
{
    public delegate void DisconnectRobotEvent();
    public static event DisconnectRobotEvent OnDisconnectRobot;

    public void HandleClickDisconnect()
    {
        OnDisconnectRobot?.Invoke();
    }

}
