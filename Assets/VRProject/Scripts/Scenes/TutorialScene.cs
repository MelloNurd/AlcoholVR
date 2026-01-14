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

    public bool hasInteractedWithFriend { get; set; }

    private async void Start()
    {
        await UniTask.Delay(2500);

        TutorialButtons.Instance.HighlightButton(LeftControllerMaterialIndex.MENU_BUTTON);
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