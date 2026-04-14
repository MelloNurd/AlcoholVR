using PrimeTween;
using UnityEngine;

public enum Sex
{
    Male,
    Female
}

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
    [SerializeField] public static int maxDrinks = 5;

    public bool tweenWarnings = false;

    [SerializeField] bool startDrunk = false;

    [SerializeField] private Sex playerSex = Sex.Male;

    public void SetSex(float value)
    {
        playerSex = value > 50 ? Sex.Male : Sex.Female; ;
        Debug.Log($"Read value: {value}, Resulting gender: {playerSex}");
    }

    public bool IsMale => playerSex == Sex.Male;
    public bool IsFemale => playerSex == Sex.Female;

    public static bool blackedOut = false;

    // House stats
    public static BroughtOptions broughtItems = BroughtOptions.None;

    // Party stats
    public static int hoopsScore = 0;
    public static bool wonDarts = false;
    public static int arcadeScore = 0;
    public static bool letDrunkFriendDrive = false;
    public static bool helpedRagingDrunk = false;

    // Bonfire stats
    public static bool playerGrabbedAlcohol = false; // This is for the first scenario, when at the cooler
    public static bool playerWentWithFlirt = false;
    public static bool playerDrankMysteryDrink = false;
    public static bool playerStoppedFire = false;
    public static bool called911 = false;

    // Party Interactions
    public static bool talkedToCouchNPC = false;
    public static bool talkedToDrunkDriverNPC = false;
    public static bool talkedToDrunkRageNPC = false;
    public static bool talkedToEdibleNPC = false;
    public static bool talkedToPhoneNPC = false;

    // Bonfire Interactions
    public static bool talkedToFlirtNPC = false;
    public static bool talkedToFireNPC = false;
    public static bool talkedToSmokingNPC = false;

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
            return;
        }

        // Only runs in editor so it won't accidentally set drink count for players
#if UNITY_EDITOR
        if (startDrunk)
        {
            DrinkCount = 99;
        }
#endif
    }

    public void IncrementHoopsScored()
    {
        hoopsScore++;
    }

    //Even triggered when current drinks equals or exceeds max drinks to allow for tracking overdrinking

}
