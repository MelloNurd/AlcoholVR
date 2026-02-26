using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[SelectionBase]
public class NPC : MonoBehaviour
{
    [SerializeReference]
    public List<Sequence> Sequences = new List<Sequence>();

    public int CurrentSequenceIndex = 0;

    public UnityEvent OnNPCInteract = new();

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

        _grabInteractable.selectEntered.AddListener((args) => OnNPCInteract.Invoke());
    }

    private async void Start()
    {
        await UniTask.Yield();
        
        if (CurrentSequenceIndex >= 0 && CurrentSequenceIndex < Sequences.Count)
        {
            Sequences[CurrentSequenceIndex].StartSequence(this);
        }
    }

    private void Update()
    {
        if (CurrentSequenceIndex < 0 || CurrentSequenceIndex >= Sequences.Count)
            return;

        Sequences[CurrentSequenceIndex].UpdateSequence(this);
    }

    public void StartNextSequence()
    {
        if (CurrentSequenceIndex < 0 || CurrentSequenceIndex >= Sequences.Count)
        {
            Debug.LogWarning($"Current sequence {CurrentSequenceIndex} is out of bounds.");
            return;
        }

        Sequences[CurrentSequenceIndex].EndSequence(this);

        CurrentSequenceIndex++;

        if (CurrentSequenceIndex < Sequences.Count)
        {
            Sequences[CurrentSequenceIndex].StartSequence(this);
        }
    }

    public void StartSequence(int index)
    {
        if (index < 0 || index >= Sequences.Count)
        {
            Debug.LogWarning($"Sequence index {index} is out of bounds.");
            return;
        }
        if (CurrentSequenceIndex >= 0 && CurrentSequenceIndex < Sequences.Count)
        {
            Sequences[CurrentSequenceIndex].EndSequence(this);
        }
        CurrentSequenceIndex = index;
        Sequences[CurrentSequenceIndex].StartSequence(this);
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