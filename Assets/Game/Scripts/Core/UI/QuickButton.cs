using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MoreMountains.NiceVibrations;
using DG.Tweening;

[DisallowMultipleComponent][AddComponentMenu("UI/Quick Button")]
public class QuickButton : MonoBehaviour
{
    public string title;
    public HapticTypes hapticMode = HapticTypes.LightImpact;
    public EventTriggerType triggerType = EventTriggerType.PointerUp;
    [Space]
    public QuickButtonAnimationSettings animationSettings;
    [Space]
    [SerializeField]
    private bool interactable = true;
    [Space]
    public QuickButtonContent content;
    [Space]
    public UnityEvent onClick;

    private EventTrigger _eventTrigger;

    private float _baseScale;
    private float _pulseHalfDuration;

    private bool _isInitialized;

    public bool Interactable { get { return interactable; } set { interactable = value; if (content.blockingOverlay) { content.blockingOverlay.SetActive(!interactable); } } }

    private void Awake()
    {
        if (!_isInitialized)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        _baseScale = transform.localScale.x;
        _pulseHalfDuration = animationSettings.pulseDuration / 2f;

        _eventTrigger = gameObject.AddComponent<EventTrigger>();

        if (onClick.GetPersistentEventCount() > 0)
        {
            SetEvent(onClick.Invoke);
        }

        _isInitialized = true;
    }

    public void SetEvent(Action newEvent, string newTitle = "")
    {
        _eventTrigger.RemoveAllListeners();

        _eventTrigger.AddEvent(triggerType, () => { if (interactable) { newEvent(); } } );

        InitializeAnimationEvents();

        if (newTitle.Length > 0)
        {
            SetTitle(newTitle);
        }
    }

    public void AddEvent(Action newEvent, string newTitle = "")
    {
        onClick.AddListener(() => { if (interactable) { newEvent(); } });

        if (newTitle.Length > 0)
        {
            SetTitle(newTitle);
        }
    }

    private void SetTitle(string newTitle)
    {
        title = newTitle;

        content.buttonText.text = newTitle;
    }

    private void InitializeAnimationEvents()
    {
        _eventTrigger.AddEvent(EventTriggerType.PointerUp, () => transform.DOScale(_baseScale, _pulseHalfDuration));
        _eventTrigger.AddEvent(EventTriggerType.PointerDown, () => { if (interactable) { transform.DOScale(_baseScale * (1f - animationSettings.pulseAmplitude), _pulseHalfDuration); AppManager.Instance.PlayHaptic(hapticMode); } });
    }

    private void OnValidate()
    {
        if (content.buttonText)
        {
            content.buttonText.text = title;
        }

        if (content.blockingOverlay)
        {
            Interactable = interactable;
        }
    }

    [Serializable]
    public class QuickButtonAnimationSettings
    {
        public float pulseDuration = 0.15f;
        public float pulseAmplitude = 0.03f;
    }

    [Serializable]
    public struct QuickButtonContent
    {
        public Text buttonText;
        public GameObject blockingOverlay;
    }
}
