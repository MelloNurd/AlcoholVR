using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GrabbableItemManager : MonoBehaviour
{
    public static GrabbableItemManager Instance { get; private set; }
    public static UnityEvent<Hand, GameObject> OnItemGrabbed = new UnityEvent<Hand, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
        {
            InitializeAllGrabbables();
        };
    }

    private void InitializeAllGrabbables()
    {
        XRGrabInteractable[] grabbables = FindObjectsByType<XRGrabInteractable>(FindObjectsSortMode.None);
        foreach (var grabbable in grabbables)
        {
            // Remove any existing listener to avoid duplicates
            grabbable.selectEntered.RemoveListener(OnGrabbed);
            grabbable.selectEntered.AddListener(OnGrabbed);
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        GameObject grabbedObject = args.interactableObject.transform.gameObject;
        Hand hand = DetermineHand(args.interactorObject);

        // Update held items tracking
        if (hand == Hand.Right)
            HeldItems.RightHandItem = grabbedObject;
        else
            HeldItems.LeftHandItem = grabbedObject;

        OnItemGrabbed.Invoke(hand, grabbedObject);
    }

    private Hand DetermineHand(IXRInteractor interactor)
    {
        // Check the interactor's GameObject name or tag to determine hand
        string name = interactor.transform.parent.name.ToLower();
        if (name.Contains("right"))
            return Hand.Right;

        return Hand.Left;
    }
}

public enum Hand
{
    Left,
    Right
}

public static class HeldItems
{
    public static GameObject LeftHandItem { get; set; }
    public static GameObject RightHandItem { get; set; }

    public static bool IsInRightHand(GameObject item) => RightHandItem != null && item == RightHandItem;
    public static bool IsInLeftHand(GameObject item) => LeftHandItem != null && item == LeftHandItem;

    public static bool IsHoldingItem(GameObject item) => IsInRightHand(item) || IsInLeftHand(item);
}