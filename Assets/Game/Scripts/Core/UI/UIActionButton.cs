using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIActionButton : MonoBehaviour
{
    public static UIActionButton Instance;

    public Animation animationComponent;
    public float transitionDuration;

    private Evaluator _evaluator;

    private AnimationPlayer _animationPlayer;

    private float _scaleFactor;

    private void Awake()
    {
        Instance = this;

        _evaluator = new Evaluator(MonoUpdateType.FixedUpdate);
        _animationPlayer = new AnimationPlayer(animationComponent);

        HideImmediate();
    }

    private void FixedUpdate()
    {
        if (_evaluator.Iterating)
        {
            _evaluator.Iterate(ref _scaleFactor);

            transform.localScale = new Vector3(_scaleFactor, _scaleFactor, _scaleFactor);
        }
    }

    public void Show()
    {
        if (_scaleFactor == 0)
        {
            gameObject.SetActive(true);

            _animationPlayer.Play(0);

            _evaluator.Setup(0, 1f, transitionDuration, EvaluationType.Linear);
        }
    }

    public void Hide()
    {
        if (_scaleFactor == 1f)
        {
            animationComponent.Stop();

            _evaluator.Setup(1f, 0, transitionDuration, EvaluationType.Linear, () => gameObject.SetActive(false));
        }
    }

    public virtual void ShowImmediate()
    {
        gameObject.SetActive(true);
    }

    public virtual void HideImmediate()
    {
        transform.localScale = Vector3.zero;

        gameObject.SetActive(false);
    }
}
