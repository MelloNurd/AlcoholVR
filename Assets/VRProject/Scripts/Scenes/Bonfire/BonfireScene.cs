using System.Threading;
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
    [SerializeField] private AudioClip _tentRustleSound;

    [Header("Dialogue References")]
    [SerializeField] private Dialogue grabbedSoda;
    [SerializeField] private Dialogue grabbedAlcohol;
    [SerializeField] private Dialogue soberFlirtationFemale;
    [SerializeField] private Dialogue soberFlirtationMale;
    [SerializeField] private Dialogue drunkFlirtationFemale;
    [SerializeField] private Dialogue drunkFlirtationMale;
    [SerializeField] private Dialogue alcoholPoisoning;
    [SerializeField] private Dialogue poisoningResponse;

    [SerializeField] private Dialogue drankMysteryDrinkResponse;

    [Header("NPC References")]
    [SerializeField] private SequencedNPC friendNPC;
    [SerializeField] private SequencedNPC mysteryDrinkNPC;
    [SerializeField] private SequencedNPC tableNPC;
    [SerializeField] private SequencedNPC drunkFlirtNPCfemale;
    [SerializeField] private SequencedNPC drunkFlirtNPCmale;
    private SequencedNPC drunkFlirtNPC;
    [SerializeField] private SequencedNPC fireStickNPC;
    [SerializeField] private SequencedNPC fireFriendNPC;
    [SerializeField] private SequencedNPC poisonedNPC;
    [SerializeField] private SequencedNPC miscNPC;

    [Header("Game State References")]
    [SerializeField] private BoolValue _deterredFireNPCs;
    [SerializeField] private BoolValue _drankMysteryDrink;
    private bool _playerHasGrabbedDrink = false;
    private bool _playerHasTalkedToFlirt = false;
    private bool _playerHasTalkedToTable = false;
    private bool _playerHasTalkedToMysteryDrink = false;
    private bool _playerHasTalkedToFireNPCs = false;
    private bool _isPoisonedNpcReady = false;
    private bool _isMysteryDrinkReady = false;

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
    [SerializeField] private Transform _playerDrankNewPoint;
    [SerializeField] private Transform _cooler;
    [SerializeField] private Transform _tent;

    // Misc variables
    private bool _isFlirtWaitingForPlayer = false;
    private CancellationTokenSource _cancelToken;
    TriggerEventHandler _poisonedInteractionTrigger;

    private ObjectiveSystem _grabDrink;
    private ObjectiveSystem _talkToTable;
    private ObjectiveSystem _talkToFire;
    private ObjectiveSystem _followFlirt;
    private ObjectiveSystem _investigateGroup;

    void Start()
    {
        _cancelToken = new CancellationTokenSource();

        _poisonedInteractionTrigger = poisonedNPC.transform.parent.GetOrAddComponent<TriggerEventHandler>();

        // reset game states
        _deterredFireNPCs.Value = false;
        _drankMysteryDrink.Value = false;

        _friendsSoda.SetActive(false);
        _friendsAlcohol.SetActive(false);

        // Choose flirt NPC based on
        if(GlobalStats.Instance.IsFemale)
        {
            drunkFlirtNPC = drunkFlirtNPCmale;
            drunkFlirtNPCfemale.gameObject.SetActive(false);
        }
        else
        {
            drunkFlirtNPC = drunkFlirtNPCfemale;
            drunkFlirtNPCmale.gameObject.SetActive(false);
        }


        PlayerAudio.PlayLoopingSound(_natureSound);

        ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Explore the bonfire.", 0, Vector3.zero));
        _grabDrink = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Grab a drink from the cooler.", 1, _cooler));
        _talkToTable = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Talk to your peers at the picnic table.", 1, tableNPC.transform));
        _talkToFire = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("See what your peers are doing by the bonfire.", 1, fireStickNPC.transform));

        fireStickNPC.dialogueSystem.onStart.AddListener(() => _talkToFire.Complete());
        fireStickNPC.dialogueSystem.onEnd.AddListener(HandleFireNPCs);
        tableNPC.dialogueSystem.onStart.AddListener(() => _talkToTable.Complete());
        tableNPC.dialogueSystem.onEnd.AddListener(() =>
        {
            _playerHasTalkedToTable = true;
            RunSequenceChecks();
        });
    }

    private void Update()
    {
        CheckFlirtationProximity();

        if(Keyboard.current.f1Key.wasPressedThisFrame)
        {
            StartMysteryDrinkNPC();
        }
        else if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            StartAlcoholPoisoning();
        }
        else if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            drunkFlirtNPC.StartNextSequence();
        }

        if (_isMysteryDrinkReady)
        {
            _isMysteryDrinkReady = false; // Only do this once
            StartMysteryDrinkNPC();
        }
        if (_isPoisonedNpcReady && Vector3.Distance(poisonedNPC.bodyObj.transform.position, _bonfire.transform.position) < 4.1f)
        {
            _isPoisonedNpcReady = false; // Only do this once
            StartAlcoholPoisoning();
        }
    }

    private void RunSequenceChecks()
    {
        if (_playerHasTalkedToFlirt && _playerHasTalkedToFireNPCs && _playerHasTalkedToTable && _playerHasTalkedToMysteryDrink)
        {
            _isPoisonedNpcReady = true;
        }
        if (_playerHasTalkedToFlirt && _playerHasTalkedToFireNPCs && _playerHasTalkedToTable)
        {
            _isMysteryDrinkReady = true;
        }
    }

    private async void StartAlcoholPoisoning()
    {
        poisonedNPC.wrapAroundSequences = false;
        poisonedNPC.sequences.Clear();
        Sequence faint = new Sequence(Sequence.Type.Animate, _faintAnimation, false);
        poisonedNPC.sequences.Add(faint);
        poisonedNPC.StartSequence(faint);

        await UniTask.Delay(Mathf.RoundToInt(Random.Range(2000, 5000)));

        _investigateGroup = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Investigate the commotion by the bonfire.", 1, poisonedNPC.transform));

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
            _investigateGroup.Complete();

            if (other.gameObject.layer != LayerMask.NameToLayer("PlayerBody") && other.gameObject.layer != LayerMask.NameToLayer("PlayerHand"))
                return; // Only allow player to interact with the poisoned NPC

            _poisonedInteractionTrigger.EventsEnabled = false; // We only want this to run once

            alcoholPoisoning.onDialogueEnd.AddListener(() =>
            {
                friendNPC.lookAt.isLooking = false;
            });

            Sequence poisoningDialogue = new Sequence(Sequence.Type.Dialogue, alcoholPoisoning, nextSequenceOnEnd: false);
            friendNPC.sequences.Add(poisoningDialogue);
            friendNPC.StartSequence(poisoningDialogue);

            alcoholPoisoning.options[0].onOptionSelected.AddListener(async () =>
            {
                await UniTask.Delay(2_500);

                mysteryDrinkNPC.turnBodyToFacePlayer = false;
                mysteryDrinkNPC.turnHeadToFacePlayer = false;

                Sequence responseSequence = new Sequence(Sequence.Type.Dialogue, poisoningResponse, nextSequenceOnEnd: false);
                mysteryDrinkNPC.sequences.Add(responseSequence);
                mysteryDrinkNPC.StartSequence(responseSequence);

                await UniTask.Delay(3_000);

                Phone.Instance.CanToggle = false;
                Phone.Instance.ClearNotifications();
                Phone.Instance.EnablePhone();
                Phone.Instance.ShowEmergencyScreen();

                // Will be cancelled if player skips to sirens by calling 911, which means no wait
                await UniTask.Delay(37_000, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();

                EndScene();

                await UniTask.Delay(5000);

                Phone.Instance.DisablePhone();
            });
        });
    }

    public void SkipToSirens()
    {
        _cancelToken.Cancel();
        GlobalStats.called911 = true;
        Phone.Instance.DisablePhone();
    }

    public async void EndScene()
    {
        foreach (Transform child in _flashingLightsObj.transform)
        {
            if (child.TryGetComponent<FlashingLights>(out var flashingLights))
            {
                foreach (var light in flashingLights.lights)
                {
                    _ = Tween.LightIntensity(light, 0f, 10f, 15f, Ease.InOutExpo);
                }
                flashingLights.StartLights();
            }
        }

        PlayerAudio.PlaySound(_sirensSound, 0f, out AudioSource sirenSource);
        if (sirenSource != null)
        {
            _ = Tween.AudioVolume(sirenSource, 0.6f, 15f, Ease.InOutSine);
        }

        int quarterDelay = _sirensSound.length.ToMS() / 4;

        await UniTask.Delay(quarterDelay * 3);

        await Player.Instance.loading.CloseEyesAsync(0.25f);

        _ = Tween.AudioVolume(sirenSource, 0, 0.4f, Ease.InOutSine);

        await UniTask.Delay(550);

        ContinuousMusic.Instance?.StopMusic();

        Player.Instance.loading.LoadSceneByName("EndScene");
    }

    public void AssignDrunkFlirtOutcome()
    {
        _playerHasTalkedToFlirt = true;
        RunSequenceChecks();

        if (GlobalStats.DrinkCount >= 2)
        {
            Dialogue drunkFlirtDialogue = GlobalStats.Instance.IsMale ? drunkFlirtationFemale : drunkFlirtationMale;
            drunkFlirtNPC.sequences[drunkFlirtNPC.currentSequenceIndex + 1].dialogue = drunkFlirtDialogue;
            drunkFlirtNPC.dialogueSystem.onEnd.AddListener(() =>
            { // Player is drunk, so they went with the NPC
                _isFlirtWaitingForPlayer = true;
                GlobalStats.playerWentWithFlirt = true;
                _followFlirt = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Follow your peer.", 1, drunkFlirtNPC.transform));
            });
        }
        else
        {
            Dialogue soberFlirtDialogue = GlobalStats.Instance.IsMale ? soberFlirtationFemale : soberFlirtationMale;
            drunkFlirtNPC.sequences[drunkFlirtNPC.currentSequenceIndex + 1].dialogue = soberFlirtDialogue;
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
            if (Vector3.Distance(Player.Instance.CamPosition, _tent.position) < 2.75f)
            {
                _followFlirt.Complete();

                _isFlirtWaitingForPlayer = false;
                Player.Instance.CloseEyes(1f);
                Player.Instance.DisableMovement();

                await UniTask.Delay(1000);

                PlayerAudio.PlaySound(_tentRustleSound, 1f, out AudioSource tempAudio);

                drunkFlirtNPC.transform.localPosition = new Vector3(10f, -1.25f, 4.33f);
                drunkFlirtNPC.bodyObj.transform.localPosition = Vector3.zero;
                drunkFlirtNPC.transform.localEulerAngles = new Vector3(0, 203.75f, 0);
                drunkFlirtNPC.bodyObj.transform.localEulerAngles = Vector3.zero;

                Sequence sitSequence = new Sequence(Sequence.Type.Animate, _sittingAnimation, false);
                drunkFlirtNPC.StartSequence(sitSequence);

                await UniTask.Delay(tempAudio.clip.length.ToMS());

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
        _grabDrink.Complete();

        friendNPC.sequences[friendNPC.sequences.Count - 2].dialogue = grabbedSoda;
        await friendNPC.StartNextSequenceAsync();
        _friendsSoda.SetActive(true);

        int delay = Random.Range(30f, 50f).ToMS();
        Debug.Log($"Drunk flirt NPC will start in {delay} ms");
        await UniTask.Delay(delay);

        await UniTask.WaitUntil(() => !Player.Instance.IsInDialogue);

        drunkFlirtNPC.StartNextSequence();
    }

    [Button]
    public async void OnPlayerGrabbedAlcohol()
    {
        if (_playerHasGrabbedDrink || friendNPC.currentSequenceIndex < 3) return;
        _playerHasGrabbedDrink = true;
        GlobalStats.playerGrabbedAlcohol = true;
        _grabDrink.Complete();

        friendNPC.sequences[friendNPC.sequences.Count - 2].dialogue = grabbedAlcohol;
        await friendNPC.StartNextSequenceAsync();
        _friendsAlcohol.SetActive(true);

        int delay = Random.Range(30f, 50f).ToMS();
        await UniTask.Delay(delay);

        await UniTask.WaitUntil(() => !Player.Instance.IsInDialogue);

        drunkFlirtNPC.StartNextSequence();
    }

    public async void StartMysteryDrinkNPC()
    {
        mysteryDrinkNPC.dialogueSystem.onEnd.AddListener(() =>
        {
            if (_drankMysteryDrink.Value)
            {
                return; // Different ending, no need to handle the rest
            }

            _playerHasTalkedToMysteryDrink = true;
            if (_playerHasTalkedToFlirt && _playerHasTalkedToFireNPCs && _playerHasTalkedToTable && _playerHasTalkedToMysteryDrink)
            {
                _isPoisonedNpcReady = true;
            }
        });

        drankMysteryDrinkResponse.onDialogueStart.AddListener(async () => // dialogue that runs immediately after the player drinks the drink
        {
            _drankMysteryDrink.Value = true;

            PlayerFace.Instance.BlurVision(5f);

            await Player.Instance.loading.CloseEyesAsync(0.15f);

            GlobalStats.called911 = true; // workaround to make obituary not show for this ending
            GlobalStats.playerDrankMysteryDrink = true;

            ContinuousMusic.Instance?.StopMusic();

            // Set new scene after eyes close
            Player.Instance.loading.LoadSceneByName("BonfireCutscene");
        });

        int delay = Random.Range(20f, 45f).ToMS();
        Debug.Log($"Mystery drink NPC will start in {delay} ms");
        await UniTask.Delay(delay);

        await UniTask.WaitUntil(() => _playerHasGrabbedDrink && !Player.Instance.IsInDialogue);

        mysteryDrinkNPC.StartNextSequence();
    }

    public async void HandleFireNPCs()
    {
        _playerHasTalkedToFireNPCs = true;
        RunSequenceChecks();

        // check if player convinced them to stop
        if (!_deterredFireNPCs.Value) return; // didn't deter, they stay where they are

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