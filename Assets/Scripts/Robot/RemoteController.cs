using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteController : MonoBehaviour
{
    public delegate void SendCommand(RobotCommand command, float nextCommandInSeconds);
    public static event SendCommand OnSendCommand;
    public delegate void SyncCommands(List<ShiftCommand> commands, ShiftCommand held);
    public static event SyncCommands OnSyncCommands;
    public delegate void RobotLostEvent();
    public static event RobotLostEvent OnRobotLost;

    [SerializeField]
    private bool robotAlive = false;

    [SerializeField]
    private int instructionFeedLength = 7;

    [SerializeField]
    private int processingFramRate = 144;

    [SerializeField]
    private int processingHertz = 66;

    [SerializeField]
    private int[] cardFreqs = new int[9];

    private List<RobotCommand> drawDeck = new List<RobotCommand>();
    private List<ShiftCommand> instructionsFeed = new List<ShiftCommand>();
    private List<RobotCommand> trashDeck = new List<RobotCommand>();
    private ShiftCommand heldCard = ShiftCommand.NothingHeld;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ExecutionLoop());
    }

    private void OnEnable()
    {
        UIRobotCommand.OnGrabRobotCommand += UIRobotCommand_OnGrabRobotCommand;
        UIRobotCommand.OnReleaseRobotCommand += UIRobotCommand_OnReleaseRobotCommand;
        RobotController.OnRobotDeath += RobotController_OnRobotDeath;
        RobotFactory.OnSpawnRobot += RobotFactory_OnSpawnRobot;
        UISelectSpeed.OnChangeSpeed += UISelectSpeed_OnChangeSpeed;
        Flame.OnFlameChange += Flame_OnFlameChange;
        UIDisconnect.OnDisconnectRobot += UIDisconnect_OnDisconnectRobot;
        GoalTile.OnGoalReached += GoalTile_OnGoalReached;
    }

    private void OnDisable()
    {
        UIRobotCommand.OnGrabRobotCommand -= UIRobotCommand_OnGrabRobotCommand;
        UIRobotCommand.OnReleaseRobotCommand -= UIRobotCommand_OnReleaseRobotCommand;
        RobotFactory.OnSpawnRobot -= RobotFactory_OnSpawnRobot;
        UISelectSpeed.OnChangeSpeed -= UISelectSpeed_OnChangeSpeed;
        Flame.OnFlameChange -= Flame_OnFlameChange;
        UIDisconnect.OnDisconnectRobot -= UIDisconnect_OnDisconnectRobot;
        GoalTile.OnGoalReached -= GoalTile_OnGoalReached;
    }

    bool reachedGoal = false;
    private void GoalTile_OnGoalReached()
    {
        reachedGoal = true;
        StartCoroutine(WipeFeed());
    }

    private void UIDisconnect_OnDisconnectRobot()
    {
        if (reachedGoal) return;
        StartCoroutine(WipeFeed());
        OnRobotLost?.Invoke();
    }

    bool robotHasFlame = false;

    private void Flame_OnFlameChange(int intensity)
    {
        robotHasFlame = intensity > 0;    
    }

    private void UISelectSpeed_OnChangeSpeed(int speed)
    {
        processingHertz = speed;
    }

    private void RobotFactory_OnSpawnRobot(RobotController robot)
    {
        if (reachedGoal) return;
        StartCoroutine(ReconnectRobot());
    }

    IEnumerator<WaitForSeconds> ReconnectRobot()
    {
        while (instructionsFeed.Count < instructionFeedLength)
        {
            DrawOne();
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);
        robotAlive = true;
    }

    private void RobotController_OnRobotDeath(RobotController robot)
    {
        robotAlive = false;
        StartCoroutine(WipeFeed());
    }


    IEnumerator<WaitForSeconds> WipeFeed() {
        heldCard = ShiftCommand.NothingHeld;
        
        //Remove what remains
        float flushSpeed = 0.1f;
        while (instructionsFeed.Count > 0)
        {
            ExecuteCommand(flushSpeed, false);
            yield return new WaitForSeconds(flushSpeed);
        }        
    }

    private void UIRobotCommand_OnReleaseRobotCommand(ShiftCommand command, ShiftCommand insertBefore)
    {
        if (!robotAlive || reachedGoal) return;

        if (!heldCard.SameAs(command))
        {
            Debug.LogWarning(string.Format("Trying to insert card {0} that doesn't match what is held {1}.", command, heldCard));
            return;
        }
        bool inserted = false;
        ShiftCommand insertion = ShiftCommand.NothingHeld;
        int position = 0;
        for (int i=0, l=instructionsFeed.Count; i<l; i++)
        {
            if (inserted)
            {
                instructionsFeed[i] = ShiftCommand.JumpRight(instructionsFeed[i]);
            } else
            {
                if (instructionsFeed[i].SameAs(insertBefore))
                {
                    insertion = ShiftCommand.Insert(instructionsFeed[i], command.command);
                    inserted = true;
                    position = i;
                }
            }
        }
        if (inserted)
        {
            instructionsFeed.Insert(position, insertion);
        } else
        {
            instructionsFeed.Add(ShiftCommand.MoveLeftFrom(command.command, instructionsFeed.Count));
        }

        heldCard = ShiftCommand.NothingHeld;
        OnSyncCommands?.Invoke(instructionsFeed, heldCard);
    }

    private void UIRobotCommand_OnGrabRobotCommand(ShiftCommand command, ShiftCommand insertBefore)
    {
        if (reachedGoal || !robotAlive || !insertBefore.Held) return;
        for (int i=0, l=instructionsFeed.Count; i<l; i++)
        {
            ShiftCommand card = instructionsFeed[i];
            if (card.SameAs(command))
            {
                instructionsFeed.RemoveAt(i);
                if (!heldCard.Empty)
                {
                    Debug.LogWarning(string.Format("Picking up a second card {0} while holding {1}", card, heldCard));
                    trashDeck.Add(heldCard.command);
                }
                heldCard = ShiftCommand.Pickup(card);
                SendSynchFeed();
                return;
            }
        }
        Debug.LogWarning(string.Format("Tried to pickup card {0}, but couldn't find it", command));
    }

    private void InitDeck()
    {
        for (int idcmd=0; idcmd < cardFreqs.Length; idcmd++)
        {
            for (int idcard=0, ncard=cardFreqs[idcmd]; idcard < ncard; idcard++)
            {             
                drawDeck.Add((RobotCommand)idcmd);
            }
        }
        Debug.Log(string.Format("Deck size {0}", drawDeck.Count));
        drawDeck.Shuffle();
    }

    private void DrawOne()
    {        
        // Get new draw deck if needed
        if (drawDeck.Count == 0) FlipTrash();

        // Get the card
        RobotCommand cmd = RobotCommand.NONE;
        if (drawDeck.Count > 0)
        {
            cmd = drawDeck[0];
            drawDeck.RemoveAt(0);
        }

        // Insert card
        instructionsFeed.Add(ShiftCommand.MoveLeftFrom(cmd, instructionsFeed.Count));

        // Inform subscribers 
        SendSynchFeed();
    }

    private void DrawToFeed()
    {   
        for (int i=instructionsFeed.Count; i < instructionFeedLength - (heldCard.Empty ?  0 : 1); i++)
        {
            DrawOne();
        }
    }

    private void FlipTrash()
    {
        drawDeck.AddRange(trashDeck);
        trashDeck.Clear();
        drawDeck.Shuffle();
        Debug.Log(string.Format("Deck size {0} + {1}", drawDeck.Count, instructionsFeed.Count));
    }

    private void SendSynchFeed()
    {
        for (int idx=0,l=instructionsFeed.Count; idx<l; idx++)
        {
            if (instructionsFeed[idx].LeftOrOn(idx))
            {
                instructionsFeed[idx] = ShiftCommand.MoveLeftFrom(instructionsFeed[idx].command, idx);
            }
        }
        OnSyncCommands?.Invoke(instructionsFeed, heldCard);
    }

    private void ExecuteCommand(float nextCommandInSeconds, bool draw=true)
    {
        if (instructionsFeed.Count == 0)
        {
            OnRobotLost?.Invoke();
            return;
        }
        RobotCommand cmd = instructionsFeed[0].command;
        instructionsFeed.RemoveAt(0);
        if (cmd != RobotCommand.NONE)
        {
            trashDeck.Add(cmd);
        }

        if (!reachedGoal)
        {
            OnSendCommand?.Invoke(cmd, nextCommandInSeconds);
        }
        if (draw)
        {
            DrawToFeed();
        } else
        {
            SendSynchFeed();
        }
    }

    [SerializeField] float beforeCardsDelay = 2f;
    [SerializeField] float beforeExecutionDelay = 3f;
    private IEnumerator<WaitForSeconds> ExecutionLoop()
    {
        InitDeck();
        yield return new WaitForSeconds(beforeCardsDelay);
        while (instructionsFeed.Count < instructionFeedLength)
        {
            DrawOne();
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(beforeExecutionDelay);
        while (true)
        {
            float nextCommandInSeconds = (float)processingFramRate / (float)processingHertz;
            if (robotAlive)
            {
                ExecuteCommand(nextCommandInSeconds, robotHasFlame);
            }
            yield return new WaitForSeconds(nextCommandInSeconds);
        }
    }
}
