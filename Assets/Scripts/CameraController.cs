using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] RobotController robot;
    Vector3 offset;
    Vector3 velocity;
    float minY;
    [SerializeField] float smoothTime = 0.5f; 
    private void Start()
    {
        minY = transform.position.y;
        offset = transform.position - robot.transform.position;
    }

    void LateUpdate()
    {
        Vector3 target = robot.transform.position + offset;
        target.y = Mathf.Max(target.y, minY);
        transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, smoothTime);
        transform.LookAt(robot.transform);
    }
}
