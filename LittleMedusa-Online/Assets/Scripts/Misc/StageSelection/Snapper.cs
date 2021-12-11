using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Snapper : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public float snapSpeed;

    public RectTransform viewPort;
    public RectTransform contentRect;
    public HorizontalLayoutGroup horizontalLayoutGroup;

    public float childWidth;
    float centrePositionX;
    float spacing;
    public float maxX;
    public float minX;

    public int selectedIndex;
    public float contentAbsPosValue;

    public UnityEvent onFingerUp;
    public UnityEvent onFingerDown;

    bool touch;

    public void InitialiseSnapper()
    {
        centrePositionX = viewPort.rect.width / 2f;
        spacing = horizontalLayoutGroup.spacing;
        maxX = centrePositionX;
        minX = centrePositionX - (contentRect.transform.childCount - 1) * (spacing + childWidth);
    }

    void OnTouchBegin()
    {
        onFingerDown?.Invoke();
    }

    void OnTouchOver()
    {
        onFingerUp?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (SnapCor != null)
        {
            if(!touch)
            {
                OnTouchBegin();
                touch = true;
            }
            StopCoroutine(SnapCor);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!touch)
        {
            OnTouchBegin();
            touch = true;
        }
        Vector2 movePosition = contentRect.anchoredPosition;
        movePosition += new Vector2(eventData.delta.x, 0f);
        float movePositionClampedX = Mathf.Clamp(movePosition.x, minX, maxX);
        contentRect.anchoredPosition = new Vector2(movePositionClampedX, movePosition.y);


        UpdateContentNormalisedPosition();
        UpdateSelectedIndex();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (touch)
        {
            OnTouchOver();
            touch = false;
        }
        SnapCor = SnapToCentre();
        StartCoroutine(SnapCor);
    }

    IEnumerator SnapCor;

    IEnumerator SnapToCentre()
    {
        UpdateContentNormalisedPosition();
        UpdateSelectedIndex();

        Vector2 snapPosition = new Vector2(centrePositionX - selectedIndex * (spacing + childWidth), contentRect.anchoredPosition.y);

        while (Vector2.Distance(contentRect.anchoredPosition, snapPosition) >= 0.05f)
        {
            contentRect.anchoredPosition = Vector2.Lerp(contentRect.anchoredPosition, snapPosition, Time.fixedDeltaTime * snapSpeed);
            UpdateContentNormalisedPosition();
            UpdateSelectedIndex();
            yield return new WaitForFixedUpdate();
        }
        contentRect.anchoredPosition = snapPosition;
        yield break;
    }

    public void InitialiseStageSelection(int previouslySelectedMap)
    {
        selectedIndex = previouslySelectedMap;
        contentRect.anchoredPosition = new Vector2(centrePositionX - selectedIndex * (spacing + childWidth), contentRect.anchoredPosition.y);
    }

    void UpdateContentNormalisedPosition()
    {
        contentAbsPosValue = maxX-contentRect.anchoredPosition.x;
    }

    void UpdateSelectedIndex()
    {
        float percent = (contentAbsPosValue / (maxX - minX));
        float index = percent * (contentRect.transform.childCount - 1);
        selectedIndex = Mathf.RoundToInt(index);
    }
}

