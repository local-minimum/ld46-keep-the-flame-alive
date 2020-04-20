using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RobotController : MonoBehaviour
{
    public delegate void RobotDeathEvent(RobotController robot);
    public static event RobotDeathEvent OnRobotDeath;
    public delegate void RobotMessageEvent(string msg);
    public static event RobotMessageEvent OnRobotMessage;

    Flame flame;
    public bool FlameAlive
    {
        get
        {
            return flame.Burning;
        }
    }

    float moveAnimationDelta = 0.02f;

    Tile tile;
    TileEdge heading;
    bool dead = true;

    [SerializeField, Range(0, .95f)]
    float robotMoveTimeFraction = 0.8f;

    public void SetSpawn(Tile tile)
    {
        this.tile = tile;
        heading = tile.SpawnHeading;
        EndWalk();
        dead = false;
    }
    
    private void OnEnable()
    {
        flame = GetComponentInChildren<Flame>();
        RemoteController.OnSendCommand += RemoteController_OnSendCommand;
        RemoteController.OnRobotLost += RemoteController_OnRobotLost;
        GoalTile.OnGoalReached += GoalTile_OnGoalReached;
    }

    private void OnDisable()
    {
        RemoteController.OnSendCommand -= RemoteController_OnSendCommand;
        RemoteController.OnRobotLost -= RemoteController_OnRobotLost;
        GoalTile.OnGoalReached -= GoalTile_OnGoalReached;
    }

    bool reachedGoal = false;
    private void GoalTile_OnGoalReached()
    {
        reachedGoal = true;
    }

    private void RemoteController_OnRobotLost()
    {
        if (!dead && !reachedGoal) WalkOverEdge(tile.transform, heading.transform.position - tile.transform.position);
    }

    void EndWalk()
    {
        transform.position = tile.RestPosition.position;
        transform.LookAt(heading.transform, Vector3.up);
    }

    void WalkOverEdge(Transform edge, Vector3 impulse)
    {
        dead = true;
        transform.position = edge.transform.position;
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(impulse, ForceMode.Impulse);
        OnRobotDeath?.Invoke(this);
        Destroy(gameObject, 2f);
    }

    IEnumerator<WaitForSeconds> RotationSequence(TileEdge[] lookAts, float totalDuration)
    {
        float duration = totalDuration * robotMoveTimeFraction;
        float partDuration = duration / (lookAts.Length - 1);
        for (int from = 0, to = 1; to < lookAts.Length; from++, to++)
        {
            float delta = 0;
            float start = Time.timeSinceLevelLoad;
            Vector3 fromForward = lookAts[from].transform.position - transform.position;
            Vector3 toForward = lookAts[to].transform.position - transform.position;

            Quaternion qFrom = Quaternion.LookRotation(fromForward, Vector3.up);
            Quaternion qTo = Quaternion.LookRotation(toForward, Vector3.up);

            while (delta < partDuration)
            {
                delta = Time.timeSinceLevelLoad - start;
                transform.rotation = Quaternion.Lerp(qFrom, qTo, delta / partDuration);
                yield return new WaitForSeconds(moveAnimationDelta);
            }
        }        
        heading = lookAts[lookAts.Length -1];
        ManageFlame();
        transform.LookAt(heading.transform, Vector3.up);
    }

    [SerializeField] AnimationCurve bumpPositionCurve;
    [SerializeField] AnimationCurve fallPositionCurve;
    [SerializeField] AnimationCurve runPositionCurve;
    [SerializeField] AnimationCurve runRotationCurve;

    private IEnumerator<WaitForSeconds> MoveForward(int steps, float nextCommandInSeconds)
    {
        float duration = nextCommandInSeconds * robotMoveTimeFraction;
        float partDuration = duration / steps;
        for (int step = 0; step < steps; step++)
        {
            float start = Time.timeSinceLevelLoad;
            float delta = 0;
            Vector3 sourcePos = transform.position;
            switch (heading.ExitMode)
            {
                case TileEdgeMode.Block:
                    bool bumped = false;
                    while (delta < partDuration)
                    {
                        delta = Time.timeSinceLevelLoad - start;
                        float fraction = delta / partDuration;
                        if (!bumped && fraction > 0.5f)
                        {
                            bumped = true;
                            if (heading.BumpConnected(flame.Burning) == TileEffect.Burning)
                            {
                                flame.Inflame();
                            }
                            ManageFlame();
                        }
                        transform.position = Vector3.Lerp(sourcePos, heading.transform.position, bumpPositionCurve.Evaluate(fraction));
                        yield return new WaitForSeconds(moveAnimationDelta);
                    }
                    transform.position = sourcePos;
                    break;
                case TileEdgeMode.Allow:
                    tile = heading.ConnectedTile;
                    TileEdge toHeading = heading.HeadingAfterPassing;
                    Vector3 fromForward = heading.transform.position - transform.position;
                    Vector3 toForward = toHeading.transform.position - tile.RestPosition.position;
                    Quaternion qFrom = Quaternion.LookRotation(fromForward, Vector3.up);
                    Quaternion qTo = Quaternion.LookRotation(toForward, Vector3.up);
                    bool flameManaged = false;
                    while (delta < partDuration)
                    {
                        delta = Time.timeSinceLevelLoad - start;
                        float part = delta / partDuration;
                        if (!flameManaged && part > 0.6f)
                        {
                            ManageFlame();
                            flameManaged = true;
                        }
                        transform.position = Vector3.Lerp(part < 0.5f ? sourcePos : heading.transform.position, part < 0.5f ? heading.transform.position : tile.RestPosition.position, runPositionCurve.Evaluate(part));
                        transform.rotation = Quaternion.Lerp(qFrom, qTo, delta / partDuration);
                        yield return new WaitForSeconds(moveAnimationDelta);
                    }
                    heading = toHeading;
                    EndWalk();                    
                    break;
                case TileEdgeMode.Fall:
                    while (delta < partDuration)
                    {
                        delta = Time.timeSinceLevelLoad - start;
                        transform.position = Vector3.Lerp(sourcePos, heading.transform.position, fallPositionCurve.Evaluate(delta / partDuration));
                        yield return new WaitForSeconds(moveAnimationDelta);
                    }
                    WalkOverEdge(heading.transform, heading.transform.position - sourcePos);
                    break;
            }
            if (dead) break;
        }
    }

    private IEnumerator<WaitForSeconds> MoveBackward(int steps, float nextCommandInSeconds)
    {
        float duration = nextCommandInSeconds * robotMoveTimeFraction;
        float partDuration = duration / steps;
        for (int step = 0; step < steps; step++)
        {
            TileEdge reverseHeading = tile.Backward(heading);
            float start = Time.timeSinceLevelLoad;
            float delta = 0;
            Vector3 sourcePos = transform.position;

            switch (reverseHeading.ExitMode)
            {
                case TileEdgeMode.Block:
                    while (delta < partDuration)
                    {
                        delta = Time.timeSinceLevelLoad - start;
                        transform.position = Vector3.Lerp(sourcePos, reverseHeading.transform.position, bumpPositionCurve.Evaluate(delta / partDuration));
                        yield return new WaitForSeconds(moveAnimationDelta);
                    }
                    ManageFlame();
                    transform.position = sourcePos;
                    break;
                case TileEdgeMode.Allow:
                    tile = reverseHeading.ConnectedTile;
                    heading = tile.Backward(reverseHeading.HeadingAfterPassing);                    
                    Vector3 fromForward = transform.position - reverseHeading.transform.position;
                    Vector3 toForward = heading.transform.position - tile.RestPosition.position;
                    Quaternion qFrom = Quaternion.LookRotation(fromForward, Vector3.up);
                    Quaternion qTo = Quaternion.LookRotation(toForward, Vector3.up);
                    bool flameManaged = false;

                    while (delta < partDuration)
                    {
                        delta = Time.timeSinceLevelLoad - start;
                        float part = delta / partDuration;
                        if (!flameManaged && part > 0.75f)
                        {
                            flameManaged = true;
                            ManageFlame();
                        }
                        transform.position = Vector3.Lerp(part < 0.5f ? sourcePos : reverseHeading.transform.position, part < 0.5f ? reverseHeading.transform.position : tile.RestPosition.position, runPositionCurve.Evaluate(part));
                        transform.rotation = Quaternion.Lerp(qFrom, qTo, delta / partDuration);
                        yield return new WaitForSeconds(moveAnimationDelta);
                    }
                    EndWalk();  
                    break;
                case TileEdgeMode.Fall:
                    while (delta < partDuration)
                    {
                        delta = Time.timeSinceLevelLoad - start;
                        transform.position = Vector3.Lerp(sourcePos, reverseHeading.transform.position, fallPositionCurve.Evaluate(delta / partDuration));
                        yield return new WaitForSeconds(moveAnimationDelta);
                    }
                    WalkOverEdge(reverseHeading.transform, reverseHeading.transform.position - sourcePos);
                    break;
            }
            if (dead) break;
        }
    }

    private void RemoteController_OnSendCommand(RobotCommand command, float nextCommandInSeconds)
    {
        if (dead || reachedGoal) return;
        switch (command)
        {
            case RobotCommand.TurnLeftTwo:
                TileEdge[] lookAts = new TileEdge[3] { heading, tile.Left(heading), tile.Left(heading, 2)};
                StartCoroutine(RotationSequence(lookAts, nextCommandInSeconds));
                break;
            case RobotCommand.TurnLeft:
                lookAts = new TileEdge[2] { heading, tile.Left(heading) };
                StartCoroutine(RotationSequence(lookAts, nextCommandInSeconds));
                break;
            case RobotCommand.TurnRightTwo:
                lookAts = new TileEdge[3] { heading, tile.Right(heading), tile.Right(heading, 2) };
                StartCoroutine(RotationSequence(lookAts, nextCommandInSeconds));
                break;
            case RobotCommand.TurnRight:
                lookAts = new TileEdge[2] { heading, tile.Right(heading) };
                StartCoroutine(RotationSequence(lookAts, nextCommandInSeconds));
                break;
            case RobotCommand.ForwardOne:
                StartCoroutine(MoveForward(1, nextCommandInSeconds));
                break;
            case RobotCommand.ForwardTwo:
                StartCoroutine(MoveForward(2, nextCommandInSeconds));
                break;
            case RobotCommand.ForwardThree:
                StartCoroutine(MoveForward(3, nextCommandInSeconds));
                break;
            case RobotCommand.BackupOne:
                StartCoroutine(MoveBackward(1, nextCommandInSeconds));
                break;
            case RobotCommand.BackupTwo:
                StartCoroutine(MoveBackward(2, nextCommandInSeconds));
                break;
            case RobotCommand.NONE:
                //TODO: Idle!
                break;
        }        
    }

    [SerializeField] string[] flameMsgs;
    [SerializeField] string[] waterMsgs;
    [SerializeField] string[] windMsgs;
    int idxFlame;
    int idxWater;
    int idxWind;

    void ManageFlame()
    {
        switch (tile.TileEffect)
        {
            case TileEffect.Burning:
                flame.Inflame();
                OnRobotMessage?.Invoke(flameMsgs[idxFlame]);
                idxFlame++;
                if (idxFlame >= flameMsgs.Length) idxFlame = 0;
                break;
            case TileEffect.Watery:
                flame.Douse();
                OnRobotMessage?.Invoke(waterMsgs[idxWater]);
                idxWater++;
                if (idxWater >= waterMsgs.Length) idxWater = 0;
                break;
            case TileEffect.Windy:
                flame.Blow();
                OnRobotMessage?.Invoke(windMsgs[idxWind]);
                idxWind++;
                if (idxWind >= windMsgs.Length) idxWind = 0;
                break;
        }
    }
}
