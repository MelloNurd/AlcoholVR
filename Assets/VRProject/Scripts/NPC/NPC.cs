using System;
using System.Collections.Generic;
using EditorAttributes;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    public enum NPCState
    {
        Idle,
        ExecutingSequence,
        InDialogue
    }

    // NPC VARIABLES
    public bool isDrunk = false;
    public bool canInteract = true;

    // SEQUENCES

    // Flags
    [Tooltip("Should the NPC start executing its sequences as soon as the scene starts?")]
    public bool startSequencesOnAwake = true;
    [Tooltip("When reaching the end of the NPC's sequences, should it start over from the beginning?")]
    public bool loopSequences = false;

    // Sequences
    [SerializeReference, SerializeReferenceDropdown]
    public List<Sequence> sequences = new();

    [ReadOnly, SerializeField] private int _currentSequenceIndex = 0;
    [ReadOnly] public NPCState _currentState = NPCState.Idle;

    // Component references
    public Animator Animator { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public DialogueSystem DialogueSystem { get; private set; }
    public AudioSource AudioSource { get; private set; }

    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();
        DialogueSystem = GetComponent<DialogueSystem>();
        AudioSource = GetComponent<AudioSource>();
        Agent = GetComponentInChildren<NavMeshAgent>();
    }

    private void Start()
    {
        InitializeSequences();
    }

    #region Sequencing Methods
    private void InitializeSequences()
    {
        foreach (var sequence in sequences)
            sequence.npc = this;

        if (startSequencesOnAwake && sequences.Count > 0)
            ExecuteCurrentSequence();
    }

    public void ExecuteCurrentSequence()
    {
        if (_currentSequenceIndex >= sequences.Count || _currentSequenceIndex < 0)
            return;

        var sequence = sequences[_currentSequenceIndex];
        if(sequence == null)
        {
            ExecuteNextSequence();
            return;
        }

        _currentState = NPCState.ExecutingSequence;
        sequence.Execute();
    }

    public void ExecuteNextSequence()
    {
        _currentSequenceIndex++;
        if (_currentSequenceIndex < sequences.Count)
        {
            ExecuteCurrentSequence();
        }
        else if (loopSequences)
        {
            _currentSequenceIndex = 0;
            ExecuteCurrentSequence();
        }
        else
        {
            _currentState = NPCState.Idle;
            PlayIdleAnimation();
        }
    }
    #endregion

    #region Animation Methods
    public void PlayAnimation(AnimationClip animationClip)
    {
        if (animationClip == null)
        {
            Debug.LogWarning("NPC: No animation clip provided.", gameObject);
            return;
        }

        PlayAnimation(animationClip.name);
    }

    public void PlayAnimation(string animationName)
    {
        if (Animator == null)
        {
            Debug.LogWarning("NPC: No Animator component found.", gameObject);
            return;
        }

        Animator.CrossFade(animationName, 0.2f);
    }

    public void PlayIdleAnimation()
    {
        Animator.SetBool("isDrunk", isDrunk);

        Animator.SetBool("isWalk", false);
        Animator.SetTrigger("Start Idle");
    }

    public void PlayWalkAnimation()
    {
        Animator.SetBool("isDrunk", isDrunk);

        Animator.SetBool("isWalk", true);
        Animator.SetTrigger("Start Idle");
    }
    #endregion
}
