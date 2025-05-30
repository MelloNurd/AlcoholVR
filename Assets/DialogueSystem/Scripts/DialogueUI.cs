using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; }

    [SerializeField] private GameObject _dialogueOptionRow;
    [SerializeField] private GameObject _dialogueOptionButton;

    private GameObject _dialogueOptionsHolder;

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

        _dialogueOptionsHolder = transform.Find("DialogueOptionsHolder").gameObject;
    }

    public void CreateDialogueButtons(DialogueSystem system)
    {
        ClearButtons();

        GameObject currentRow = null;
        Dialogue currentDialogue = system.currentDialogue;

        for (int i = 0; i < currentDialogue.options.Count; i++)
        {
            if (i % 3 == 0)
            {
                currentRow = Instantiate(_dialogueOptionRow, _dialogueOptionsHolder.transform);
                currentRow.name = "DialogueOptionRow_" + (i / 3);
            }


            Button optionButton = Instantiate(_dialogueOptionButton, currentRow.transform).GetComponent<Button>();
            optionButton.name = "DialogueOptionButton_" + i;

            int index = i; // weird behavior needed with lambda function, called a closure
            optionButton.onClick.AddListener(() => system.SwitchDialogue(index));

            optionButton.GetComponentInChildren<TMP_Text>().text = currentDialogue.options[i].text;
        }
    }

    public void ClearButtons()
    {
        foreach (Transform child in _dialogueOptionsHolder.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
