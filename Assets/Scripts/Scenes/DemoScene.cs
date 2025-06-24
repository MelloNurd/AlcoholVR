using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DemoScene : MonoBehaviour
{
    public bool broughtSnacks = false;
    public bool broughtBeer = false;

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

    private async void Start()
    {
        await UniTask.Delay(1);

        var temp = new PhoneMessage
        {
            Sender = "Markus",
            Content = "Hey! Are you coming to the party?",
        };

        Phone.Instance.ShowNotification(temp);

        obj1 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Grab snacks from the fridge.", 1, _fridgeTransform));
        obj2 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Grab beer from the fridge.", 1, _fridgeTransform));
    }

    // When you first GRAB the item
    public void OnItemGrabbed(SelectEnterEventArgs args)
    {
        if (args.interactableObject is XRBaseInteractable interactable)
        {
            Debug.Log("Grabbed: " + interactable.name);

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
            Debug.Log("Dropped: " + interactable.name);

            if (interactable.name.Contains("Bottle") && !broughtBeer)
            {
                obj4?.Complete();
                broughtBeer = true;
                if (obj5 == null)
                {
                    obj5 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Go talk to your dad.", 1, _dadTransform));
                }
            }
            else if (interactable.name.Contains("Food") && !broughtSnacks)
            {
                obj3?.Complete();
                broughtSnacks = true;
                if (obj5 == null)
                {
                    obj5 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Go talk to your dad.", 1, _dadTransform));
                }
            }
            Destroy(interactable.gameObject);
        }
    }

    public void OnDadTalkedTo()
    {
        obj5?.Complete();
        if (obj6 == null)
        {
            obj6 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Leave the house.", 1, _doorTransform));
        }
    }

    public void OnHouseLeft()
    {
        Debug.Log("You left the house!");
        obj6?.Complete();
    }
}
