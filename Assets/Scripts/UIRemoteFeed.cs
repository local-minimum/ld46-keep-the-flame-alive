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
    public void InjectDraggedCard(UIRobotCommand card)
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
            card.SnapToPosition(feedLength - 1);
            return;
        }
        int insertPosition = bestCard.FeedPosition;        
        bool leftMoved = false;
        for (int i = 1, l=feed.Count; i < feedLength; i++)
        {
            Debug.Log(string.Format("{0} {1} {2}", insertPosition, leftMoved, i));
            if (i == insertPosition && leftMoved) break;
            for (int j = 0; j < l; j++)
            {
                var fCard = feed[j];
                Debug.Log(string.Format("Occ {0} {1}!={2} {3}", fCard == card, fCard.FeedPosition, i, !fCard.Occupies(fCard.FeedPosition)));
                if (fCard == card || fCard.FeedPosition != i || !fCard.Occupies(fCard.FeedPosition)) continue;
                if (i < insertPosition)
                {
                    var newPos = GetLeftmostOpen(fCard.FeedPosition);
                    Debug.Log(string.Format("Left {0} => {1}", fCard.FeedPosition, newPos));
                    if (newPos != fCard.FeedPosition)
                    {
                        feed[i].SnapToPosition(newPos);
                        leftMoved = true;
                    }                    
                } else
                {
                    Debug.Log(string.Format("Right"));
                    feed[i].ShiftRight();
                }                
                break;
            }
        }
        card.SnapToPosition(GetLeftmostOpen(insertPosition), true);
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
        uiCard.Spawn(cardSprites[(int)command], GetLeftmostOpen(feedSlot), feedLength);
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
