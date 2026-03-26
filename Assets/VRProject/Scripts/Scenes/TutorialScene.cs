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
    private const string WELCOME_TEXT = "Welcome to the simulator! Let's start with a tutorial on the controls.\n\nIn the tutorial, you have controllers instead of hands and the controls will highlight on the controllers.\n\nPress the trigger on the left or right controller to continue.";
    private const string SMOOTH_MOVEMENT_TUTORIAL_TEXT = "Let's get started!\n\nYou can use the joystick on the left controller to walk around. Try it now!";
    private const string TELEPORT_TUTORIAL_TEXT = "You can also teleport by pushing the joystick on the right controller forward. Try it out!";
    private const string MOVEMENT_PREFERENCE_TEXT = "Both of these movements will be available throughout the game, use whatever makes you the most comfortable!\n\nPress the trigger on the left or right controller to continue.";
    private const string ROTATION_TUTORIAL_TEXT = "You can rotate in place by pushing the joystick on the right controller left or right. Give it a try!";

    private const string PHONE_TUTORIAL_TEXT1 = "Press the menu or one of the face buttons on your left controller to pull out and put away your phone.";
    private const string PHONE_TUTORIAL_TEXT2 = "The phone is filled with lots of useful features like texts, objectives, settings, and even a working camera!\n\nYou can use your phone by pointing with your right controller and clicking its trigger.Try clicking the objectives button with the flag icon!";
    private const string GUIDE_TUTORIAL_TEXT = "You can enable guide lines next to each objective to assist in progression of the game. Try it now!";

    private const string TALK_TUTORIAL_TEXT = "People with an exclamation point above them can be talked to.\n\nTry reaching out and clicking on them using the trigger on one of your controllers.";
    private const string DIALOGUE_TUTORIAL_TEXT = "Try pressing one of the buttons in with your hands to make a dialogue selection!";
   
    private const string GRAB_TUTORIAL_TEXT = "Using the trigger on either controller, you can grab and hold most objects. Try grabbing one of the drinks!";
    private const string INTERACT_HELP_TEXT = "Try holding a drink in one hand and interact with your friend with the other!";

    [Header("Audio")]
    [SerializeField] private AudioClip _bgAudio;
    [SerializeField] private AudioClip _popUpAudio;
    [SerializeField] private AudioClip _tutorialCompleteAudio;

    [Header("Controllers")]
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;

    [Header("NPCs")]
    [SerializeField] private InteractableNPC_SM _friendNPC;

    [Header("Objects")]
    [SerializeField] private Transform _tablesPos;
    [SerializeField] private Transform _car;

    [Header("Dialogue")]
    [SerializeField] private Dialogue _waitingForDrink;
    [SerializeField] private Dialogue _foundSoda;
    [SerializeField] private Dialogue _foundAlcohol;

    [Header("Other")]
    [SerializeField] private CanvasGroup _objectivesScreen;

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
    private bool playerHasTeleported = false;
    private bool hasOpenedObjectives = false;

    // Make sure popups happens to complete tutorial steps only after the relevant tutorial text is shown
    private bool movePopUp = false;
    private bool teleportPopUp = false;
    private bool objectivePopUp = false;

    private ObjectiveSystem _talkToFriendObjective;
    private ObjectiveSystem bringDrinkToFriend;

    private async void Start()
    {
        AudioSource bgAudioSource = PlayerAudio.PlayLoopingSound(_bgAudio);
        _ = Tween.AudioVolume(bgAudioSource, 0f, 1f, 1f);

        SetupEvents();

        await InitializeMovementTutorial();
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

                    BoxCollider collider = _car.gameObject.GetComponent<BoxCollider>();
                    collider.enabled = true;
                }
            }
        });
    }

    private async UniTask InitializeMovementTutorial()
    {
        // Display welcome text and highlight triggers after a short delay
        await UniTask.Delay(1_000);
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(WELCOME_TEXT);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        // Wait until the player presses either of the VR triggers to continue
        await UniTask.WaitUntil(() => Keyboard.current.tKey.wasPressedThisFrame 
                                    || InputManager.Instance.leftController.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerValue) && leftTriggerValue
                                    || InputManager.Instance.rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerValue) && rightTriggerValue);
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        // After completing the welcome text, show the smooth movement tutorial after a short delay
        await UniTask.Delay(500);
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(SMOOTH_MOVEMENT_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_JOYSTICK);
        movePopUp = true;
        // Wait until the player moves using the left joystick to continue
        await UniTask.WaitUntil(() => playerHasMoved);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_JOYSTICK);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        // After completing the smooth movement tutorial, show the teleport tutorial after a short delay
        await UniTask.Delay(500);
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(TELEPORT_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_JOYSTICK);
        teleportPopUp = true;
        // Wait until the player moves using the right joystick to continue
        await UniTask.WaitUntil(() => playerHasTeleported || Keyboard.current.uKey.wasPressedThisFrame);
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_JOYSTICK);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        // After completing the teleport tutorial, show the movement preference text after a short delay
        await UniTask.Delay(500);
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(MOVEMENT_PREFERENCE_TEXT);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        // Wait until the player presses either of the VR triggers to continue
        await UniTask.WaitUntil(() => Keyboard.current.tKey.wasPressedThisFrame
                                    || InputManager.Instance.leftController.TryGetFeatureValue(CommonUsages.triggerButton, out bool leftTriggerVal) && leftTriggerVal
                                    || InputManager.Instance.rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool rightTriggerVal) && rightTriggerVal);
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        // After completing the movement preference text, show the rotation tutorial after a short delay
        await UniTask.Delay(500);
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(ROTATION_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_JOYSTICK);
        // Wait until the player rotates using the right joystick to continue
        await UniTask.WaitUntil(() => Keyboard.current.mKey.wasPressedThisFrame
                                    || InputManager.Instance.rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightJoystickInput) && Mathf.Abs(rightJoystickInput.x) > 0.5f);
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_JOYSTICK);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        TutorialText.Instance.HideText();
        PhoneTutorialSequence();
    }

    private async void PhoneTutorialSequence()
    {
        _talkToFriendObjective = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Talk to your friend at the table.", 0, _friendNPC.transform));
        _talkToFriendObjective.Begin();

        // Display tutorial to open phone
        await UniTask.Delay(2_000);
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(PHONE_TUTORIAL_TEXT1);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.MENU_BUTTON);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.X_BUTTON);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.Y_BUTTON);
        // Wait until the player opens the phone by pressing the menu or face buttons on the left controller
        await UniTask.WaitUntil(() => isPhoneEnabled || Keyboard.current.nKey.wasPressedThisFrame);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.MENU_BUTTON);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.X_BUTTON);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.Y_BUTTON);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        // After completing the phone opening tutorial, show the phone feature tutorial to open objectives menu
        await UniTask.Delay(500);
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(PHONE_TUTORIAL_TEXT2);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        objectivePopUp = true;
        // Wait until the player opens the objectives menu by pressing the right trigger while pointing at the phone's objectives button
        await UniTask.WaitUntil(() => hasOpenedObjectives || Keyboard.current.oKey.wasPressedThisFrame);
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        // After completing the phone feature tutorial, show the guide tutorial after a short delay
        await UniTask.Delay(500);
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(GUIDE_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        // Wait until the player enables the guide by pressing the right trigger while pointing at the phone's guide toggle
        await UniTask.WaitUntil(() => hasActivatedGuide || Keyboard.current.bKey.wasPressedThisFrame);
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        TutorialText.Instance.HideText();
        TalkToFriend();
    }

    private async void TalkToFriend()
    {
        // Wait until the player approaches the friend NPC to trigger the tutorial
        await UniTask.WaitUntil(() => Vector3.Distance(Player.Instance.Position, _friendNPC.transform.position) < 3f);

        // After approaching the friend, show the tutorial to talk to them
        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(TALK_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        // Wait until the player interacts with the friend NPC
        await UniTask.WaitUntil(() => friendInteractCount > 0);
        _talkToFriendObjective.Complete();
        TutorialText.Instance.HideText();
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        TutorialText.Instance.HideText();
    }

    private async void GrabDrinkTutorialSequence()
    {
        _friendNPC.dialogueSystem.onEnd.RemoveListener(GrabDrinkTutorialSequence);
        
        ObjectiveSystem _getDrinkObjective = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Find your friend a drink.", 0, _tablesPos));
        _getDrinkObjective.Begin();

        await UniTask.Delay(1_000);
        TutorialText.Instance.ShowText("Try looking at your objectives to see what to do next.");
        await UniTask.Delay(5_000);
        TutorialText.Instance.HideText();

        await UniTask.WaitUntil(() => Vector3.Distance(Player.Instance.Position, _tablesPos.position) < 5f);

        PlayerAudio.PlaySound(_popUpAudio);
        TutorialText.Instance.ShowText(GRAB_TUTORIAL_TEXT);
        TutorialButtons.Instance.HighlightButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        await UniTask.WaitUntil(() => hasGrabbedDrink || Keyboard.current.gKey.wasPressedThisFrame);
        hasGrabbedDrink = true;
        _getDrinkObjective.Complete();
        TutorialText.Instance.HideText();
        TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_TRIGGER);
        PlayerAudio.PlaySound(_tutorialCompleteAudio);

        // objective to bring drink to friend
        bringDrinkToFriend = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Bring the drink to your friend.", 0, _friendNPC.transform));
        bringDrinkToFriend.Begin();

        await UniTask.Delay(1_000);
        TutorialText.Instance.ShowText("Try looking at your objectives to see what to do next.");
        await UniTask.Delay(5_000);
        TutorialText.Instance.HideText();
    }

    private void Update()
    {
        if (!playerHasMoved && movePopUp)
        {
            InputManager.Instance.leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input);
            if (input != Vector2.zero
#if UNITY_EDITOR
                || Keyboard.current.wKey.wasPressedThisFrame
#endif
                )
            {
                Debug.Log("Triggering player has moved reading value: " + input);
                playerHasMoved = true;
            }
        }

        if (!playerHasTeleported && teleportPopUp)
        {
            InputManager.Instance.rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 input);
            if (input != Vector2.zero
                #if UNITY_EDITOR
                || Keyboard.current.tKey.wasPressedThisFrame
#endif
                )
            {
                Debug.Log("Triggering player has teleported reading value: " + input);
                playerHasTeleported = true;
            }
        }

        if (!hasOpenedObjectives && objectivePopUp)
        {
            if (_objectivesScreen.alpha > 0f && _objectivesScreen.interactable)
            {
                hasOpenedObjectives = true;
            }
        }

        if (buttonsSpawned && !hasPressedButton && buttonTimer < 2f)
        {
            buttonTimer += Time.deltaTime;

            if (buttonTimer >= 2f)
            {
                PlayerAudio.PlaySound(_popUpAudio);
                TutorialText.Instance.ShowText(DIALOGUE_TUTORIAL_TEXT);
            }
        }
    }
}