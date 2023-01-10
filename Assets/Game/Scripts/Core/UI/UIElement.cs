using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Animator))]
public class UIElement : MonoBehaviour
{
    public bool initializeAutomatically = true;
    public bool followStateTransitions = true;
    [Space]

    private Animator _animator;

    private int _appearanceAnimationHash;
    private int _disappearanceAnimationHash;

    private bool _isTransitionAvailable = true;

    public bool Animating => _isTransitionAvailable;

    public virtual void Awake()
    {
        if (initializeAutomatically)
        {
            Initialize();
        }
    }

    public virtual void Initialize()
    {
        _animator = GetComponent<Animator>();

        _appearanceAnimationHash = Animator.StringToHash("Show");
        _disappearanceAnimationHash = Animator.StringToHash("Hide");

        if (_animator.runtimeAnimatorController)
        {
            _animator.ResetTrigger(_appearanceAnimationHash);
            _animator.ResetTrigger(_disappearanceAnimationHash);
        }

        if (followStateTransitions)
        {
            StartCoroutine(RegisteringCoroutine());
        }

        HideImmediate();
    }

    public virtual void Show(float appearanceDelay = 0, bool useAnimation = true, System.Action onShown = null)
    {
        if (_isTransitionAvailable)
        {
            if (appearanceDelay == 0)
            {
                ShowImmediate();

                if (useAnimation && _animator.runtimeAnimatorController)
                {
                    _animator.SetTrigger(_appearanceAnimationHash);
                    _animator.ResetTrigger(_disappearanceAnimationHash);
                }

                if (onShown != null)
                {
                    onShown();
                }
            }
            else
            {
                DOVirtual.DelayedCall(appearanceDelay, () =>
                {
                    ShowImmediate();

                    if (useAnimation && _animator.runtimeAnimatorController)
                    {
                        _animator.SetTrigger(_appearanceAnimationHash);
                        _animator.ResetTrigger(_disappearanceAnimationHash);
                    }

                    if (onShown != null)
                    {
                        onShown();
                    }
                });
            }


        }
    }

    public virtual void Hide(float disappearanceDelay = 0, bool useAnimation = true, System.Action onHiden = null)
    {
        if (_isTransitionAvailable)
        {
            if (disappearanceDelay == 0)
            {
                if (useAnimation && _animator.runtimeAnimatorController)
                {
                    _animator.SetTrigger(_disappearanceAnimationHash);
                    _animator.ResetTrigger(_appearanceAnimationHash);
                }
                else
                {
                    HideImmediate();
                }

                if (onHiden != null)
                {
                    onHiden();
                }
            }
            else
            {
                DOVirtual.DelayedCall(disappearanceDelay, () =>
                {
                    if (useAnimation && _animator.runtimeAnimatorController)
                    {
                        _animator.SetTrigger(_disappearanceAnimationHash);
                        _animator.ResetTrigger(_appearanceAnimationHash);
                    }
                    else
                    {
                        HideImmediate();
                    }

                    if (onHiden != null)
                    {
                        onHiden();
                    }
                });
            }
        }
    }

    public virtual void ShowImmediate()
    {
        gameObject.SetActive(true);
    }

    public virtual void HideImmediate()
    {
        transform.localScale = Vector3.one;

        gameObject.SetActive(false);
    }

    public void BlockTransitions()
    {
        _isTransitionAvailable = false;
    }

    public void UnblockTransitions()
    {
        _isTransitionAvailable = true;
    }

    private IEnumerator RegisteringCoroutine()
    {
        while (!UIManager.Instance) { yield return null; }

        UIManager.Instance.RegisterElement(this);
    }
}