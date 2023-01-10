using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickAnimator : MonoBehaviour
{
    public AnimationClip[] animations;
    public bool playOnActive;

    private AnimationPlayer _animationPlayer;

    private void Awake()
    {
        _animationPlayer = new AnimationPlayer(gameObject.AddComponent<Animation>(), animations);
    }

    private void LateUpdate()
    {
        if (playOnActive)
        {
            if (_animationPlayer != null)
            {
                if (gameObject.activeSelf)
                {
                    playOnActive = false;

                    _animationPlayer.Play(0);
                }
            }
        }
    }
}
