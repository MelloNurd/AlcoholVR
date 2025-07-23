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

    public static int hoopsScore = 0;
    public static bool wonDarts = false;
    public static int arcadeScore = 0;

    public static BroughtOptions BroughtToParty = BroughtOptions.None;
    public static bool LetDrunkFriendDrive = false;
    public static bool HelpedRagingDrunk = false;

    public bool tweenWarnings = false;

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
