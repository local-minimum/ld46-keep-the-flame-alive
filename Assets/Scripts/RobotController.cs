using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RobotController : MonoBehaviour
{
    [SerializeField] Tile spawnTile;
    Tile tile;
    TileEdge heading;
    bool dead = false;

    // Start is called before the first frame update
    void Start()
    {
        tile = spawnTile;        
        heading = spawnTile.SpawnHeading;
        EndWalk();
    }
    
    private void OnEnable()
    {
        RemoteController.OnSendCommand += RemoteController_OnSendCommand;
    }

    private void OnDisable()
    {
        RemoteController.OnSendCommand -= RemoteController_OnSendCommand;
    }

    void EndWalk()
    {
        transform.position = tile.RestPosition.position;
        transform.LookAt(heading.transform, Vector3.up);
    }

    void WalkOverEdge(Transform edge)
    {
        dead = true;
        Vector3 impulse = edge.position - transform.position;
        transform.position = edge.transform.position;
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(impulse, ForceMode.Impulse);
    }

    private void MoveForward(int steps, float nextCommandInSeconds)
    {
        switch (heading.ExitMode)
        {
            case TileEdgeMode.Block:
                //TODO move and retreat.
                heading.BumpConnected();
                break;
            case TileEdgeMode.Allow:
                tile = heading.ConnectedTile;
                heading = heading.HeadingAfterPassing;
                EndWalk();
                break;
            case TileEdgeMode.Fall:
                WalkOverEdge(heading.transform);
                break;
        }

        steps--;
        if (steps > 0 && !dead) MoveForward(steps, nextCommandInSeconds);
    }

    private void MoveBackward(int steps, float nextCommandInSeconds)
    {
        TileEdge reverseHeading = tile.Backward(heading);
        switch (reverseHeading.ExitMode)
        {
            case TileEdgeMode.Block:
                //TODO: move and retreat.
                // we don't bump since fire is infront of us
                break;
            case TileEdgeMode.Allow:
                tile = reverseHeading.ConnectedTile;
                heading = tile.Backward(reverseHeading.HeadingAfterPassing);
                EndWalk();
                break;
            case TileEdgeMode.Fall:
                WalkOverEdge(reverseHeading.transform);
                break;
                
        }
        steps--;
        if (steps > 0 && !dead) MoveBackward(steps, nextCommandInSeconds);
    }

    private void RemoteController_OnSendCommand(RobotCommand command, float nextCommandInSeconds)
    {
        if (dead) return;
        switch (command)
        {
            case RobotCommand.TurnLeftTwo:
                heading = tile.Left(heading);
                heading = tile.Left(heading);
                break;
            case RobotCommand.TurnLeft:
                heading = tile.Left(heading);
                break;
            case RobotCommand.TurnRightTwo:
                heading = tile.Right(heading);
                heading = tile.Right(heading);
                break;
            case RobotCommand.TurnRight:
                heading = tile.Right(heading);
                break;
            case RobotCommand.ForwardOne:
                MoveForward(1, nextCommandInSeconds);
                break;
            case RobotCommand.ForwardTwo:
                MoveForward(2, nextCommandInSeconds);
                break;
            case RobotCommand.ForwardThree:
                MoveForward(3, nextCommandInSeconds);
                break;
            case RobotCommand.BackupOne:
                MoveBackward(1, nextCommandInSeconds);
                break;
            case RobotCommand.BackupTwo:
                MoveBackward(2, nextCommandInSeconds);
                break;
            case RobotCommand.NONE:
                //TODO: Idle!
                break;
        }
        transform.LookAt(heading.transform, Vector3.up);
    }
}
