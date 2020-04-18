using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRobotCommand : MonoBehaviour
{
    [SerializeField]
    int feedPosition;

    public bool isNextInFeed
    {
        get
        {
            return gameObject.activeSelf && feedPosition == 0;
        }
    }

    [SerializeField]
    Image image;

    bool beingPulled = false;

    int nPositions;

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

    Vector2 targetAnchoredPosition
    {
        get
        {
            float width = containerWidth;
            float x = -(1 - zeroMarginLeft) * width / 2;
            if (feedPosition == 0)
            {
                return new Vector2(x, 0);
            }
            x += zeroMarginRight * width / 2;
            return new Vector2(x + feedPosition * width * positionsFraction / nPositions, 0 );
        }
    }
    public void Spawn(Sprite sprite, int position, int nPositions)
    {
        feedPosition = position;
        this.nPositions = nPositions;
        image.sprite = sprite;
        RectTransform t = (transform as RectTransform);
        t.anchoredPosition = targetAnchoredPosition;
        gameObject.SetActive(true);
    }

    public void SnapToPosition(int position)
    {
        feedPosition = position;
        RectTransform t = (transform as RectTransform);
        t.anchoredPosition = targetAnchoredPosition;
    }

    public void SnapToPosition()
    {
        SnapToPosition(feedPosition);
    }

    public int ShiftRight()
    {
        if (beingPulled) return feedPosition;
        SnapToPosition(feedPosition + 1);
        return feedPosition;
    }

    public int ShiftLeft()
    {
        if (beingPulled) return feedPosition;
        SnapToPosition(feedPosition - 1);
        return feedPosition;
    }

    public void PlayCard()
    {
        gameObject.SetActive(false);
    }
}
