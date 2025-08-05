using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BonfireScene : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip _natureSound;
    [SerializeField] private AudioClip _sirensSound;

    [Header("Dialogue References")]
    [SerializeField] private Dialogue grabbedSoda;
    [SerializeField] private Dialogue grabbedAlcohol;
    [SerializeField] private Dialogue soberFlirtation;
    [SerializeField] private Dialogue drunkFlirtation;
    [SerializeField] private Dialogue alcoholPoisoning;

    [Header("NPC References")]
    [SerializeField] private SequencedNPC friendNPC;
    [SerializeField] private SequencedNPC mysteryDrinkNPC;
    [SerializeField] private SequencedNPC tableNPC;
    [SerializeField] private SequencedNPC drunkFlirtNPC;
    [SerializeField] private SequencedNPC fireStickNPC;
    [SerializeField] private SequencedNPC fireFriendNPC;
    [SerializeField] private SequencedNPC poisonedNPC;
    [SerializeField] private SequencedNPC miscNPC;

    [Header("Game State References")]
    [SerializeField] private BoolValue _deterredFireNPCs;
    private bool _playerHasGrabbedDrink = false;
    private bool _playerHasTalkedToFlirt = false;
    private bool _playerHasTalkedToTable = false;
    private bool _playerHasTalkedToMysteryDrink = false;
    private bool _playerHasTalkedToFireNPCs = false;
    private bool _isPoisonedNpcReady = false;

    [Header("Misc References")]
    [SerializeField] private GameObject _bonfire;
    [SerializeField] private GameObject _friendsSoda;
    [SerializeField] private GameObject _friendsAlcohol;
    [SerializeField] private AnimationClip _sittingAnimation;
    [SerializeField] private AnimationClip _faintAnimation;
    [SerializeField] private Transform _fireNPCWalkTarget1;
    [SerializeField] private Transform _fireNPCWalkTarget2;
    [SerializeField] private Transform _friendPoisoningReactionPoint;
    [SerializeField] private Transform _mysteryPoisoningReactionPoint;
    [SerializeField] private Transform _miscPoisoningReactionPoint;
    [SerializeField] private GameObject _flashingLightsObj;

    // Misc variables
    private bool _isFlirtWaitingForPlayer = false;

    TriggerEventHandler _poisonedInteractionTrigger;

    void Start()
    {
        _poisonedInteractionTrigger = poisonedNPC.transform.parent.GetOrAddComponent<TriggerEventHandler>();

        _deterredFireNPCs.Value = false;

        _friendsSoda.SetActive(false);
        _friendsAlcohol.SetActive(false);

        PlayerAudio.PlayLoopingSound(_natureSound);

        fireStickNPC.dialogueSystem.onEnd.AddListener(HandleFireNPCs);
        tableNPC.dialogueSystem.onEnd.AddListener(() =>
        {
            _playerHasTalkedToTable = true;
            if (_playerHasTalkedToFlirt && _playerHasTalkedToFireNPCs && _playerHasTalkedToTable && _playerHasTalkedToMysteryDrink)
            {
                _isPoisonedNpcReady = true;
            }
        });
    }

    private void Update()
    {
        CheckFlirtationProximity();

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            _isPoisonedNpcReady = true;
        }

        if (_isPoisonedNpcReady && Vector3.Distance(poisonedNPC.bodyObj.transform.position, _bonfire.transform.position) < 4.1f)
        {
            _isPoisonedNpcReady = false; // Only do this once
            StartAlcoholPoisoning();
        }
    }

    private async void StartAlcoholPoisoning()
    {
        poisonedNPC.wrapAroundSequences = false;
        poisonedNPC.sequences.Clear();
        Sequence faint = new Sequence(Sequence.Type.Animate, _faintAnimation, false);
        poisonedNPC.sequences.Add(faint);
        poisonedNPC.StartSequence(faint);

        await UniTask.Delay(Mathf.RoundToInt(Random.Range(1000, 3000)));

        Sequence mysteryWalkTo = new Sequence(Sequence.Type.Walk, _mysteryPoisoningReactionPoint);
        Sequence turnToFace2 = new Sequence(Sequence.Type.TurnToFace, directionToFace: poisonedNPC.bodyObj.transform.position - _mysteryPoisoningReactionPoint.transform.position, nextSequenceOnEnd: false);
        mysteryDrinkNPC.sequences.Add(mysteryWalkTo);
        mysteryDrinkNPC.sequences.Add(turnToFace2);
        mysteryDrinkNPC.StartSequence(mysteryWalkTo);

        Sequence miscWalkTo = new Sequence(Sequence.Type.Walk, _miscPoisoningReactionPoint);
        Sequence turnToFace3 = new Sequence(Sequence.Type.TurnToFace, directionToFace: poisonedNPC.bodyObj.transform.position - _miscPoisoningReactionPoint.transform.position, nextSequenceOnEnd: false);
        miscNPC.sequences.Add(miscWalkTo);
        miscNPC.sequences.Add(turnToFace3);
        miscNPC.StartSequence(miscWalkTo);

        Sequence friendWalkTo = new Sequence(Sequence.Type.Walk, _friendPoisoningReactionPoint);
        Sequence turnToFace1 = new Sequence(Sequence.Type.TurnToFace, directionToFace: poisonedNPC.bodyObj.transform.position - _friendPoisoningReactionPoint.transform.position, nextSequenceOnEnd: false);
        friendNPC.sequences.Add(friendWalkTo);
        friendNPC.sequences.Add(turnToFace1);
        await friendNPC.StartSequenceAsync(friendWalkTo); // Wait for the friend to walk to the poisoned NPC

        friendNPC.turnBodyToFacePlayer = false;

        _poisonedInteractionTrigger.OnTriggerEnterEvent.AddListener((Collider other) =>
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("PlayerBody") && other.gameObject.layer != LayerMask.NameToLayer("PlayerHand"))
                return; // Only allow player to interact with the poisoned NPC

            _poisonedInteractionTrigger.EventsEnabled = false; // We only want this to run once

            alcoholPoisoning.onDialogueEnd.AddListener(async () =>
            {
                friendNPC.lookAt.isLooking = false;

                await UniTask.Delay(3000);

                foreach (Transform child in _flashingLightsObj.transform)
                {
                    if (child.TryGetComponent<FlashingLights>(out var flashingLights))
                    {
                        foreach (var light in flashingLights.lights)
                        {
                            _ = Tween.LightIntensity(light, 0f, 75f, 20f, Ease.InOutExpo);
                        }
                        flashingLights.StartLights();
                    }
                }

                await UniTask.Delay(1000);

                AudioSource sirenSource = PlayerAudio.PlaySound(_sirensSound, 0f);
                if (sirenSource != null)
                {
                    _ = Tween.AudioVolume(sirenSource, 0.4f, 15f, Ease.InCirc);
                }

                int quarterDelay = _sirensSound.length.ToMS() / 4;

                await UniTask.Delay(quarterDelay * 3);

                await Player.Instance.loading.CloseEyesAsync(0.25f);
                
                _ = Tween.AudioVolume(sirenSource, 0, 0.35f, Ease.InOutSine);
                
                await UniTask.Delay(500);

                Player.Instance.loading.LoadSceneByName("EndScene");
            });

            Sequence poisoningDialogue = new Sequence(Sequence.Type.Dialogue, alcoholPoisoning, nextSequenceOnEnd: false);
            friendNPC.sequences.Add(poisoningDialogue);
            friendNPC.StartSequence(poisoningDialogue);
        });
    }

    public void AssignDrunkFlirtOutcome()
    {
        _playerHasTalkedToFlirt = true;
        if(_playerHasTalkedToFlirt && _playerHasTalkedToFireNPCs && _playerHasTalkedToTable && _playerHasTalkedToMysteryDrink)
        {
            _isPoisonedNpcReady = true;
        }

        if (GlobalStats.DrinkCount >= 0)
        {
            drunkFlirtNPC.sequences[drunkFlirtNPC.currentSequenceIndex + 1].dialogue = drunkFlirtation;
            drunkFlirtNPC.dialogueSystem.onEnd.AddListener(() =>
            { // Player is drunk, so they went with the NPC
                _isFlirtWaitingForPlayer = true;
                GlobalStats.playerWentWithFlirt = true;
            });
        }
        else
        {
            drunkFlirtNPC.sequences[drunkFlirtNPC.currentSequenceIndex + 1].dialogue = soberFlirtation;
            drunkFlirtNPC.dialogueSystem.onEnd.AddListener(() =>
            { // Player is not drunk, so they did not go with the NPC
                Sequence sitSequence = new Sequence(Sequence.Type.Animate, _sittingAnimation, false);
                drunkFlirtNPC.sequences.Add(sitSequence);
                drunkFlirtNPC.StartSequence(sitSequence);
            });
        }
    }

    private async void CheckFlirtationProximity()
    {
        if (_isFlirtWaitingForPlayer && drunkFlirtNPC.currentSequence.type == Sequence.Type.Wait)
        {
            if (Vector3.Distance(Player.Instance.Position, drunkFlirtNPC.bodyObj.transform.position) < 2.5f)
            {
                _isFlirtWaitingForPlayer = false;
                Player.Instance.CloseEyes(1.5f);
                Player.Instance.DisableMovement();

                await UniTask.Delay(2000);

                drunkFlirtNPC.transform.localPosition = new Vector3(10f, -1.25f, 4.33f);
                drunkFlirtNPC.bodyObj.transform.localPosition = Vector3.zero;
                drunkFlirtNPC.transform.localEulerAngles = new Vector3(0, 203.75f, 0);
                drunkFlirtNPC.bodyObj.transform.localEulerAngles = Vector3.zero;
                Sequence sitSequence = new Sequence(Sequence.Type.Animate, _sittingAnimation, false);
                drunkFlirtNPC.StartSequence(sitSequence);

                Player.Instance.OpenEyes(0.75f);
                Player.Instance.EnableMovement();
            }
        }
    }

    [Button]
    public async void OnPlayerGrabbedSoda()
    {
        if (_playerHasGrabbedDrink || friendNPC.currentSequenceIndex < 3) return;
        _playerHasGrabbedDrink = true;
        GlobalStats.playerGrabbedAlcohol = false;

        friendNPC.sequences[friendNPC.sequences.Count - 2].dialogue = grabbedSoda;
        await friendNPC.StartNextSequenceAsync();
        _friendsSoda.SetActive(true);

        StartMysteryDrinkNPC();
    }

    [Button]
    public async void OnPlayerGrabbedAlcohol()
    {
        if (_playerHasGrabbedDrink || friendNPC.currentSequenceIndex < 3) return;
        _playerHasGrabbedDrink = true;
        GlobalStats.playerGrabbedAlcohol = true;

        friendNPC.sequences[friendNPC.sequences.Count - 2].dialogue = grabbedAlcohol;
        await friendNPC.StartNextSequenceAsync();
        _friendsAlcohol.SetActive(true);

        StartMysteryDrinkNPC();
    }

    public async void StartMysteryDrinkNPC()
    {
        await UniTask.Delay(Mathf.RoundToInt(Random.Range(20_000, 45_000)));

        await UniTask.WaitUntil(() => _playerHasGrabbedDrink && !Player.Instance.IsInteractingWithNPC);

        mysteryDrinkNPC.StartNextSequence();
        mysteryDrinkNPC.dialogueSystem.onEnd.AddListener(async () =>
        {
            _playerHasTalkedToMysteryDrink = true;
            if (_playerHasTalkedToFlirt && _playerHasTalkedToFireNPCs && _playerHasTalkedToTable && _playerHasTalkedToMysteryDrink)
            {
                _isPoisonedNpcReady = true;
            }
            await UniTask.Delay(Mathf.RoundToInt(Random.Range(30_000, 120_000)));
            drunkFlirtNPC.StartNextSequence();
        });
    }

    public async void HandleFireNPCs()
    {
        Debug.Log("Handling fire NPCs...");
        // check if player convinced them to stop
        if (!_deterredFireNPCs.Value) return; // didn't deter, they stay where they are

        _playerHasTalkedToFireNPCs = true;
        if (_playerHasTalkedToFlirt && _playerHasTalkedToFireNPCs && _playerHasTalkedToTable && _playerHasTalkedToMysteryDrink)
        {
            _isPoisonedNpcReady = true;
        }

        // Disable the stick object
        fireStickNPC.GetComponentInChildren<Light>().transform.parent.gameObject.SetActive(false);

        Sequence walkAwaySequence1 = new Sequence(Sequence.Type.Walk, _fireNPCWalkTarget1, nextSequenceOnEnd: true);
        Sequence walkAwaySequence2 = new Sequence(Sequence.Type.Walk, _fireNPCWalkTarget2, nextSequenceOnEnd: true);
        Sequence turnSequence = new Sequence(Sequence.Type.TurnToFace, directionToFace: new Vector3(1, 0, -1));
        Sequence sitSequence = new Sequence(Sequence.Type.Animate, _sittingAnimation, false);

        fireStickNPC.sequences.Add(walkAwaySequence1);
        fireStickNPC.sequences.Add(turnSequence);
        //fireStickNPC.sequences.Add(sitSequence);

        fireFriendNPC.sequences.Add(walkAwaySequence2);
        fireFriendNPC.sequences.Add(turnSequence);
        fireFriendNPC.sequences.Add(sitSequence);

        fireStickNPC.StartSequence(walkAwaySequence1);
        await UniTask.Delay(270); // slight delay before second NPC starts walking away
        fireFriendNPC.StartSequence(walkAwaySequence2);
    }
}