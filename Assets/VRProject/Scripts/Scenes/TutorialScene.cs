using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using CommonUsages = UnityEngine.XR.CommonUsages;

public class TutorialScene : MonoBehaviour
{
    private const string MOVEMENT_TUTORIAL_TEXT = "Use the left joystick to move around.";
    private const string TALK_TUTORIAL_TEXT = "People with an exclamation point above them can be talked to.\n\nTry clicking on them using the trigger on one of your controllers.";
    private const string DIALOGUE_TUTORIAL_TEXT = "Try pressing one of the buttons in with your hands to make a dialogue selection!";
    private const string PHONE_TUTORIAL_TEXT1 = "Press the menu button on your left controller to pull out and put away your phone.";
    private const string PHONE_TUTORIAL_TEXT2 = "You can use your phone by clicking the trigger on your right hand. There are buttons on the bottom row of the home screen that you can click in to see different menus.";
    private const string GUIDE_TUTORIAL_TEXT = "Try opening the objectives menu by pressing on the button with the flag. There, you can enable guidelines to assist in progression of the game.";
    private const string GRAB_TUTORIAL_TEXT = "Using the trigger on either controller, you can grab objects. Try grabbing one of the drinks.";
    private const string INTERACT_HELP_TEXT = "Try holding a drink when you talk to them.";

    [Header("Audio")]
    [SerializeField] private AudioClip _bgAudio;

    [Header("Controllers")]
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;

    [Header("NPCs")]
    [SerializeField] private InteractableNPC_SM _friendNPC;

    [Header("Objects")]
    [SerializeField] private Transform _tablesPos;
    [SerializeField] private XRSimpleInteractable _car;

    [Header("Dialogue")]
    [SerializeField] private Dialogue _waitingForDrink;
    [SerializeField] private Dialogue _foundSoda;
    [SerializeField] private Dialogue _foundAlcohol;

    private int friendInteractCount = 0;

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

    private bool playerHasMoved = false;

    private ObjectiveSystem bringDrinkToFriend;

    private async void Start()
    {
        AudioSource bgAudioSource = PlayerAudio.PlayLoopingSound(_bgAudio);
        _ = Tween.AudioVolume(bgAudioSource, 0f, 1f, 1f);

        SetupEvents();

        await InitializeMovementTutorial();
        await CheckIfPlayerTalkedToFriend();
    }

    private void SetupEvents()
    {
        foreach(var drinks in FindObjectsByType<OpenableBottle>(FindObjectsSortMode.None))
        {
            var rb = drinks.GetComponent<Rigidbody>();
            if (!rb) continue;

            rb.isKinematic = true;
            var grabInteractable = drinks.GetComponent<XRGrabInteractable>();
            grabInteractable.enabled = false;

            _friendNPC.dialogueSystem.onEnd.AddListener(() =>
            {
                grabInteractable.enabled = true;
                rb.isKinematic = false;
            });
        };

        Phone.OnPhoneToggled.AddListener((isEnabled) =>
        {
            leftController.SetActive(!isEnabled);
            isPhoneEnabled = isEnabled;
        });
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
            friendInteractCount++;
            if(friendInteractCount == 1)
            {
                return; // First interaction, just start dialogue, no tutorial progression
            }

            if (!_heldDrink || !_heldDrink.isSelected)
            {
                _friendNPC.firstDialogue = _waitingForDrink;
                if (friendInteractCount > 3)
                {
                    // After multiple interactions of trying to talk without a drink, show tutorial text to hint at the solution
                    TutorialText.Instance.ShowText(INTERACT_HELP_TEXT);

                    await UniTask.Delay(8_000);

                    if (TutorialText.Instance.CurrentText == INTERACT_HELP_TEXT) // Safeguard to only hide if it's still showing the interact help text
                        TutorialText.Instance.HideText();
                }
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
                    bringDrinkToFriend.Complete();
                    Destroy(_heldDrink.gameObject);

                    await UniTask.WaitUntil(() => _friendNPC.dialogueSystem.IsDialogueActive == false);

                    await UniTask.Delay(Random.Range(5f, 10f).ToMS());

                    Phone.Instance.QueueNotification("Mom", "Hey, it's time to go home. I'll be waiting in the car.");

                    TutorialText.Instance.ShowText("You got a text message. You can view them on your phone.");
                    TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.MENU_BUTTON);

                    ObjectiveSystem _getDrinkObjective = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Head to the car.", 0, _car.transform));
                    _getDrinkObjective.Begin();

                    _car.activated.AddListener((ActivateEventArgs test) =>
                    {
                        _getDrinkObjective.Complete();
                        Player.Instance.loading.TransitionSceneById(SceneManager.GetActiveScene().buildIndex + 1);
                    });
                }
            }
        });
    }

    private async UniTask InitializeMovementTutorial()
    {
        await UniTask.Delay(5_000);

        TutorialText.Instance.ShowText(MOVEMENT_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_JOYSTICK);

        await UniTask.WaitUntil(() => playerHasMoved);

        TutorialText.Instance.HideText();
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_JOYSTICK);
    }

    private async UniTask CheckIfPlayerTalkedToFriend()
    {
        await UniTask.Delay(30_000);

        if (friendInteractCount == 0)
        {
            TutorialText.Instance.ShowText(TALK_TUTORIAL_TEXT);
            TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
            TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_TRIGGER);

            await UniTask.WaitUntil(() => friendInteractCount > 0);

            TutorialText.Instance.HideText();
            TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
            TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        }
    }

    private async void GrabDrinkTutorialSequence()
    {
        Debug.Log("GRAB YOUR FRIEND A DRINK TUTORIAL START");
        _friendNPC.dialogueSystem.onEnd.RemoveListener(GrabDrinkTutorialSequence);
        
        ObjectiveSystem _getDrinkObjective = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Find your friend a drink.", 0, _tablesPos));
        _getDrinkObjective.Begin();

        await UniTask.Delay(2_000);

        TutorialText.Instance.ShowText(PHONE_TUTORIAL_TEXT1);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.MENU_BUTTON);

        await UniTask.WaitUntil(() => isPhoneEnabled || Keyboard.current.nKey.wasPressedThisFrame);

        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.MENU_BUTTON);

        TutorialText.Instance.ShowText(PHONE_TUTORIAL_TEXT2);

        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);

        await UniTask.Delay(12_000);

        TutorialText.Instance.ShowText(GUIDE_TUTORIAL_TEXT);

        await UniTask.WaitUntil(() => hasActivatedGuide || Keyboard.current.bKey.wasPressedThisFrame);

        TutorialText.Instance.HideText();
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);

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
        bringDrinkToFriend = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Bring the drink to your friend.", 0, _friendNPC.transform));
        bringDrinkToFriend.Begin();

        await UniTask.Delay(1_000);
        TutorialText.Instance.ShowText("Try looking at your objectives to see what to do next.");
        await UniTask.Delay(6_000);
        TutorialText.Instance.HideText();
    }

    private void Update()
    {
        if (!playerHasMoved)
        {
            InputManager.Instance.leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input);
            if (input != Vector2.zero)
            {
                Debug.Log("Triggering player has moved reading value: " + input);
                playerHasMoved = true;
            }
        }

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