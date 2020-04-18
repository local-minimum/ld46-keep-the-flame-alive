using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
                dead = true;
                transform.position = heading.transform.position;
                break;
        }

        steps--;
        if (steps > 0 && !dead) MoveForward(steps, nextCommandInSeconds);
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
        }
        transform.LookAt(heading.transform, Vector3.up);
    }
}
