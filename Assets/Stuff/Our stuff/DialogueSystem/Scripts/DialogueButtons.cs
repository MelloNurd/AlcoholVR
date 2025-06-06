using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueButtons : MonoBehaviour
{
    public static DialogueButtons Instance { get; private set; }

    [SerializeField] private GameObject _dialogueButtonPrefab;

    [SerializeField, Range(0, 360)] private float _buttonAngleSpacing = 30f;

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

    public void CreateDialogueButtons(DialogueSystem system, bool reverseOrder = false)
    {
        ClearButtons();

        Dialogue currentDialogue = system.currentDialogue;

        int countCache = currentDialogue.options.Count;
        Transform cameraCache = Camera.main.transform;

        for (int i = 0; i < countCache; i++)
        {
            int index = i % currentDialogue.options.Count; // weird behavior needed with lambda function, called a closure. plus allows us to multiply count above for debugging without breaking things

            var angleCalculation = (i * _buttonAngleSpacing) - (_buttonAngleSpacing * (countCache * 0.5f - 0.5f)); // angle in degrees
            Vector3 angle = Quaternion.AngleAxis(angleCalculation, Vector3.up) * cameraCache.forward; // angle as a normalized vector
            Vector3 spawnPosition = (cameraCache.position + angle) - new Vector3(0, cameraCache.position.y * 0.3f, 0); // position in world
            Quaternion spawnRotation = Quaternion.LookRotation(spawnPosition - cameraCache.position, Vector3.up) * Quaternion.Euler(-90, 0, 0); // rotation to face camera

            PhysicalButton optionButton = Instantiate(_dialogueButtonPrefab, spawnPosition, spawnRotation, transform).GetComponent<PhysicalButton>();
            optionButton.name = "DialogueButton: " + currentDialogue.options[index].text;

            optionButton.OnButtonDown.AddListener(() => system.SwitchDialogue(index));

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
