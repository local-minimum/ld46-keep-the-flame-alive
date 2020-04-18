﻿using System.Collections;
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
        RemoteController.OnSyncCommands += RemoteController_OnSyncCommands;
    }

    private void OnDisable()
    {
        RemoteController.OnDrawCommand -= RemoteController_OnDrawCommand;
        RemoteController.OnSendCommand -= RemoteController_OnSendCommand;
        RemoteController.OnSyncCommands -= RemoteController_OnSyncCommands;
    }

    private void RemoteController_OnSyncCommands(List<RobotCommand> commands)
    {
        Debug.Log(string.Join(", ", commands));
        int idC = 0;
        int lc = commands.Count;
        for (int idF=0, l=feed.Count; idF<l; idF++)
        {
            if (feed[idF].Grabbed) continue;
            if (idC < lc)
            {
                RobotCommand command = commands[(int)idC];
                feed[idF].SnapToPosition(cardSprites[(int) command], idC, feedLength);
            } else
            {
                feed[idF].gameObject.SetActive(false);
            }
            idC++;
        }
        while (idC < lc)
        {
            var card = GetInactiveOrSpawn();
            RobotCommand command = commands[(int)idC];
            card.SnapToPosition(cardSprites[(int) command], idC, feedLength);
            idC++;
        }
    }

    private void RemoteController_OnSendCommand(RobotCommand command, float nextCommandInSeconds)
    {
        for (int i = 0, l = feed.Count; i < l; i++)
        {
            if (feed[i].isNextInFeed)
            {
                feed[i].PlayCard();
            }
        }
        for (int i = 1; i < feedLength; i++)
        {
            for (int j = 0,l = feed.Count; j<l; j++)
            {
                if (feed[j].Occupies(i))
                {
                    feed[j].SnapToPosition(GetLeftmostOpen(i));
                }
            }
        }
    }

    [SerializeField] float beforeCardMargin = 10;
    public int GetBestCardPosition(UIRobotCommand card)
    {
        float cardX = (card.transform as RectTransform).anchoredPosition.x;
        float bestDelta = -Mathf.Infinity;
        UIRobotCommand bestCard = null;
        for (int i = 0, l = feed.Count; i < l; i++)
        {
            var fCard = feed[i];
            if (fCard == card || card.isNextInFeed) continue;
            var fCardX = fCard.targetAnchoredPosition.x;
            var delta = cardX - fCardX - beforeCardMargin;
            if (delta > 0 || delta < bestDelta) continue;
            bestCard = fCard;
            bestDelta = delta;
        }
        if (!bestCard)
        {
            return feedLength - 1;
        }
        int insertPosition = bestCard.FeedPosition;
        return GetLeftmostOpen(insertPosition);
    }

    int GetLeftmostOpen(int slot)
    {
        for (int pos=slot - 1; pos >= 0; pos--)
        {
            for (int fpos=0, l=feed.Count; fpos<l; fpos++)
            {
                if (feed[fpos].Occupies(pos)) return pos + 1;
            }
        }
        return 0;
    }

    private void RemoteController_OnDrawCommand(RobotCommand command, int feedSlot, int feedLength)
    {
        this.feedLength = feedLength;
        UIRobotCommand uiCard = GetInactiveOrSpawn();
        uiCard.SnapToPosition(cardSprites[(int)command], GetLeftmostOpen(feedSlot), feedLength);
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
