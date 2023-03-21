using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorListener
{
    private Animator animator;

    private AnimatorStateInfo animatorStateInfo;

    private float animationStartTime;
    private float animationNormalizedTime;

    private int requiredShortNameHash;

    private bool isFrameReached;
    private bool isAnimationStarted;

    public AnimatorListener(Animator animator)
    {
        this.animator = animator;
    }

    public void ListenToState(string animatorStateName)
    {
        requiredShortNameHash = Animator.StringToHash(animatorStateName);
    }

    public bool IsFrameReached(float time)
    {
        animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (animatorStateInfo.shortNameHash == requiredShortNameHash)
        {
            if (!isAnimationStarted)
            {
                isAnimationStarted = true;

                animationStartTime = Time.timeSinceLevelLoad;
            }

            animationNormalizedTime = Mathf.Repeat(Time.timeSinceLevelLoad - animationStartTime, animatorStateInfo.length);

            //Debug.Log($" - T: {animationNormalizedTime}");

            if (animationNormalizedTime >= time)
            {
                if (!isFrameReached)
                {
                    return isFrameReached = true;
                }
            }
            else
            {
                isFrameReached = false;
            }
        }

        return false;
    }

    public void Reset()
    {
        isAnimationStarted = false;
    }
}