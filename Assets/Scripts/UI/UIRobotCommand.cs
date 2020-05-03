using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class UIRobotCommand : MonoBehaviour, IEndDragHandler, IDragHandler, IBeginDragHandler
{
    public delegate void RobotCommandGrabEvent(ShiftCommand command, ShiftCommand target);
    public static event RobotCommandGrabEvent OnGrabRobotCommand;
    public static event RobotCommandGrabEvent OnReleaseRobotCommand;

    public ShiftCommand shiftCommand { get; private set; }

    UIRemoteFeed _feed;
    UIRemoteFeed feed
    {
        get
        {
            if (_feed == null)
            {
                _feed = GetComponentInParent<UIRemoteFeed>();
            }
            return _feed;
        }
    }

    [SerializeField]
    Image image;    

    [SerializeField]
    float zeroMarginLeft = 0.005f;

    [SerializeField]
    float zeroMarginRight = 0.01f;

    [SerializeField]
    float positionsFraction = .75f;

    float containerWidth
    {
        get
        {
            return (transform.parent as RectTransform).rect.width;
        }
    }

    public Vector2 targetAnchoredPosition
    {
        get
        {
            float width = containerWidth;
            float x = -(1 - zeroMarginLeft) * width / 2;
            int feedPosition = shiftCommand.position;
            x += (1 + feedPosition) * width * positionsFraction / feed.feedFullLength;
            if (feedPosition < 1)
            {
                return new Vector2(x, 0);
            }
            x += zeroMarginRight * width / 2;
            return new Vector2(x, 0 );
        }
    }

    public void SnapToPosition(bool forceSnap = false)
    {        
        if (!beingPulled || forceSnap)
        {
            RectTransform t = (transform as RectTransform);
            t.anchoredPosition = targetAnchoredPosition;
        }
    }

    public void ShiftRight(ShiftCommand command)
    {
        shiftCommand = command;
        SnapToPosition(true);        
    }

    public void ShiftLeft(ShiftCommand command)
    {
        shiftCommand = command;
        SnapToPosition();
    }

    public void NotInPlay()
    {
        shiftCommand = ShiftCommand.NotInPlay;
        beingPulled = false;
        SnapToPosition();
        gameObject.SetActive(false);
    }

    public void SetInPlay(ShiftCommand command)
    {
        shiftCommand = command;
        beingPulled = false;
        SnapToPosition();
        gameObject.SetActive(true);
    }

    public void SetInPlay(ShiftCommand command, Sprite sprite)
    {
        image.sprite = sprite;
        SetInPlay(command);
    }

    Vector2 dragOffset;

    bool beingPulled;
    public void OnDrag(PointerEventData eventData)
    {
        if (shiftCommand.BeingPlayed || !beingPulled) return;
        dragOffset += new Vector2(eventData.delta.x, 0);
        RectTransform t = (transform as RectTransform);
        t.anchoredPosition = targetAnchoredPosition + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (beingPulled)
        {
            ShiftCommand insertBefore = feed.GetBestInsertBeforeCard(this);            
            beingPulled = false;
            OnReleaseRobotCommand?.Invoke(this.shiftCommand, insertBefore);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (shiftCommand.BeingPlayed) return;
        dragOffset = Vector2.up * 20f;
        beingPulled = true;
        transform.SetAsLastSibling();
        OnGrabRobotCommand?.Invoke(shiftCommand, ShiftCommand.NothingHeld);
    }    
}
