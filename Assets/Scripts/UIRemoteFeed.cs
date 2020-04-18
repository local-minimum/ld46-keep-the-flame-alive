using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRemoteFeed : MonoBehaviour
{
    private int feedLength = 9;

    [SerializeField]
    private UIRobotCommand commandPrefab;

    [SerializeField]
    Sprite[] cardSprites = new Sprite[9];

    private List<UIRobotCommand> feed = new List<UIRobotCommand>();

    private void OnEnable()
    {
        RemoteController.OnDrawCommand += RemoteController_OnDrawCommand;
        RemoteController.OnSendCommand += RemoteController_OnSendCommand;
    }

    private void OnDisable()
    {
        RemoteController.OnDrawCommand -= RemoteController_OnDrawCommand;
        RemoteController.OnSendCommand -= RemoteController_OnSendCommand;
    }

    private void RemoteController_OnSendCommand(RobotCommand command, float nextCommandInSeconds)
    {
        bool needMoreShifts = true;
        for (int i = 0, l = feed.Count; i < l; i++)
        {
            if (feed[i].isNextInFeed)
            {
                feed[i].PlayCard();
            }
        }
        while (needMoreShifts)
        {
            for (int i = 0, l = feed.Count; i < l; i++)
            {
                if (feed[i].ShiftLeft() == 0)
                {
                    needMoreShifts = false;
                }
            }
        }
    }

    private void RemoteController_OnDrawCommand(RobotCommand command, int feedSlot, int feedLength)
    {
        this.feedLength = feedLength;
        UIRobotCommand uiCard = GetInactiveOrSpawn();
        uiCard.Spawn(cardSprites[(int)command], feedSlot, feedLength);
    }

    private UIRobotCommand GetInactiveOrSpawn()
    {
        for (int i = 0, l = feed.Count; i < l; i++)
        {
            if (!feed[i].gameObject.activeSelf)
            {
                return feed[i];
            }
        }
        var card = Instantiate(commandPrefab, transform, false);
        feed.Add(card);
        return card;
    }

}
