using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[Serializable] 
public class Action
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

public enum Timing
{
    Slow, // 10 seconds between
    Normal, // 5 seconds between
    Fast, // 2 seconds between
    Instant, // 0 seconds between
}