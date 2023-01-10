using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationPlayer
{
    public Animation component;

    private List<AnimationState> _animationStates;

    private int _lastClipIndex = -1;

    public bool IsPlaying => component.isPlaying;

    public AnimationPlayer(Animation animationComponent)
    {
        component = animationComponent;

        _animationStates = new List<AnimationState>();

        foreach (AnimationState state in animationComponent)
        {
            _animationStates.Add(state);
        }
    }

    public AnimationPlayer(Animation animationComponent, AnimationClip[] clips)
    {
        component = animationComponent;

        for (int i = 0; i < clips.Length; i++)
        {
            component.AddClip(clips[i], clips[i].name);
        }

        _animationStates = new List<AnimationState>();

        foreach (AnimationState state in animationComponent)
        {
            _animationStates.Add(state);
        }
    }

    public void Play(Action onPlayed = null)
    {
        component.Play();

        if (onPlayed != null)
        {
            DOVirtual.DelayedCall(_animationStates[0].length + 0.1f, () => onPlayed());
        }
    }

    public void Play(int clipIndex, Action onPlayed = null)
    {
        _lastClipIndex = clipIndex;

        component.Play(_animationStates[clipIndex].name);

        if (onPlayed != null)
        {
            DOVirtual.DelayedCall(_animationStates[clipIndex].length + 0.1f, () => onPlayed());
        }
    }

    public void PlayNext(bool loop = true)
    {
        Play(_lastClipIndex < _animationStates.Count - 1 ? _lastClipIndex + 1 : (loop ? 0 : _animationStates.Count - 1));
    }

    public void PlayAll()
    {
        foreach (AnimationState state in _animationStates)
        {
            component.PlayQueued(state.name);
        }
    }

    public void Stop()
    {
        component.Stop();
    }
}
