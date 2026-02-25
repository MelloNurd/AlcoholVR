using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public abstract class Sequence
{
    public UnityEvent OnSequenceBegan = new();
    public UnityEvent OnSequenceComplete = new();

    public bool NextSequenceOnEnd = false;

    public abstract UniTask Execute(NPC npc);
}

public class WaitSequence : Sequence
{
    public float Time = 0f;

    public override async UniTask Execute(NPC npc)
    {
        OnSequenceBegan.Invoke();

        await UniTask.Delay(Time.ToMS());

        OnSequenceComplete.Invoke();
    }
}

public class AnimateSequence : Sequence
{
    public AnimationClip Animation;

    public override async UniTask Execute(NPC npc)
    {
        OnSequenceBegan.Invoke();

        await npc.PlayAnimationAsync(Animation);

        OnSequenceComplete.Invoke();
    }
}

public class DialogueSequence : Sequence
{
    public string DialogueText;

    public override async UniTask Execute(NPC npc)
    {
        OnSequenceBegan.Invoke();
        // Placeholder for dialogue system integration
        Debug.Log($"NPC says: {DialogueText}");
        await UniTask.Delay(2000); // Simulate time taken for dialogue
        OnSequenceComplete.Invoke();
    }
}

public class WaitForItemSequence : Sequence
{
    public override async UniTask Execute(NPC npc)
    {
        OnSequenceBegan.Invoke();
        
        

        OnSequenceComplete.Invoke();
    }
}