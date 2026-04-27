using EditorAttributes;
using UnityEngine;

public enum EyebrowsState
{
    Neutral = 0,
    Mad,
    Sad,
    Raised,
    Lowered
}

public enum MouthState
{
    Neutral = 0,
    Smile,
    Frown
}

public class FacialExpressions : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    // Eyebrows
    [Button]
    public void SetEyebrows(EyebrowsState state)
    {
        if (!Application.isPlaying) return;
        if (state == EyebrowsState.Neutral)
        {
            ResetEyebrows();
            return;
        }

        string triggerName = "StartEyebrows" + state.ToString();
        Debug.Log($"Setting eyebrows to {triggerName} on {gameObject.name}", gameObject);

        _animator.SetTrigger(triggerName);
    }

    [Button]
    public void ResetEyebrows()
    {
        if (!Application.isPlaying) return;
        _animator.SetTrigger("StartResetEyebrows");
    }

    // Mouth
    [Button]
    public void SetMouth(MouthState state)
    {
        if (!Application.isPlaying) return;
        if (state == MouthState.Neutral)
        {
            ResetMouth();
            return;
        }

        string triggerName = "Start" + state.ToString() + "Mouth";
        Debug.Log($"Setting mouth to {triggerName} on {gameObject.name}", gameObject);

        _animator.SetTrigger(triggerName);
    }

    [Button]
    public void ResetMouth()
    {
        if (!Application.isPlaying) return;
        _animator.SetTrigger("StartResetMouth");
    }
}