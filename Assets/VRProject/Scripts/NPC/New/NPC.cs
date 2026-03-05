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

    [HideInInspector] public int CurrentSequenceIndex = 0;

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

    public async UniTask MoveToAsync(Transform transform) => await MoveToAsync(transform.position);
    public async UniTask MoveToAsync(Vector3 destination)
    {
        if (_agent == null || !_agent.isActiveAndEnabled)
        {
            Debug.LogWarning("NavMeshAgent is not active or enabled. Cannot move.");
            return;
        }
        if (!_agent.SetDestinationToClosestPoint(destination, 1.5f))
        {
            Debug.LogWarning($"Failed to set destination for {_agent.gameObject.name}. Check if the destination is valid.");
            return;
        }

        PlayWalkAnimation();
        while (!_agent.IsAtDestination())
        {
            await UniTask.Yield(); // Wait for the next frame
        }
        PlayIdleAnimation();
    }
}