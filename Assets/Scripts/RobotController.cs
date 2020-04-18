using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotController : MonoBehaviour
{
    [SerializeField] Tile spawnTile;
    Tile tile;
    TileEdge facing;

    // Start is called before the first frame update
    void Start()
    {
        tile = spawnTile;
        transform.position = tile.RestPosition.position;
        facing = spawnTile.SpawnHeading;
        transform.LookAt(facing.transform, Vector3.up);
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
        switch (command)
        {
            case RobotCommand.TurnLeftTwo:
                facing = tile.Left(facing);
                facing = tile.Left(facing);
                break;
            case RobotCommand.TurnLeft:
                facing = tile.Left(facing);
                break;
            case RobotCommand.TurnRightTwo:
                facing = tile.Right(facing);
                facing = tile.Right(facing);
                break;
            case RobotCommand.TurnRight:
                facing = tile.Right(facing);
                break;
        }
        transform.LookAt(facing.transform, Vector3.up);
    }
}
