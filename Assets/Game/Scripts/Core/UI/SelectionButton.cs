using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(Button))]
public class SelectionButton : MonoBehaviour
{
    [SerializeField] protected Image _icon;
    [SerializeField] protected SelectionGroupScrollRect _groupScrollRect;

    private List<SelectionButton> _groupOtherButtons;

    private Vector3 touchPivot;

    private float baseScale;

    public int buttonIndex;
    protected Action<int> onSelected;

    public bool isSelected;
    public bool isSelectable;

    private bool isPressing;
    private bool isScrollingInitiated;

    public Button Button { get; private set; }
    public Image ButtonImage { get; private set; }

    private void Awake()
    {
        baseScale = transform.localScale.x;
    }

    public virtual void Initialize(List<SelectionButton> selectionGroupButtons)
    {
        Button = GetComponent<Button>();
        ButtonImage = GetComponent<Image>();

        _groupOtherButtons = new List<SelectionButton>(selectionGroupButtons);
        _groupOtherButtons.Remove(this);

        this.ApplySelectionAttributes(OnPressed, OnReleased);
    }

    private void Update()
    {
        if (isPressing)
        {
            if (_groupScrollRect && !isScrollingInitiated)
            {
                if ((Input.mousePosition - touchPivot).magnitude > _groupScrollRect.scrollThreshold)
                {
                    isScrollingInitiated = true;

                    _groupScrollRect.StartScrolling();
                }
            }
        }
    }

    public void Select()
    {
        if (isSelectable)
        {
            AnimateSelection();
            OnSelected();

            isSelected = true;
        }
    }

    public void Deselect()
    {
        if (isSelected)
        {
            AnimateDeselection();
            OnDeselected();

            isSelected = false;
        }
    }

    protected virtual void OnPressed()
    {
        touchPivot = Input.mousePosition;

        isPressing = true;
        isScrollingInitiated = false;
    }

    protected virtual void OnReleased()
    {
        isPressing = false;

        if (isSelectable && isSelected)
        {
            AnimateSelection();
        }

        if (isScrollingInitiated)
        {
            _groupScrollRect.StopScrolling();
        }
        else
        {
            Select();
        }
    }

    private void AnimateSelection(float upscaleValue = 0.15f, float pulseDuration = 0.15f)
    {
        transform.DOScale(baseScale * (1f + upscaleValue), pulseDuration);
    }

    private void AnimateDeselection(float pulseDuration = 0.15f)
    {
        transform.DOScale(baseScale, pulseDuration);
    }

    protected virtual void OnSelected()
    {
        for (int i = 0; i < _groupOtherButtons.Count; i++)
        {
            _groupOtherButtons[i].Deselect();
        }
    }

    protected virtual void OnDeselected()
    {

    }

    public virtual void ApplyInfo(Sprite iconSprite)
    {
        _icon.sprite = iconSprite;
    }
    
    public void SetSelectionListener(Action<int> listener)
    {
        onSelected = listener;
    }
    
    public void OnDestroy()
    {
        onSelected = null;
    }
}
