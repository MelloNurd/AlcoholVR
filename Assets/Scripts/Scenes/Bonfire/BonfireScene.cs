using System.Linq;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.InputSystem;

public class BonfireScene : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip _natureSound;

    [Header("Dialogue References")]
    [SerializeField] private Dialogue grabbedSoda;
    [SerializeField] private Dialogue grabbedAlcohol;
    [SerializeField] private Dialogue soberFlirtation;
    [SerializeField] private Dialogue drunkFlirtation;

    [Header("NPC References")]
    [SerializeField] private SequencedNPC friendNPC;
    [SerializeField] private SequencedNPC mysteryDrinkNPC;
    [SerializeField] private SequencedNPC drunkFlirtNPC;
    [SerializeField] private SequencedNPC fireStickNPC;
    [SerializeField] private SequencedNPC fireFriendNPC;

    [Header("Misc References")]
    [SerializeField] private GameObject _friendsSoda;
    [SerializeField] private GameObject _friendsAlcohol;
    [SerializeField] private AnimationClip _sittingAnimation;
    [SerializeField] private BoolValue _deterredFireNPCs;

    // Misc variables
    private bool _playerHasGrabbedDrink = false;
    private bool _isFlirtWaitingForPlayer = false;

    void Start()
    {
        _friendsSoda.SetActive(false);
        _friendsAlcohol.SetActive(false);

        PlayerAudio.PlayLoopingSound(_natureSound);

        fireStickNPC.dialogueSystem.onEnd.AddListener(HandleFireNPCs);
    }

    private void Update()
    {
        CheckFlirtationProximity();

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            drunkFlirtNPC.StartNextSequence();
            if (GlobalStats.DrinkCount >= 0)
            {
                drunkFlirtNPC.dialogueSystem.onEnd.AddListener(() =>
                { // Player is drunk, so they went with the NPC
                    _isFlirtWaitingForPlayer = true;
                    GlobalStats.playerWentWithFlirt = true;
                });
            }
            else
            {
                drunkFlirtNPC.dialogueSystem.onEnd.AddListener(() =>
                { // Player is not drunk, so they did not go with the NPC
                    Sequence sitSequence = new Sequence(Sequence.Type.Animate, _sittingAnimation, false);
                    drunkFlirtNPC.StartSequence(sitSequence);
                });
            }
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
        //await UniTask.Delay(4500);
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
        //await UniTask.Delay(4500);
        _friendsAlcohol.SetActive(true);

        StartMysteryDrinkNPC();
    }

    public async void StartMysteryDrinkNPC()
    {
        await UniTask.Delay(Mathf.RoundToInt(Random.Range(20_000, 45_000)));

        await UniTask.WaitUntil(() => _playerHasGrabbedDrink && !Player.Instance.IsInteractingWithNPC);

        mysteryDrinkNPC.StartNextSequence();
    }

    public void HandleFireNPCs()
    {
        // check if player convinced them to stop
        if(!_deterredFireNPCs.Value) return; // didn't deter, they stay where they are

        // if so, create new sequence which is walking to point somewhere else
        // add it to existing sequences and start it (so it has the proper index)
        // additionally, create one more sequence which is sitting down and add that as well (should auto transition to this by index)
    }
}
