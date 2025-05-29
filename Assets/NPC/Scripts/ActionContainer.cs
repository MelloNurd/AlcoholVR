using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class ActionContainer : MonoBehaviour
{
    public List<Action> _actions = new();

    public SortMode actionSortMode; // How it determines the next action to play
    [ShowIf("actionSortMode", SortMode.Random)] public bool includeCurrentAction = false; // If it should be able to play the same action again immediately

    public TimeBetweenActions delaySpeed;

    [Min(1)] public int minActions = 1;
    [Min(1)] public int maxActions = 5;

    private int _currentActionIndex = -1;

    private void Start()
    {
        _currentActionIndex = GetFirstIndex();
    }

    public Action GetNextAction() => GetNextAction(out _);
    public Action GetNextAction(out float seconds)
    {
        if(_actions.Count == 0)
        {
            Debug.LogWarning("No actions available on {" + gameObject.name + "}. Returning default action.");
            seconds = 0f;
            return default;
        }

        int index = -1;
        switch (actionSortMode)
        {
            case SortMode.Random:
                int threshold = 0;
                do
                {
                    index = Random.Range(0, _actions.Count);
                }
                while (index == _currentActionIndex && !includeCurrentAction && threshold++ < 50);
                if (threshold >= 50)
                {
                    Debug.LogWarning("Could not find a new action after 50 attempts. Returning current action.");
                    index = _currentActionIndex;
                }
                break;
            case SortMode.RoundRobin:
                index = (_currentActionIndex + 1) % _actions.Count;
                break;
            case SortMode.RoundRobinReverse:
                index = (_currentActionIndex - 1 + _actions.Count) % _actions.Count;
                break;
        }

        _currentActionIndex = index;
        seconds = _actions[index % _actions.Count].animToPlay.length;

        return _actions[index % _actions.Count];
    }

    public Action GetActionByAnimation(AnimationClip anim) => GetActionByAnimation(anim, out _);
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

    private int GetFirstIndex()
    {
        return actionSortMode switch
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
