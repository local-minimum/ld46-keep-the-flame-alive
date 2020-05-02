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

    RobotPosition position;
    bool dead = true;

    [SerializeField, Range(0, .95f)]
    float robotMoveTimeFraction = 0.8f;

    public void SetSpawn(Tile tile)
    {
        position = RobotPosition.SpawnPosition(tile);
        EndWalk();
        dead = false;
    }
    
    public void HideCorpse()
    {
        gameObject.SetActive(false);
    }

    public void Respawn(Tile tile)
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;
        reachedGoal = false;
        SetSpawn(tile);
        gameObject.SetActive(true);
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
        if (dead) return;
        Vector3 impulse = new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1));
        if (impulse.sqrMagnitude == 0)
        {
            impulse = Vector3.forward;
        } else
        {
            impulse.Normalize();
        }
        if (!dead && !reachedGoal) WalkOverEdge(transform, impulse);
    }

    void EndWalk()
    {
        transform.position = position.tile.RestPosition.position;
        transform.LookAt(position.heading.transform, Vector3.up);
    }

    void WalkOverEdge(Transform edge, Vector3 impulse)
    {
        dead = true;
        transform.position = edge.transform.position;
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.AddForce(impulse, ForceMode.Impulse);
        OnRobotDeath?.Invoke(this);
    }   

    IEnumerator<WaitForSeconds> RotationSequence(TileEdge[] lookAts, float totalDuration)
    {
        float duration = totalDuration * robotMoveTimeFraction;
        float partDuration = duration / (lookAts.Length - 1);
        position = position.Rotate(lookAts[lookAts.Length - 1]);
        for (int from = 0, to = 1; to < lookAts.Length; from++, to++)
        {
            if (reachedGoal) break;
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
                if (dead) break;
            }
            if (dead) break;
        }        
        ManageFlame();
        transform.LookAt(position.heading.transform, Vector3.up);
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
            if (reachedGoal) break;
            float start = Time.timeSinceLevelLoad;
            float delta = 0;
            Vector3 sourcePos = transform.position;
            switch (position.heading.ExitMode)
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
                            if (position.heading.BumpConnected(flame.Burning) == TileEffect.Burning)
                            {
                                flame.Inflame();
                            }
                            ManageFlame();
                        }
                        transform.position = Vector3.Lerp(sourcePos, position.heading.transform.position, bumpPositionCurve.Evaluate(fraction));
                        yield return new WaitForSeconds(moveAnimationDelta);
                        if (dead) break;
                    }
                    transform.position = sourcePos;
                    break;
                case TileEdgeMode.Allow:
                    var tile = position.heading.ConnectedTile;
                    TileEdge fromHeading = position.heading;
                    TileEdge toHeading = position.heading.HeadingAfterPassing;
                    if (toHeading.tile != tile)
                    {
                        Debug.LogError(string.Format(
                            "Trying to move from {0} to {1}, but heading after {2} is on tile {3}",
                            position.tile.name,
                            tile.name,
                            toHeading.name,
                            toHeading.tile.name
                        ));
                    }                    
                    Vector3 fromForward = position.heading.transform.position - transform.position;
                    position = new RobotPosition(tile, toHeading);
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
                        transform.position = Vector3.Lerp(part < 0.5f ? sourcePos : fromHeading.transform.position, part < 0.5f ? fromHeading.transform.position : tile.RestPosition.position, runPositionCurve.Evaluate(part));
                        transform.rotation = Quaternion.Lerp(qFrom, qTo, delta / partDuration);
                        yield return new WaitForSeconds(moveAnimationDelta);
                        if (dead) break;
                    }                    
                    if (!dead) EndWalk();                    
                    break;
                case TileEdgeMode.Fall:
                    while (delta < partDuration)
                    {
                        delta = Time.timeSinceLevelLoad - start;
                        transform.position = Vector3.Lerp(sourcePos, position.heading.transform.position, fallPositionCurve.Evaluate(delta / partDuration));
                        yield return new WaitForSeconds(moveAnimationDelta);
                        if (dead) break;
                    }
                    if (!dead) WalkOverEdge(position.heading.transform, position.heading.transform.position - sourcePos);
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
            if (reachedGoal) break;
            TileEdge reverseHeading = position.tile.Backward(position.heading);
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
                        if (dead) break;
                    }
                    if (!dead)
                    {
                        ManageFlame();
                        transform.position = sourcePos;
                    }
                    break;
                case TileEdgeMode.Allow:
                    var tile = reverseHeading.ConnectedTile;
                    var heading = tile.Backward(reverseHeading.HeadingAfterPassing);
                    if (heading.tile != tile)
                    {
                        Debug.LogError(string.Format(
                            "Trying to move from {0} to {1}, but heading after {2} is on tile {3}",
                            position.tile.name,
                            tile.name,
                            heading.name,
                            heading.tile.name
                        ));
                    }
                    position = new RobotPosition(tile, heading);
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
                        if (dead) break;
                    }
                    if (!dead) EndWalk();  
                    break;
                case TileEdgeMode.Fall:
                    while (delta < partDuration)
                    {
                        delta = Time.timeSinceLevelLoad - start;
                        transform.position = Vector3.Lerp(sourcePos, reverseHeading.transform.position, fallPositionCurve.Evaluate(delta / partDuration));
                        yield return new WaitForSeconds(moveAnimationDelta);
                        if (dead) break;
                    }
                    if (!dead) WalkOverEdge(reverseHeading.transform, reverseHeading.transform.position - sourcePos);
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
                TileEdge[] lookAts = new TileEdge[3] { position.heading, position.tile.Left(position.heading), position.tile.Left(position.heading, 2)};
                StartCoroutine(RotationSequence(lookAts, nextCommandInSeconds));
                break;
            case RobotCommand.TurnLeft:
                lookAts = new TileEdge[2] { position.heading, position.tile.Left(position.heading) };
                StartCoroutine(RotationSequence(lookAts, nextCommandInSeconds));
                break;
            case RobotCommand.TurnRightTwo:
                lookAts = new TileEdge[3] { position.heading, position.tile.Right(position.heading), position.tile.Right(position.heading, 2) };
                StartCoroutine(RotationSequence(lookAts, nextCommandInSeconds));
                break;
            case RobotCommand.TurnRight:
                lookAts = new TileEdge[2] { position.heading, position.tile.Right(position.heading) };
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


    void ManageFlame()
    {
        switch (position.tile.TileEffect)
        {
            case TileEffect.Burning:
                flame.Inflame();
                OnRobotMessage?.Invoke(flameMsgs[Random.Range(0, flameMsgs.Length)]);
                break;
            case TileEffect.Watery:
                flame.Douse();
                OnRobotMessage?.Invoke(waterMsgs[Random.Range(0, waterMsgs.Length)]);
                break;
            case TileEffect.Windy:
                flame.Blow();
                OnRobotMessage?.Invoke(windMsgs[Random.Range(0, windMsgs.Length)]);
                break;
        }
    }
}
