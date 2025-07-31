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

    [Header("Misc References")]
    [SerializeField] private GameObject _friendsSoda;
    [SerializeField] private GameObject _friendsAlcohol;
    [SerializeField] private AnimationClip _sittingAnimation;

    // Misc variables
    private bool _playerHasGrabbedDrink = false;

    void Start()
    {
        _friendsSoda.SetActive(false);
        _friendsAlcohol.SetActive(false);

        PlayerAudio.PlayLoopingSound(_natureSound);
    }

    private void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            drunkFlirtNPC.StartNextSequence();
            if (GlobalStats.DrinkCount >= 2)
            { 
                drunkFlirtNPC.dialogueSystem.onEnd.AddListener(() =>
                { // Player is drunk, so they went with the NPC
                    Debug.Log("flirt dialogue ended- player IS drunk");
                });
            }
            else
            {
                drunkFlirtNPC.dialogueSystem.onEnd.AddListener(() =>
                { // Player is not drunk, so they did not go with the NPC

                    // somehow make npc stop rest of sequences and just sit down

                    //drunkFlirtNPC.sequences[0].animation = _sittingAnimation;
                    //drunkFlirtNPC.StartSequence(0).Forget();

                    Debug.Log("flirt dialogue ended- player IS NOT drunk");
                });
            }
        }
    }

    [Button]
    public async void OnPlayerGrabbedSoda()
    {
        if (_playerHasGrabbedDrink) return;
        _playerHasGrabbedDrink = true;
        GlobalStats.playerGrabbedAlcohol = false;

        friendNPC.sequences[friendNPC.sequences.Count - 2].dialogue = grabbedSoda;
        friendNPC.StartNextSequence();
        await UniTask.Delay(4500);
        _friendsSoda.SetActive(true);

        StartMysteryDrinkNPC();
    }

    [Button]
    public async void OnPlayerGrabbedAlcohol()
    {
        if (_playerHasGrabbedDrink) return;
        _playerHasGrabbedDrink = true;
        GlobalStats.playerGrabbedAlcohol = true;

        friendNPC.sequences[friendNPC.sequences.Count - 2].dialogue = grabbedAlcohol;
        friendNPC.StartNextSequence();
        await UniTask.Delay(4500);
        _friendsAlcohol.SetActive(true);

        StartMysteryDrinkNPC();
    }

    public async void StartMysteryDrinkNPC()
    {
        await UniTask.Delay(Mathf.RoundToInt(Random.Range(20_000, 45_000)));

        await UniTask.WaitUntil(() => _playerHasGrabbedDrink && !Player.Instance.IsInteractingWithNPC);

        mysteryDrinkNPC.StartNextSequence();
    }
}
