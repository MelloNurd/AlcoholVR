using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialScene : MonoBehaviour
{
    [Header("NPCs")]
    [SerializeField] private GameObject friendNPC;

    [Header("Animations")]
    [SerializeField] private AnimationClip wavingAnimation;
    [SerializeField] private AnimationClip idleAnimation;

    // Misc
    private Vector3 playerStartPos;

    public bool hasInteractedWithFriend { get; set; }

    private void Start()
    {
        InitializeMovementTutorial();
    }

    private async void InitializeMovementTutorial()
    {
        playerStartPos = Player.Instance.Position;

        await UniTask.Delay(15_000); // Time to check for player movement

        // If player has not moved after some seconds, show tutorial text
        if (Player.Instance.Position == playerStartPos)
        {
            Debug.Log("Player has not moved, showing tutorial text for movement.");
            TutorialText.Instance.ShowText("Use the joystick to move around.");
            TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.LEFT_JOYSTICK);

            await UniTask.WaitUntil(() => Player.Instance.Position != playerStartPos);

            TutorialText.Instance.HideText();
            TutorialButtons.Instance.ResetButton(LeftControllerMaterialIndex.LEFT_JOYSTICK);
        }
        else
        {
            Debug.Log("Player has moved, no need to show movement tutorial.");
        }
    }

    private void Update()
    {
        AnimateFriendNPC();
    }

    private void AnimateFriendNPC()
    {
        if (!hasInteractedWithFriend) // This is set automatically when the player interacts with the NPC
        {
            float distanceToPlayer = Vector3.Distance(friendNPC.transform.position, Player.Instance.Position);
            // If player is far from NPC, play waving animation
            // else, play idle animation
        }
    }
}