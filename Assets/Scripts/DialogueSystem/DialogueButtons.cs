using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueButtons : MonoBehaviour
{
    public static DialogueButtons Instance { get; private set; }

    [SerializeField] private GameObject _dialogueButtonPrefab;

    [SerializeField, Range(0, 360)] private float _buttonAngleSpacing = 30f;

    private Vector3 _camPosition;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        _camPosition = Camera.main.transform.position;
    }

    public void CreateDialogueButtons(DialogueSystem system, bool reverseOrder = false)
    {
        ClearButtons();

        Dialogue currentDialogue = system.currentDialogue;

        for (int i = 0; i < currentDialogue.options.Count; i++)
        {
            int index = reverseOrder ? currentDialogue.options.Count - 1 - i : i; // adjust index for reverse order

            var angleCalculation = (i * _buttonAngleSpacing) - (_buttonAngleSpacing * (currentDialogue.options.Count * 0.5f - 0.5f)); // angle in degrees
            Vector3 angle = Quaternion.AngleAxis(angleCalculation, Vector3.up) * Camera.main.transform.forward.WithY(0).normalized; // angle as a vector
            Vector3 spawnPosition = _camPosition + angle * 0.75f; // position in world
            spawnPosition.y = _camPosition.y - 0.25f; // adjust height to be slightly below camera
            Quaternion spawnRotation = Quaternion.LookRotation(spawnPosition - _camPosition, Vector3.up) * Quaternion.Euler(-90, 0, 0); // rotate to face camera

            PhysicalButton optionButton = Instantiate(_dialogueButtonPrefab, spawnPosition, spawnRotation, transform).GetComponent<PhysicalButton>();
            optionButton.name = "DialogueButton: " + currentDialogue.options[index].text;

            int closerIndex = i; // weird behavior needed with lambda function, called a closure
            optionButton.OnButtonDown.AddListener(() => system.SwitchDialogue(closerIndex));

            optionButton.SetButtonText(currentDialogue.options[index].text);
        }
    }

    public void ClearButtons()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }
}
