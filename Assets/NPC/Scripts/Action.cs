using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct Action
{
    public AnimationClip animToPlay;
    public UnityEvent OnActionStart;
    public UnityEvent OnActionEnd;
}

public enum SortMode
{
    RoundRobin,
    RoundRobinReverse,
    Random,
}

public enum TimeBetweenActions // WIP
{
    Slow,
    Normal,
    Fast,
    Instant,
}