using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionGroupScrollRect : MonoBehaviour
{
    public RectTransform content;
    public RectTransform viewport;
    [Space]
    public float scrollThreshold;

    Vector2 touchPivot;

    float scrollPivotCoordX;

    float minScrollCoordX;
    float maxScrollCoordX;

    bool isScrolling;

    bool isInitialized;

    public void Initialize()
    {
        if (!isInitialized)
        {
            ResetContentPosition();
            UpdateScrollBorders();
            AddTriggerOnContent();

            isInitialized = true;
        }
    }

    void Update()
    {
        if (isScrolling)
        {
            content.localPosition = new Vector3(Mathf.Clamp(scrollPivotCoordX + Input.mousePosition.x - touchPivot.x, minScrollCoordX, maxScrollCoordX), 0, 0);
        }
    }

    public void StartScrolling()
    {
        touchPivot = Input.mousePosition;

        scrollPivotCoordX = content.localPosition.x;

        isScrolling = true;
    }

    public void StopScrolling()
    {
        isScrolling = false;
    }

    void ResetContentPosition()
    {
        content.offsetMax += new Vector2(content.offsetMin.x, 0);
        content.offsetMin = new Vector2(0, content.offsetMin.y);

        //content.localPosition += new Vector3((viewport.position.x - viewport.rect.width / 2f) - (content.position.x - content.rect.width / 2f), 0, 0);
    }

    void UpdateScrollBorders()
    {
        minScrollCoordX = (viewport.localPosition.x + viewport.rect.width / 2f) - (content.localPosition.x + content.rect.width / 2f) + content.localPosition.x;
        maxScrollCoordX = (viewport.localPosition.x - viewport.rect.width / 2f) - (content.localPosition.x - content.rect.width / 2f) + content.localPosition.x;
    }

    void AddTriggerOnContent()
    {
        if (content.GetComponent<EventTrigger>())
        {
            GameObject.Destroy(content.GetComponent<EventTrigger>());
        }

        EventTrigger trigger = content.gameObject.AddComponent<EventTrigger>();

        var pointerUp = new EventTrigger.Entry();
        var pointerDown = new EventTrigger.Entry();

        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((e) => StopScrolling());

        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((e) => StartScrolling());

        trigger.triggers.Add(pointerUp);
        trigger.triggers.Add(pointerDown);
    }
}
