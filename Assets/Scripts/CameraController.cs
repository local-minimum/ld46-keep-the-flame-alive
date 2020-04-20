using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    RobotController robot;
    RobotFactory factory;

    [SerializeField] Vector3 offset;
    Vector3 velocity;
    float minY;
    [SerializeField] float smoothTime = 0.5f;

    [SerializeField] Vector3 LookAtOffset = Vector3.up * 0.5f;

    private void Start()
    {
        minY = transform.position.y;
    }

    private void OnEnable()
    {
        RobotFactory.OnSpawnRobot += RobotFactory_OnSpawnRobot;
        RobotFactory.OnActivateFactory += RobotFactory_OnActivateFactory;
    }

    private void OnDisable()
    {
        RobotFactory.OnSpawnRobot -= RobotFactory_OnSpawnRobot;
        RobotFactory.OnActivateFactory -= RobotFactory_OnActivateFactory;
    }

    private void RobotFactory_OnActivateFactory(RobotFactory factory)
    {        
        this.factory = factory;
        this.robot = null;
    }

    private void RobotFactory_OnSpawnRobot(RobotController robot)
    {
        this.robot = robot;
    }

    void LateUpdate()
    {
        if (robot != null)
        {
            Vector3 target = robot.transform.position + offset;
            target.y = Mathf.Max(target.y, minY);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
            transform.LookAt(robot.transform.position + LookAtOffset);
        }
        else if (factory != null)
        {
            Vector3 target = factory.transform.position + offset;
            target.y = Mathf.Max(target.y, minY);
            transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
            transform.LookAt(factory.transform.position + LookAtOffset);
        }
    }
}
