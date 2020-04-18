using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RobotCommand
{
    ForwardOne,
    ForwardTwo,
    ForwardThree,
    TurnLeft,
    TurnLeftTwo,
    TurnRight,
    TurnRightTwo,
    BackupOne,
    BackupTwo,
}

public class RemoteController : MonoBehaviour
{
    public delegate void SendCommand(RobotCommand command, float nextCommandInSeconds);
    public static event SendCommand OnSendCommand;
    public delegate void DrawCommand(RobotCommand command, int feedSlot, int feedLength);
    public static event DrawCommand OnDrawCommand;

    [SerializeField]
    private bool robotAlive = true;

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

    // Start is called before the first frame update
    void Start()
    {        
        StartCoroutine(ExecutionLoop());
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
        RobotCommand cmd = drawDeck[0];
        drawDeck.RemoveAt(0);
        OnDrawCommand?.Invoke(cmd, instructionsFeed.Count, instructionFeedLength);
        instructionsFeed.Add(cmd);
    }

    private void DrawToFeed()
    {   
        for (int i=instructionsFeed.Count; i < instructionFeedLength; i++)
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

    private void ExecuteCommand(float nextCommandInSeconds)
    {
        if (instructionsFeed.Count == 0)
        {
            Debug.LogWarning("Instructions feed empty, should never happen");
            return;
        }
        RobotCommand cmd = instructionsFeed[0];
        instructionsFeed.RemoveAt(0);
        trashDeck.Add(cmd);
        OnSendCommand?.Invoke(cmd, nextCommandInSeconds);
        DrawToFeed();
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
                Debug.Log(string.Join(", ", instructionsFeed));
            }
            yield return new WaitForSeconds(nextCommandInSeconds);
        }
    }
}
