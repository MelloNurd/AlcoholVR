using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[SelectionBase]
public class NPC : MonoBehaviour
{
    [SerializeReference]
    public List<Sequence> Sequences = new List<Sequence>();

    private int _currentSequenceIndex = 0;

    [HideInInspector] public UnityEvent OnNPCInteract = new();

    public bool isDrunk = false;

    // Component References
    private NavMeshAgent _agent;
    private Animator _animator;
    private XRSimpleInteractable _interactable;
    private AudioSource _audioSource;

    public void Awake()
    {
        _agent = GetComponentInChildren<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _interactable = GetComponentInChildren<XRSimpleInteractable>();
        _audioSource = GetComponentInChildren<AudioSource>();

        _interactable.selectEntered.AddListener((args) => OnNPCInteract.Invoke());
    }

    private async void Start()
    {
        await UniTask.Yield();
        
        if (_currentSequenceIndex >= 0 && _currentSequenceIndex < Sequences.Count)
        {
            Sequences[_currentSequenceIndex].StartSequence(this);
        }
    }

    private void Update()
    {
        if (_currentSequenceIndex < 0 || _currentSequenceIndex >= Sequences.Count)
            return;

        Sequences[_currentSequenceIndex].UpdateSequence(this);
    }

    public void StartNextSequence()
    {
        if (_currentSequenceIndex < 0 || _currentSequenceIndex >= Sequences.Count)
        {
            Debug.LogWarning($"Current sequence {_currentSequenceIndex} is out of bounds.");
            return;
        }

        Sequences[_currentSequenceIndex].EndSequence(this);

        _currentSequenceIndex++;

        if (_currentSequenceIndex < Sequences.Count)
        {
            Sequences[_currentSequenceIndex].StartSequence(this);
        }
    }

    public void StartSequence(int index)
    {
        if (index < 0 || index >= Sequences.Count)
        {
            Debug.LogWarning($"Sequence index {index} is out of bounds.");
            return;
        }
        if (_currentSequenceIndex >= 0 && _currentSequenceIndex < Sequences.Count)
        {
            Sequences[_currentSequenceIndex].EndSequence(this);
        }
        _currentSequenceIndex = index;
        Sequences[_currentSequenceIndex].StartSequence(this);
    }

    public void PlayAnimation(AnimationClip clip)
    {
        PlayAnimationAsync(clip).Forget();
    }

    public void PlayIdleAnimation()
    {
        _animator.SetBool("isDrunk", isDrunk);

        _animator.SetTrigger("Start Idle");
        _animator.SetBool("isWalk", false);
    }

    public void PlayWalkAnimation()
    {
        _animator.SetBool("isDrunk", isDrunk);

        _animator.SetTrigger("Start Idle");
        _animator.SetBool("isWalk", true);
    }

    public async UniTask PlayAnimationAsync(AnimationClip clip)
    {
        _animator.CrossFade(clip.name, 0.15f);
        await UniTask.Delay(clip.length.ToMS());
    }
}