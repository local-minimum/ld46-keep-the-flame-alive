using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRemoteFeed : MonoBehaviour
{
    public int feedFullLength { get; } = 9;

    [SerializeField] Transform cardsHolder;

    [SerializeField]
    private UIRobotCommand commandPrefab;

    [SerializeField]
    Sprite[] cardSprites = new Sprite[9];

    private List<UIRobotCommand> feed = new List<UIRobotCommand>();
    private UIRobotCommand heldCard;
    private List<UIRobotCommand> cardsNotInPlay = new List<UIRobotCommand>();

    private void OnEnable()
    {
        RemoteController.OnSendCommand += RemoteController_OnSendCommand;
        RemoteController.OnSyncCommands += RemoteController_OnSyncCommands;
    }

    private void OnDisable()
    {
        RemoteController.OnSendCommand -= RemoteController_OnSendCommand;
        RemoteController.OnSyncCommands -= RemoteController_OnSyncCommands;
    }

    private void RemoteController_OnSyncCommands(List<ShiftCommand> commands, ShiftCommand held)
    {
        int idC = 0;
        int lC = commands.Count;
        for (int idF=0, lF=feed.Count; idF<lF; idF++)
        {
            var command = commands[idC];
            var card = feed[idF];
            // Card is where it should be, nothing to do
            if (card.shiftCommand.SameAs(command))
            {
                idC++;
                continue;
            }

            // Card has moved leftward
            if (card.shiftCommand.SameButShiftedLeft(command))
            {
                card.ShiftLeft(command);
                idC++;
                continue;
            }

            // Card needs to shift rightward
            if (card.shiftCommand.SameButShiftedRight(command))
            {
                card.ShiftRight(command);
                idC++;
                continue;
            }

            // Card is picked up
            if (card.shiftCommand.SameButPickedUp(held))
            {
                if (heldCard != null)
                {
                    cardsNotInPlay.Add(heldCard);
                    heldCard.NotInPlay();
                }

                heldCard = card;
                feed.RemoveAt(idF);
                idF--;
                lF--;
                idC++;
                continue;
            }

            // Actually previously held card is now here
            if (heldCard != null && command.command == heldCard.shiftCommand.command)
            {
                heldCard.SetInPlay(command);
                feed.Insert(idF, heldCard);
                heldCard = null;
                lF++;                
                continue;
            }

            // Remove card because not in play
            if (idF > 0)
            {
                Debug.LogWarning(string.Format("Lost track of UI Card {0}, this should not happen", card));
            }            
            card.NotInPlay();
            cardsNotInPlay.Add(card);            
            feed.RemoveAt(idF);
            idF--;
            lF--;
            continue;
        }

        if (idC != feed.Count)
        {
            Debug.LogWarning(string.Format("We have a gap in the feed, it should be {0} long, but is {1}", idC, feed.Count));
        }

        // Fillout new cards
        while (idC < lC)
        {
            ShiftCommand command = commands[(int)idC];
            if (!command.Empty)
            {
                var card = GetInactiveOrSpawn();
                card.SetInPlay(command, cardSprites[(int)command.command]);
                feed.Add(card);
            }
            idC++;
        }

        //Deal with held card
        if (held.Empty && heldCard != null)
        {
            heldCard.NotInPlay();
            cardsNotInPlay.Add(heldCard);
            heldCard = null;
        } else if (!held.Empty && heldCard != null && !held.SameAs(heldCard.shiftCommand))
        {
            Debug.LogWarning(string.Format("Held card is wrong, this should not happen, expected {0}, but found {1} (updating)", held, heldCard.shiftCommand));
            heldCard.SetInPlay(held, cardSprites[(int)held.command]);
        }
    }

    private void RemoteController_OnSendCommand(RobotCommand command, float nextCommandInSeconds)
    {
        Debug.Log("Not implemented smooth scrolling");
    }

    [SerializeField] float beforeCardMargin = 10;
    public ShiftCommand GetBestInsertBeforeCard(UIRobotCommand card)
    {
        var cardX = card.transform.position.x + beforeCardMargin;
        // We can't allow placeing before currently playing card
        for (int i=1, l=feed.Count; i<l; i++)
        {
            var target = feed[i];
            if (target.transform.position.x < cardX)
            {
                return target.shiftCommand;
            }
        }
        return ShiftCommand.NotInPlay;
    }

    private UIRobotCommand GetInactiveOrSpawn()
    {
        UIRobotCommand card;
        if (cardsNotInPlay.Count > 0)
        {
            card = cardsNotInPlay[0];
            cardsNotInPlay.RemoveAt(0);
        } else
        {
            card = Instantiate(commandPrefab, cardsHolder, false);
        }
        return card;
    }

}
