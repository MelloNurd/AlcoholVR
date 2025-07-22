using UnityEngine;

public class GlobalStats : MonoBehaviour
{
    public enum BroughtOptions
    {
        None,
        Snacks,
        Alcohol
    }

    public static GlobalStats Instance { get; private set; }

    public static int DrinkCount = 0;

    public static BroughtOptions BroughtToParty = BroughtOptions.None;
    public static bool LetDrunkFriendDrive = false;
    public static bool HelpedRagingDrunk = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
