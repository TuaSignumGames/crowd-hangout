using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UICountdownPanel : UIElement
{
    public static UICountdownPanel Instance;

    [SerializeField] private UIElement[] _countdownElements;
    [Space]
    [SerializeField] private float _tickDuration;

    public float goDuration;

    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    public override void ShowImmediate()
    {
        base.ShowImmediate();

        StartCoroutine(CountdownCoroutine());
    }

    public override void HideImmediate()
    {
        base.HideImmediate();

        _isFinished = false;
    }

    private IEnumerator CountdownCoroutine()
    {
        float preDelay = _tickDuration * 0.2f;
        float postDelay = _tickDuration * 0.8f;

        for (int i = 0; i <= _countdownElements.Length; i++)
        {
            if (i > 0)
            {
                _countdownElements[i - 1].Hide();
            }

            yield return new WaitForSeconds(preDelay);

            if (i < _countdownElements.Length)
            {
                _countdownElements[i].Show(onShown: () => { if (i == _countdownElements.Length - 1) { _isFinished = true; } });
            }

            yield return new WaitForSeconds(postDelay);
        }

        yield return new WaitForSeconds(1f);

        HideImmediate();
    }
}
