using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PartyScene : MonoBehaviour
{
    [Header("NPC References")]
    [SerializeField] private SequencedNPC _introNPC;
    [SerializeField] private SequencedNPC _drunkDrivingFriendNPC;
    [SerializeField] private InteractableNPC_SM _couchFriend;
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

    public bool IsOnSecondFloor => Player.Instance.transform.position.y > 3f;

    private bool _hasEnteredHouse = false;

    // sequence properties
    public BoolValue hasDoneIntro;
    public BoolValue hasTalkedToCouchFriend;
    public BoolValue hasFoundPhone;
    public BoolValue hasTalkedToDrunkFriend;
    public BoolValue hasTakenKeysFromFriend;

    private bool isDrinkinFriendReady = false;

    private void Start()
    {
        // set all bool values to false (they are persistent)
        hasDoneIntro.Value = false;
        hasTalkedToCouchFriend.Value = false;
        hasFoundPhone.Value = false;
        hasTalkedToDrunkFriend.Value = false;
        hasTakenKeysFromFriend.Value = false;

        SetCouchDialogue();
        SetDrunkFriendDestination();
        _introNPC.onFinishSequences.AddListener(() => hasDoneIntro.Value = true);
    }

    private void Update()
    {
        if(!isDrinkinFriendReady && (hasDoneIntro.Value && hasTalkedToCouchFriend.Value))
        {
            isDrinkinFriendReady = true;
            InitiateDrunkFriend();
        }
    }

    private async void InitiateDrunkFriend()
    {
        int delay = Mathf.RoundToInt(Random.Range(45_000f, 120_000f));
        Debug.Log($"Delaying drunk friend sequence for {delay} milliseconds.");
        await UniTask.Delay(delay); // Random delay between 45 seconds and 2 minutes
        _drunkDrivingFriendNPC.StartNextSequence();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!_hasEnteredHouse && other.gameObject.layer == LayerMask.NameToLayer("PlayerBody"))
        {
            _hasEnteredHouse = true;
            _introNPC.sequences[2].dialogue = _broughtAlcohol;
            _introNPC.StartNextSequence(); // Will start walking to player
        }
    }

    public void SetCouchDialogue()
    {
        if(GlobalStats.DrinkCount > 0)
        {
            _couchFriend.firstDialogue = _couchDrunk; // sober is set in inspector by default
        }

        _couchFriend.dialogueSystem.onEnd.AddListener(() =>
        {
            _couchFriend.IsInteractable = false;
            hasTalkedToCouchFriend.Value = true;
        });
    }

    public void SetDrunkFriendDestination()
    {
        _drunkDrivingFriendNPC.dialogueSystem.onEnd.AddListener(() =>
        {
            _drunkDrivingFriendNPC.sequences[_drunkDrivingFriendNPC.sequences.Count - 1].destination = hasTakenKeysFromFriend.Value
                ? _drunkFriendStayingDestination
                : _drunkFriendDrivingDestination;
        });
    }
}
