using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[SelectionBase]
public class NPC : MonoBehaviour
{
    [SerializeReference]
    public List<Sequence> Sequences = new List<Sequence>();
    public int CurrentSequenceIndex = 0;

    // Component References
    private NavMeshAgent _agent;
    private Animator _animator;
    private XRGrabInteractable _grabInteractable;
    private AudioSource _audioSource;

    public void Awake()
    {
        _agent = GetComponentInChildren<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _grabInteractable = GetComponentInChildren<XRGrabInteractable>();
        _audioSource = GetComponentInChildren<AudioSource>();
    }

    private async void Start()
    {
        await UniTask.Yield(); // Delay to ensure all components are initialized before executing the first sequence
        ExecuteSequence();
    }

    public void SetSequenceIndex(int index)
    {
        if (index < 0 || index >= Sequences.Count)
        {
            Debug.LogWarning($"Attempted to set sequence index to {index}, but it is out of bounds.");
            return;
        }

        CurrentSequenceIndex = index;
    }

    public void ExecuteSequence()
    {
        if (CurrentSequenceIndex < 0 || CurrentSequenceIndex >= Sequences.Count)
        {
            Debug.LogWarning($"Attempted to play sequence with index {CurrentSequenceIndex}, but it is out of bounds.");
            return;
        }

        Sequences[CurrentSequenceIndex].Execute(this);
    }

    public void PlayAnimation(AnimationClip clip)
    {
        PlayAnimationAsync(clip).Forget();
    }

    public async UniTask PlayAnimationAsync(AnimationClip clip)
    {
        _animator.CrossFade(clip.name, 0.15f);
        await UniTask.Delay(clip.length.ToMS());
    }
}