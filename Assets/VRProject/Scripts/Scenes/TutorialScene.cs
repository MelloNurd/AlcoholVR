using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialScene : MonoBehaviour
{
    private const string MOVEMENT_TUTORIAL_TEXT = "Use the joystick to move around.";
    private const string TALK_TUTORIAL_TEXT = "People with an exclamation point above them can be talked to.\n\nUse your right trigger on them to start!";
    private const string DIALOGUE_TUTORIAL_TEXT = "Try pressing one of the buttons in to make a dialogue selection!";

    [Header("NPCs")]
    [SerializeField] private GameObject friendNPC;

    [Header("Animations")]
    [SerializeField] private AnimationClip wavingAnimation;
    [SerializeField] private AnimationClip idleAnimation;

    [field: SerializeField]
    public bool hasTalkedToFriend { get; set; }

    // Misc
    private Vector3 playerStartPos;

    private bool buttonsSpawned = false;
    private float buttonTimer = 0f;
    private bool hasPressedButton = false;

    private void Awake()
    {
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
    }

    private async void Start()
    {
        await InitializeMovementTutorial();
        await CheckIfPlayerTalkedToFriend();
    }

    private async UniTask InitializeMovementTutorial()
    {
        playerStartPos = Player.Instance.Position;

        await UniTask.Delay(15_000); // Time to check for player movement

        // If player has not moved after some seconds, show tutorial text
        if (Player.Instance.Position == playerStartPos)
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

            await UniTask.WaitUntil(() => hasTalkedToFriend);

            TutorialText.Instance.HideText();
            TutorialButtons.Instance.ResetButton(RightControllerMaterialIndex.RIGHT_TRIGGER);
        }
    }

    private void Update()
    {
        if (buttonsSpawned && !hasPressedButton)
        {
            buttonTimer += Time.deltaTime;

            if (buttonTimer >= 8f)
            {
                TutorialText.Instance.ShowText(DIALOGUE_TUTORIAL_TEXT);
            }
        }
    }
}