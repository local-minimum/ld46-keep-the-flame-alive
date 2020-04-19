using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteController : MonoBehaviour
{
    public delegate void SendCommand(RobotCommand command, float nextCommandInSeconds);
    public static event SendCommand OnSendCommand;
    public delegate void SyncCommands(List<RobotCommand> commands, RobotCommand held);
    public static event SyncCommands OnDrawCommand;
    public static event SyncCommands OnSyncCommands;

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
    private List<RobotCommand> instructionsFeed = new List<RobotCommand>();
    private List<RobotCommand> trashDeck = new List<RobotCommand>();
    private RobotCommand heldCard = RobotCommand.NONE;

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
    }

    private void OnDisable()
    {
        UIRobotCommand.OnGrabRobotCommand -= UIRobotCommand_OnGrabRobotCommand;
        UIRobotCommand.OnReleaseRobotCommand -= UIRobotCommand_OnReleaseRobotCommand;
        RobotFactory.OnSpawnRobot -= RobotFactory_OnSpawnRobot;
    }

    private void RobotFactory_OnSpawnRobot(RobotController robot)
    {
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
        heldCard = RobotCommand.NONE;
        OnSyncCommands?.Invoke(instructionsFeed, heldCard);
        float flushSpeed = 0.1f;
        while (instructionsFeed.Count > 0)
        {
            ExecuteCommand(flushSpeed, false);
            yield return new WaitForSeconds(flushSpeed);
        }        
    }

    private void UIRobotCommand_OnReleaseRobotCommand(int position)
    {
        if (heldCard == RobotCommand.NONE)
        {
            Debug.LogWarning("Trying to insert card but none exists, maybe held from previous robot");
            return;
        }

        if (robotAlive && instructionsFeed.Count > position) instructionsFeed.Insert(position, heldCard);
        heldCard = RobotCommand.NONE;
        OnSyncCommands?.Invoke(instructionsFeed, heldCard);
    }

    private void UIRobotCommand_OnGrabRobotCommand(int position)
    {
        RobotCommand card = instructionsFeed[position];
        instructionsFeed.RemoveAt(position);
        if (heldCard != RobotCommand.NONE)
        {
            Debug.LogWarning(string.Format("Picking up a second card {0} while holding {1}", card, heldCard));
        }
        heldCard = card;
        OnSyncCommands?.Invoke(instructionsFeed, heldCard);

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
        drawDeck.Shuffle();
    }

    private void DrawOne()
    {
        if (drawDeck.Count == 0) FlipTrash();
        RobotCommand cmd = RobotCommand.NONE;
        if (instructionsFeed.Count > 0)
        {
            cmd = drawDeck[0];
            drawDeck.RemoveAt(0);
        }
        instructionsFeed.Add(cmd);
        OnDrawCommand?.Invoke(instructionsFeed, heldCard);
    }

    private void DrawToFeed()
    {   
        for (int i=instructionsFeed.Count; i < instructionFeedLength - (heldCard == RobotCommand.NONE ?  0 : 1); i++)
        {
            DrawOne();
        }
    }

    private void FlipTrash()
    {
        drawDeck.AddRange(trashDeck);
        trashDeck.Clear();
        drawDeck.Shuffle();
    }

    private void ExecuteCommand(float nextCommandInSeconds, bool draw=true)
    {
        if (instructionsFeed.Count == 0)
        {
            Debug.LogWarning("Instructions feed empty, should never happen");
            return;
        }
        RobotCommand cmd = instructionsFeed[0];
        instructionsFeed.RemoveAt(0);
        if (cmd != RobotCommand.NONE)
        {
            trashDeck.Add(cmd);
        }
        OnSendCommand?.Invoke(cmd, nextCommandInSeconds);
        if (draw) DrawToFeed();
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
                ExecuteCommand(nextCommandInSeconds);
            }
            yield return new WaitForSeconds(nextCommandInSeconds);
        }
    }
}
