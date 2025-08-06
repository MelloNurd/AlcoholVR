using PrimeTween;
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

    public bool tweenWarnings = false;

    // Party stats
    public static int hoopsScore = 0;
    public static bool wonDarts = false;
    public static int arcadeScore = 0;
    public static BroughtOptions broughtToParty = BroughtOptions.None;
    public static bool letDrunkFriendDrive = false;
    public static bool helpedRagingDrunk = false;
    public static bool called911 = false;

    // Bonfire stats
    public static bool playerGrabbedAlcohol = false; // This is for the first scenario, when at the cooler
    public static bool playerWentWithFlirt = false;

    private void Awake()
    {
        if(!tweenWarnings)
        {
            PrimeTweenConfig.warnZeroDuration = false;
            PrimeTweenConfig.warnTweenOnDisabledTarget = false;
        }
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
