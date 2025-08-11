using System.Linq;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PartyScene : MonoBehaviour
{
    [Header("NPC References")]
    [SerializeField] private SequencedNPC _introNPC;
    [SerializeField] private SequencedNPC _drunkDrivingFriendNPC;
    [SerializeField] private InteractableNPC_SM _couchFriend;
    [SerializeField] private InteractableNPC_SM _missingPhoneDrunkNPC;
    [SerializeField] private InteractableNPC_SM _rageNPC;
    [SerializeField] private SequencedNPC _bonfireFriendNPC;

    [Header("Dialogue References")]
    [SerializeField] private Dialogue _broughtAlcohol;
    [SerializeField] private Dialogue _broughtSnacks;
    [SerializeField] private Dialogue _broughtNothing;

    [SerializeField] private Dialogue _couchDrunk;
    [SerializeField] private Dialogue _couchSober;

    [Header("Misc References")]
    [SerializeField] private Transform _drunkFriendDrivingDestination;
    [SerializeField] private Transform _drunkFriendStayingDestination;
    [SerializeField] private AnimationClip _rageStartAnimation;
    [SerializeField] private AnimationClip _rageLoopAnimation;
    [SerializeField] private AnimationClip _rageIdleAnimation;
    [SerializeField] private AnimationClip _rageFinishAnimation;
    [SerializeField] private GameObject _rageBottle;
    [SerializeField] private XRGrabInteractable _missingPhoneObj;
    [SerializeField] private Animator _carAnimator;

    public bool IsOnSecondFloor => IsInHouse && Player.Instance.Position.y > 3.5f;
    public bool InViewOfRage => IsInHouse && IsOnSecondFloor && Player.Instance.Position.z > -7f; // On the specific side of the house
    public bool IsInHouse { get; private set; } = false;

    private int _enterCount = 0;

    // sequence properties
    public BoolValue hasDoneIntro;
    public BoolValue hasTalkedToCouchFriend;
    public BoolValue hasFoundPhone;
    public BoolValue hasTalkedToDrunkFriend;
    public BoolValue hasTakenKeysFromFriend;

    private bool _isDrinkinFriendReady = false;

    private bool _isRageBegun = false;

    private void Start()
    {
        // set all bool values to false (they are persistent)
        hasDoneIntro.Value = false;
        hasTalkedToCouchFriend.Value = false;
        hasFoundPhone.Value = false;
        hasTalkedToDrunkFriend.Value = false;
        hasTakenKeysFromFriend.Value = false;

        SetDrunkFriendDestination();
        _couchFriend.onFirstInteraction.AddListener(SetCouchDialogue);
        _introNPC.dialogueSystem.onEnd.AddListener(() =>
        {
            hasDoneIntro.Value = true;
            Debug.Log("Intro sequence completed.");
        });
    }

    private void Update()
    {
        if (!_isRageBegun && hasTalkedToDrunkFriend.Value && InViewOfRage)
        {
            Debug.Log("Rage sequence triggered.");
            _isRageBegun = true;
            BeginRageSequence();
        }
        if (!_isDrinkinFriendReady && (hasDoneIntro.Value && hasTalkedToCouchFriend.Value))
        {
            _isDrinkinFriendReady = true;
            InitiateDrunkFriend();
        }
    }

    private async void InitiateDrunkFriend()
    {
        int delay = Mathf.RoundToInt(Random.Range(15_000f, 45_000f) * 0.5f); // Random delay between 45 seconds and 2 minutes (halved to split up)
        //int delay = Mathf.RoundToInt(Random.Range(45_000f, 120_000f) * 0.5f); // Random delay between 45 seconds and 2 minutes (halved to split up)
        Debug.Log("Drunk driving friend delayed by " + delay * 2 + " ms");
        await UniTask.Delay(delay);
        if(GlobalStats.broughtItems == GlobalStats.BroughtOptions.Alcohol)
        {
            Phone.Instance.QueueNotification("Mom", "We saw you take alcohol on the cameras. You are SO grounded!");
        }
        await UniTask.Delay(delay);
        _drunkDrivingFriendNPC.StartNextSequence();
    }

    public void SetCouchDialogue()
    {
        if (GlobalStats.DrinkCount > 0)
        {
            _couchFriend.firstDialogue = _couchDrunk; // sober is set in inspector by default
        }

        _couchFriend.dialogueSystem.onEnd.AddListener(() =>
        {
            _couchFriend.IsInteractable = false;
            hasTalkedToCouchFriend.Value = true;
            Debug.Log("Couch friend dialogue ended. Has talked: " + hasTalkedToCouchFriend.Value);
        });
    }

    public void SetDrunkFriendDestination()
    {
        _drunkDrivingFriendNPC.dialogueSystem.onEnd.AddListener(() =>
        {
            _drunkDrivingFriendNPC.sequences[_drunkDrivingFriendNPC.sequences.Count - 1].destination = hasTakenKeysFromFriend.Value
                ? _drunkFriendStayingDestination
                : _drunkFriendDrivingDestination;

            GlobalStats.letDrunkFriendDrive = !hasTakenKeysFromFriend.Value;
            hasTalkedToDrunkFriend.Value = true;

            _drunkDrivingFriendNPC.sequences[_drunkDrivingFriendNPC.sequences.Count - 1].onSequenceEnd.AddListener(() =>
            {
                Debug.Log($"Drunk driving friend end: has taken keys {hasTakenKeysFromFriend.Value}");
                if (hasTakenKeysFromFriend.Value) return;

                _carAnimator.SetTrigger("car");
                _drunkDrivingFriendNPC.gameObject.SetActive(false);
            });
        });
    }

    public async void BeginRageSequence()
    {
        _rageBottle.SetActive(false);

        _rageNPC.idleAnimation = _rageStartAnimation;
        _rageNPC.PlayIdleAnimation();

        await UniTask.Delay(Mathf.RoundToInt(_rageStartAnimation.length * 1000));

        _rageNPC.idleAnimation = _rageLoopAnimation;
        _rageNPC.PlayIdleAnimation();

        _bonfireFriendNPC.StartNextSequence(); // Start the bonfire friend sequence
        foreach(var sequence in _bonfireFriendNPC.sequences)
        {
            if(sequence.dialogue != null)
            {
                for(int i = 0; i < sequence.dialogue.options.Count; i++)
                {
                    Debug.Log($"Option {i}: {sequence.dialogue.options[i].optionText}");
                }

                // This system is bad, but I don't have time to improve. Magic numbers for now.
                sequence.dialogue.options[0].onOptionSelected.AddListener(() => // GOOD DECISION (try to calm down the rage)
                {
                    GlobalStats.helpedRagingDrunk = true;
                    _rageNPC.IsInteractable = true;
                    _rageNPC.objective.Begin();

                    _rageNPC.onIncompleteInteraction.AddListener(() =>
                    {
                        _rageNPC.idleAnimation = _rageIdleAnimation;
                        Debug.Log("Rage NPC idle animation set to idle.");
                        _rageNPC.PlayIdleAnimation();
                    });

                    _rageNPC.dialogueSystem.onEnd.AddListener(() =>
                    {
                        _rageNPC.idleAnimation = _rageFinishAnimation;
                        _rageNPC.PlayIdleAnimation();

                        _rageNPC.IsInteractable = false;
                        _rageNPC.objective.Complete();
                        _bonfireFriendNPC.StartNextSequence();
                    });
                });

                sequence.dialogue.options[1].onOptionSelected.AddListener(() => // BAD DECISION (ignore drunk rage)
                {
                    GlobalStats.helpedRagingDrunk = false;
                    _bonfireFriendNPC.StartNextSequence(2);
                });

                break;
            }
        }
    }

    public void CheckFoundPhone()
    {
        if(_missingPhoneObj.isSelected && !hasFoundPhone.Value)
        {
            hasFoundPhone.Value = true;
            _missingPhoneObj.gameObject.SetActive(false);
            _missingPhoneDrunkNPC.objective.Complete();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsInHouse && other.gameObject.layer == LayerMask.NameToLayer("PlayerBody"))
        {
            IsInHouse = true;
            if (_enterCount == 0)
            {
                Dialogue introDialogue = GlobalStats.broughtItems switch
                {
                    GlobalStats.BroughtOptions.Alcohol => _broughtAlcohol,
                    GlobalStats.BroughtOptions.Snacks => _broughtSnacks,
                    _ => _broughtNothing
                };

                _introNPC.sequences[2].dialogue = introDialogue;
                _introNPC.StartNextSequence(); // Will start walking to player
            }

            _enterCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsInHouse && other.gameObject.layer == LayerMask.NameToLayer("PlayerBody"))
        {
            IsInHouse = false;
        }
    }
}
