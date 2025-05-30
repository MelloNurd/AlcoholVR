using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class ActionContainer : MonoBehaviour
{
    public List<Action> _actions = new();

    public SortMode actionSortMode = SortMode.RoundRobin; // How it determines the next action to play
    [ShowIf("actionSortMode", SortMode.Random)] public bool excludeCurrentAction = false; // If it should be able to play the same action again immediately

    public Timing timeBetweenActions = Timing.Normal;
    public float AnimationDelay
    { 
        get {
            return timeBetweenActions switch
            {
                Timing.Slow => 8,
                Timing.Normal => 3f,
                Timing.Fast => 1f,
                Timing.Instant => 0f,
                _ => 5f
            };
        }
    }

    [Min(1)] public int minActions = 1;
    [Min(1)] public int maxActions = 5;

    private int _currentActionIndex = -1;

    private void Start()
    {
        GetFirstIndex();
    }

    public Action GetAction(out int miliseconds) // This will autoincrement the index after getting the current action
    {
        if(_actions.Count == 0)
        {
            Debug.Log("No actions available in \"" + gameObject.name + "\". Returning default.");
            miliseconds = 0;
            return null; // Return a default action or handle the case where there are no actions
        }

        if (_currentActionIndex < 0) GetFirstIndex();

        Action action = _actions[_currentActionIndex];
        miliseconds = action.animToPlay ? Mathf.RoundToInt(action.animToPlay.length * 1000) : 0;

        int index = -1;
        switch (actionSortMode)
        {
            case SortMode.Random:
                do index = Random.Range(0, _actions.Count);
                while (excludeCurrentAction && _currentActionIndex == index);
                break;
            case SortMode.RoundRobin:
                index = (_currentActionIndex + 1) % _actions.Count; // Move to the next checkpoint in a round-robin fashion
                break;
            case SortMode.RoundRobinReverse:
                index = (_currentActionIndex - 1 + _actions.Count) % _actions.Count; // Move to the next checkpoint in a round-robin fashion
                break;
        }
        _currentActionIndex = index;

        return action;
    }

    public Action GetActionByAnimation(AnimationClip anim, out float seconds)
    {
        foreach (var action in _actions)
        {
            if (action.animToPlay == anim)
            {
                seconds = action.animToPlay.length;
                return action;
            }
        }
        seconds = 0f;
        return default;
    }

    private void GetFirstIndex()
    {
        _currentActionIndex = actionSortMode switch
        {
            SortMode.Random => Random.Range(0, _actions.Count),
            SortMode.RoundRobin => 0,
            SortMode.RoundRobinReverse => _actions.Count - 1,
            _ => -1
        };
    }

    void OnDrawGizmosSelected() // This is so the transform is visible in the scene view
    {
        Gizmos.DrawIcon(transform.position, null, true, Color.yellow);
    }
}
