using Cysharp.Threading.Tasks;
using EditorAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class House : MonoBehaviour
{
    [SerializeField] private SequencedNPC _dadNPC;
    private bool hasDadStarted = false;

    [SerializeField] private Transform _fridgeTransform;
    [SerializeField] private Transform _backpackTransform;
    [SerializeField] private Transform _dadTransform;
    [SerializeField] private Transform _doorTransform;

    ObjectiveSystem obj1; // Grab snacks from the fridge
    ObjectiveSystem obj2; // Grab beer from the fridge
    ObjectiveSystem obj3; // Bring snacks to your backpack
    ObjectiveSystem obj4; // Bring beer to your backpack
    ObjectiveSystem obj5; // Go talk to your dad
    ObjectiveSystem obj6; // Leave the house

    [SerializeField] GameObject door;
    [SerializeField] XRGrabInteractable exitInteractable;

    private async void Start()
    {
        await UniTask.Delay(12_000);

        var temp = new PhoneMessage
        {
            Sender = "Markus",
            Content = "Hey! I'm throwing a party. Bring snacks. And do you think you can sneak some beer from your parents?",
        };

        Phone.Instance.QueueNotification(temp);

        obj1 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Grab snacks from the fridge.", 1, _fridgeTransform));
        obj2 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Grab beer from the fridge.", 1, _fridgeTransform));

        _dadNPC.dialogueSystem.onStart.AddListener(OnDadTalkedTo);
    }

    private void Update()
    {
        if(Keyboard.current.f1Key.wasPressedThisFrame)
        {
            StartDad();
        }
    }

    public void StartDad()
    {
        if (hasDadStarted) return;
        hasDadStarted = true;
        StartCoroutine(DadDelay());
    }

    IEnumerator DadDelay()
    {
        Vector3 originalDoorPosition = door.transform.position;
        Quaternion originalDoorRotation = door.transform.rotation;

        yield return new WaitForSeconds(2f);
        door.transform.localPosition = new Vector3(-.459f, .1377f, -.489f);
        door.transform.localRotation = Quaternion.Euler(0, 90, 0);

        _dadNPC.StartNextSequence();

        yield return new WaitForSeconds(1f);
        door.transform.position = originalDoorPosition;
        door.transform.rotation = originalDoorRotation;

        yield return new WaitForSeconds(3f);
        exitInteractable.enabled = true;
    }

    // When you first GRAB the item
    public void OnItemGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactableObject is XRBaseInteractable interactable)
        {
            if (interactable.name.Contains("Food"))
            {
                obj1?.Complete();
                if (obj3 == null)
                {
                    obj3 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Bring snacks to your backpack.", 1, _backpackTransform));
                }
            }
            else if (interactable.name.Contains("Bottle"))
            {
                obj2?.Complete();
                if (obj4 == null)
                {
                    obj4 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Bring beer to your backpack.", 1, _backpackTransform));
                }
            }
        }
    }

    // When you DROP the item into the backpack
    public void OnBackpackDrop(SelectEnterEventArgs args)
    {
        if (args.interactableObject is XRBaseInteractable interactable)
        {
            if (interactable.name.Contains("Bottle"))
            {
                // Mark as alcohol no matter what if you input beer
                GlobalStats.broughtItems = GlobalStats.BroughtOptions.Alcohol;
                obj4?.Complete();
                PlayerPrefs.SetInt("BroughtBeer", 1);
            }
            if (interactable.name.Contains("Food"))
            {
                // Only mark snacks if they havent brought anything yet (does not overwrite alcohol)
                if (GlobalStats.broughtItems == GlobalStats.BroughtOptions.None)
                {
                    GlobalStats.broughtItems = GlobalStats.BroughtOptions.Snacks;
                }
                obj3?.Complete();
            }

            Destroy(interactable.gameObject);
        }
    }

    public void OnDadTalkedTo()
    {
        if (obj6 == null)
        {
            obj6 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Leave the house.", 1, _doorTransform));
            obj6.Begin();
        }
    }

    public void OnHouseLeft()
    {
        if (obj6 == null) return;

        obj6?.Complete();
    }
}
