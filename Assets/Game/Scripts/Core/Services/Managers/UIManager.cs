using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum UIState { Empty, Start, ActionPhase, Success, Fail, Settings, Tutorial, Countdown }

public class UIManager : Service<UIManager>
{
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private EventTrigger _backgroundTrigger;
    [Space]
    [SerializeField] private List<UIStateView> _stateViews;
    [Space]
    [SerializeField] private GameObject _cursor;
    [SerializeField] private bool _isCursorVisible = true;

    private List<UIElement> _uiElements;
    private List<UIElement> _stateDisabledUiElements;

    private UIStateView _requestedView;

    private Transform _cursorClickContainer;

    private static UIState _currentUiState;

    private static bool _emptySpaceInteraction;

    public static UIState CurrentState => _currentUiState;

    public static bool EmptySpaceInteraction => _emptySpaceInteraction;

    public override void Initialize()
    {
        _uiElements = new List<UIElement>();

        DontDestroyOnLoad(_eventSystem);
        DontDestroyOnLoad(_canvas);

        SetBasicBackgroundTriggerEvents();

        if (_cursor)
        {
            _cursor.gameObject.SetActive(_isCursorVisible);

            _cursorClickContainer = _cursor.transform.GetChild(0);
        }

        base.Initialize();
    }

    private void Update()
    {
        if (_isCursorVisible)
        {
            if (_cursor)
            {
                Cursor.visible = false;

                _cursor.transform.position = Vector3.Lerp(_cursor.transform.position, InputManager.touchPosition, 0.3f);
                _cursorClickContainer.localScale = new Vector3(1f, Input.GetMouseButton(0) ? 0.75f : 1f, 1f);
            }
            else
            {
                Cursor.visible = true;
            }
        }
        else
        {
            Cursor.visible = false;
        }
    }

    public void RegisterElement(UIElement newElement)
    {
        _uiElements.Add(newElement);

        print($" - UI element of '{newElement.gameObject.name}' registered");
    }

    public void ChangeState(UIState state, float delay = 0)
    {
        StartCoroutine(StateChangingCoroutine(state, delay));
    }

    public void ResetView()
    {
        for (int i = 0; i < _uiElements.Count; i++)
        {
            _uiElements[i].HideImmediate();
        }
    }

    public void SetBackgroundTriggerEvent(EventTriggerType eventTriggerType, Action newEvent)
    {
        _backgroundTrigger.AddEvent(eventTriggerType, newEvent);

        SetBasicBackgroundTriggerEvents();
    }

    public void RemoveBackgroundTriggerEvents(EventTriggerType eventTriggerType)
    {
        _backgroundTrigger.RemoveAllListeners(eventTriggerType);

        SetBasicBackgroundTriggerEvents();
    }

    public void RemoveBackgroundTriggerEvents()
    {
        _backgroundTrigger.RemoveAllListeners();

        SetBasicBackgroundTriggerEvents();
    }

    private void SetBasicBackgroundTriggerEvents()
    {
        _backgroundTrigger.AddEvent(EventTriggerType.PointerDown, () => { _emptySpaceInteraction = true; });
        _backgroundTrigger.AddEvent(EventTriggerType.PointerUp, () => { _emptySpaceInteraction = false; });
    }

    private void OnValidate()
    {
        for (int i = 0; i < _stateViews.Count; i++)
        {
            _requestedView = _stateViews[i];

            _requestedView.title = _stateViews[i].uIState.ToString();

            for (int j = 0; j < _requestedView.activeElements.Length; j++)
            {
                if (_requestedView.activeElements[j].elementReference)
                {
                    _requestedView.activeElements[j].title = _requestedView.activeElements[j].elementReference.name;
                }
            }

            _stateViews[i] = _requestedView;
        }
    }

    private IEnumerator StateChangingCoroutine(UIState state, float delay)
    {
        _requestedView = _stateViews.Find((v) => state == v.uIState);

        if (_requestedView != null)
        {
            _stateDisabledUiElements = new List<UIElement>(_uiElements);

            yield return new WaitForSeconds(delay);

            UIElementData currentStateElementData;

            for (int i = 0; i < _requestedView.activeElements.Length; i++)
            {
                currentStateElementData = _requestedView.activeElements[i];

                currentStateElementData.elementReference.Show(currentStateElementData.transitionDelay, currentStateElementData.useAnimation);

                _stateDisabledUiElements.Remove(currentStateElementData.elementReference);
            }

            for (int i = 0; i < _stateDisabledUiElements.Count; i++)
            {
                _stateDisabledUiElements[i].Hide();
            }

            _currentUiState = state;
        }
    }
}

[Serializable]
public class UIStateView
{
    [HideInInspector] public string title;

    public UIState uIState;
    public UIElementData[] activeElements;
}

[Serializable]
public struct UIElementData
{
    [HideInInspector] public string title;

    public UIElement elementReference;
    public bool useAnimation;
    [Space]
    public float transitionDelay;
}
