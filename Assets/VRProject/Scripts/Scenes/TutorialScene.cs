using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TutorialScene : MonoBehaviour
{
    private const string MOVEMENT_TUTORIAL_TEXT = "Use the left joystick to move around.";
    private const string TALK_TUTORIAL_TEXT = "People with an exclamation point above them can be talked to.\n\nTry using your trigger on your controller on them.";
    private const string DIALOGUE_TUTORIAL_TEXT = "Try pressing one of the buttons in with your hands to make a dialogue selection!";
    private const string PHONE_TUTORIAL_TEXT = "Press the menu button on your left controller to pull out your phone.";
    private const string GUIDE_TUTORIAL_TEXT = "Using your phone, you can access guide markers for your current objectives.\n\nUsing the buttons in the bottom row, navigate to the objectives and activate the guide.";
    private const string GRAB_TUTORIAL_TEXT = "Using the trigger on either controller, you can grab objects. Try grabbing one of the drinks.";

    [Header("Controllers")]
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;

    [Header("NPCs")]
    [SerializeField] private InteractableNPC_SM _friendNPC;

    [Header("Objects")]
    [SerializeField] private Transform _tablesPos;
    [SerializeField] private Transform _carPos;

    [Header("Dialogue")]
    [SerializeField] private Dialogue _waitingForDrink;
    [SerializeField] private Dialogue _foundSoda;
    [SerializeField] private Dialogue _foundAlcohol;

    public bool hasTalkedToFriend { get; set; }

    // Misc
    private Vector3 playerStartPos;

    private bool buttonsSpawned = false;
    private float buttonTimer = 0f;
    private bool hasPressedButton = false;

    private bool isPhoneEnabled = false;

    private bool hasActivatedGuide = false;

    private bool hasGrabbedDrink = false;
    private bool grabbedAlcohol = false;
    private XRGrabInteractable _heldDrink;

    private async void Start()
    {
        SetupEvents();

        await InitializeMovementTutorial();
        await CheckIfPlayerTalkedToFriend();
    }

    private void SetupEvents()
    {
        Phone.OnPhoneToggled.AddListener((isEnabled) => isPhoneEnabled = isEnabled);
        ObjectiveUI.OnGuideToggle.AddListener((_) => hasActivatedGuide = true);
        OpenableBottle.OnBottleGrabbed.AddListener((OpenableBottle drink) =>
        {
            hasGrabbedDrink = true;
            _heldDrink = drink.GetComponent<XRGrabInteractable>();
            grabbedAlcohol = drink.IsAlcoholic;
        });

        DialogueButtons.OnButtonsSpawn.AddListener(() =>
        {
            buttonsSpawned = true;
        });

        DialogueButtons.OnButtonPressed.AddListener((_) =>
        {
            hasPressedButton = true;
            if (TutorialText.Instance.CurrentText == DIALOGUE_TUTORIAL_TEXT) // Safeguard to only hide if it's showing the dialogue buttons tutorial text
            {
                TutorialText.Instance.HideText();
            }
        });

        _friendNPC.dialogueSystem.onEnd.AddListener(GrabDrinkTutorialSequence);

        _friendNPC.onFirstInteraction.AddListener(async () =>
        {
            if (!hasTalkedToFriend)
            {
                hasTalkedToFriend = true;
                return;
            }

            if (!hasGrabbedDrink)
            {
                _friendNPC.firstDialogue = _waitingForDrink;
                return;
            }
            else
            {
                if (grabbedAlcohol)
                {
                    _friendNPC.firstDialogue = _foundAlcohol;
                }
                else
                {
                    _friendNPC.firstDialogue = _foundSoda;
                }

                await UniTask.Delay(30_000);

                Phone.Instance.QueueNotification("Mom", "Hey, it's time to go home. I'll be waiting in the car.");

                ObjectiveSystem _getDrinkObjective = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Head to the car.", 0, _carPos));
                _getDrinkObjective.Begin();
            }
        });
    }

    private async UniTask InitializeMovementTutorial()
    {
        playerStartPos = Player.Instance.Position;

        await UniTask.Delay(8_000); // Time to check for player movement

        // If player has not moved after some seconds, show tutorial text
        if (Vector3.Distance(Player.Instance.Position, playerStartPos) < 5)
        {
            TutorialText.Instance.ShowText(MOVEMENT_TUTORIAL_TEXT);
            TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_JOYSTICK);

            await UniTask.WaitUntil(() => Player.Instance.Position != playerStartPos);

            TutorialText.Instance.HideText();
            TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_JOYSTICK);
        }
    }

    private async UniTask CheckIfPlayerTalkedToFriend()
    {
        await UniTask.Delay(30_000);

        if (!hasTalkedToFriend)
        {
            TutorialText.Instance.ShowText(TALK_TUTORIAL_TEXT);
            TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
            TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_TRIGGER);

            await UniTask.WaitUntil(() => hasTalkedToFriend);

            TutorialText.Instance.HideText();
            TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
            TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        }
    }

    private async void GrabDrinkTutorialSequence()
    {
        ObjectiveSystem _getDrinkObjective = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Find your friend a drink.", 0, _tablesPos));
        _getDrinkObjective.Begin();

        await UniTask.Delay(5_000);

        TutorialText.Instance.ShowText(PHONE_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.MENU_BUTTON);

        await UniTask.WaitUntil(() => isPhoneEnabled || Keyboard.current.nKey.wasPressedThisFrame);

        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.MENU_BUTTON);

        TutorialText.Instance.ShowText(GUIDE_TUTORIAL_TEXT);

        await UniTask.WaitUntil(() => hasActivatedGuide || Keyboard.current.bKey.wasPressedThisFrame);

        TutorialText.Instance.HideText();

        await UniTask.WaitUntil(() => Vector3.Distance(Player.Instance.Position, _tablesPos.position) < 5f);

        TutorialText.Instance.ShowText(GRAB_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_TRIGGER);

        await UniTask.WaitUntil(() => hasGrabbedDrink || Keyboard.current.gKey.wasPressedThisFrame);
        hasGrabbedDrink = true;

        _getDrinkObjective.Complete();
        TutorialText.Instance.HideText();
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_TRIGGER);

        // objective to bring drink to friend
        ObjectiveSystem _bringDrinkObjective = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Bring the drink to your friend.", 0, _friendNPC.transform));
        _bringDrinkObjective.Begin();

        _friendNPC.dialogueSystem.onEnd.RemoveListener(GrabDrinkTutorialSequence);
    }

    private void Update()
    {
        if (buttonsSpawned && !hasPressedButton && buttonTimer < 8f)
        {
            buttonTimer += Time.deltaTime;

            if (buttonTimer >= 5f)
            {
                TutorialText.Instance.ShowText(DIALOGUE_TUTORIAL_TEXT);
            }
        }
    }
}