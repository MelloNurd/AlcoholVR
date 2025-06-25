using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DemoScene : MonoBehaviour
{
    [SceneDropdown, SerializeField] private string _sceneChange;

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
        await UniTask.Delay(12_000);

        var temp = new PhoneMessage
        {
            Sender = "Markus",
            Content = "Hey! I'm throwing a party, can you bring snacks and beer?",
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
                obj4?.Complete();
                PlayerPrefs.SetInt("BroughtBeer", 1);
                Phone.Instance.LoadObjectives();
            }
            if (interactable.name.Contains("Food"))
            {
                obj3?.Complete();
                if (obj5 == null)
                {
                    obj5 = ObjectiveManager.Instance.CreateObjectiveObject(new Objective("Go talk to your dad.", 1, _dadTransform));
                }
                Phone.Instance.LoadObjectives();
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
        if (obj6 == null) return;

        SceneManager.LoadScene(_sceneChange);
        obj6?.Complete();
    }
}
