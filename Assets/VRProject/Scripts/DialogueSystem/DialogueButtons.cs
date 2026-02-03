using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class DialogueButtons : MonoBehaviour
{
    public static DialogueButtons Instance { get; private set; }

    [SerializeField] private GameObject _dialogueButtonPrefab;
    [SerializeField] private AudioClip _buttonAppearSound;

    private List<PhysicalButton> _activeButtons = new List<PhysicalButton>();

    [SerializeField, UnityEngine.Range(0, 360)] private float _buttonAngleSpacing = 30f;
    [SerializeField] private float _spawnDistanceFromPlayer = 0.6f;

    private GameObject _buttonParentObj;

    private int currentButtonCount = 0;

    public static UnityEvent OnButtonsSpawn = new();
    public static UnityEvent<PhysicalButton> OnButtonPressed = new();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject.transform.parent); // global manager
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_buttonParentObj == null)
        {
            _buttonParentObj = new GameObject("DialogueButtons");
            DontDestroyOnLoad(_buttonParentObj);
        }
    }

    private void Update()
    {
        if(Player.Instance == null) return;
        _buttonParentObj.transform.position = Player.Instance.CamPosition.AddY(-0.2f);

        // Calculate deadzone effect based on current button count
        if (currentButtonCount > 0)
        {
            float totalArcSpan = (currentButtonCount - 1) * _buttonAngleSpacing;
            float deadzoneMultiplier = Mathf.Clamp01(1f - (totalArcSpan / 180f)); // Reduce rotation as arc gets wider
            deadzoneMultiplier = Mathf.Max(deadzoneMultiplier, 0.1f); // Minimum 10% rotation to prevent complete lock

            Vector3 targetForward = Player.Instance.Forward.WithY(0);
            Vector3 currentForward = _buttonParentObj.transform.forward;

            // Apply deadzone by reducing the rotation amount
            Vector3 dampedForward = Vector3.Slerp(currentForward, targetForward, deadzoneMultiplier * Time.deltaTime * 2f);
            _buttonParentObj.transform.rotation = Quaternion.LookRotation(dampedForward);
        }
        else
        {
            // No buttons, use normal rotation
            _buttonParentObj.transform.rotation = Quaternion.LookRotation(Player.Instance.Forward.WithY(0));
        }

#if UNITY_EDITOR
        HandleEditorKeyInput();
#endif
    }

    private void HandleEditorKeyInput()
    {
        void TryPressButton(int buttonIndex)
        {
            if (buttonIndex < _activeButtons.Count && _activeButtons[buttonIndex].IsInteractable)
            {
                _activeButtons[buttonIndex].ButtonPress();
            }
        }

        if (currentButtonCount > 0)
        {
            if (currentButtonCount > 1 && (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame))
            {
                TryPressButton(1);
            }
            else if (currentButtonCount > 2 && (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame))
            {
                TryPressButton(2);
            }
            else if (currentButtonCount > 3 && (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame))
            {
                TryPressButton(3);
            }
            else if (currentButtonCount > 4 && (Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame))
            {
                TryPressButton(4);
            }
            else if (currentButtonCount > 5 && (Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame))
            {
                TryPressButton(5);
            }
            else if (currentButtonCount > 6 && (Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame))
            {
                TryPressButton(6);
            }
            else if (currentButtonCount > 7 && (Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame))
            {
                TryPressButton(7);
            }
            else if (currentButtonCount > 8 && (Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame))
            {
                TryPressButton(8);
            }
            else if (currentButtonCount > 9 && Keyboard.current.digit0Key.wasPressedThisFrame)
            {
                TryPressButton(9);
            }
            else if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            {
                TryPressButton(0);
            }
        }
    }

    public bool TryCreateDialogueButtons(DialogueSystem system, Dialogue dialogue, bool reverseOrder = false)
    {
        if(dialogue == null || dialogue.dialogueText == null)
        {
            Debug.LogWarning("DialogueSystem or current tree/option is null. Cannot create buttons.");
            return false;
        }

        if (!TryGenerateSpawnPositions(dialogue.options.Count, out Vector3[] spawnPos))
        {
            Debug.LogWarning("Failed to find valid spawn positions for dialogue buttons.");
            return false;
        }

        int middleIndex = dialogue.options.Count / 2; // Calculate middle index for reverse order

        for (int i = 0; i < dialogue.options.Count; i++)
        {
            currentButtonCount++;

            int index = reverseOrder ? dialogue.options.Count - 1 - i : i; // adjust index for reverse order

            Quaternion spawnRotation = Quaternion.LookRotation(spawnPos[i] - Camera.main.transform.position, Vector3.up) * Quaternion.Euler(-90, 0, 0); // rotate to face camera

            PhysicalButton optionButton = Instantiate(_dialogueButtonPrefab, spawnPos[i], spawnRotation, _buttonParentObj.transform).GetComponent<PhysicalButton>();
            optionButton.name = "DialogueButton: " + dialogue.options[index].optionText;
            optionButton.interactableLayers = LayerMask.GetMask("PlayerHand"); // only the player hand can interact with these buttons
            _activeButtons.Add(optionButton);

            if (dialogue.options[i].DisableButton)
            {
                optionButton.DisableButton();
            }

            int closureIndex = i; // weird behavior needed with lambda function, called a closure
            optionButton.OnButtonDown.AddListener(async () => {
                if (!optionButton.IsInteractable) return;

                await UniTask.Delay(100); // small delay to allow button press sound to play
                dialogue.options[index].onOptionSelected?.Invoke(); // Invoke the option's selected event
                system.StartDialogue(dialogue.options[closureIndex].nextDialogue, 1);
                OnButtonPressed?.Invoke(optionButton);
            });

            optionButton.SetButtonText(dialogue.options[index].optionText);

            if (i == middleIndex && _buttonAppearSound != null)
            {
                optionButton.PlaySound(_buttonAppearSound);
            }
        }

        OnButtonsSpawn?.Invoke();
        return true;
    }

    private bool TryGenerateSpawnPositions(int amount, out Vector3[] spawnPositions)
    {
        spawnPositions = new Vector3[amount];

        bool validSpawn;
        int offset = 0;
        int increment = 1; // Will be 1 or -1
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 100f))
        {
            float leftRightValue = Vector3.Dot(hit.normal, Camera.main.transform.right);
            increment = (int)Mathf.Sign(leftRightValue);
        }

        do
        {
            validSpawn = true;

            for (int i = 0; i < amount; i++)
            {
                var angleCalculation = (i * _buttonAngleSpacing) - (_buttonAngleSpacing * (amount * 0.5f - 0.5f)) + (offset); // angle in degrees
                Vector3 angle = Quaternion.AngleAxis(angleCalculation, Vector3.up) * Camera.main.transform.forward.WithY(0).normalized; // angle as a vector
                Vector3 spawnPosition = Camera.main.transform.position + angle * _spawnDistanceFromPlayer; // position in world
                spawnPosition.y -= 0.2f; // adjust height to be slightly below camera

                spawnPositions[i] = spawnPosition;
            }

            foreach (Vector3 pos in spawnPositions)
            {
                Collider[] colliders = Physics.OverlapSphere(pos, 0.1f, -1, QueryTriggerInteraction.Ignore); // Check for existing colliders at the position
                foreach (Collider col in colliders)
                {
                    if (col.gameObject == gameObject) continue; // Avoid self-collision

                    // Just disabling this for now rather than refactoring, but this is no longer needed
                    //validSpawn = false;
                }
            }

            offset += increment;
        } 
        while (!validSpawn && Mathf.Abs(offset) < 360);

        if(!validSpawn)
        {
            spawnPositions = null;
            // Could try reducing spacing and trying once more, if this ends up happening often
            return false;
        }

        return true;
    }

    public void ClearButtons()
    {
        currentButtonCount = 0;
        foreach (Transform child in _buttonParentObj.transform)
        {
            if (child.TryGetComponent(out PhysicalButton button))
            {
                button.OnButtonDown.RemoveAllListeners(); // Remove all listeners to prevent memory leaks
                button.IsInteractable = false;
            }

            Collider[] colliders = child.GetComponentsInChildren<Collider>(); // Get all colliders in children  
            foreach (Collider grandChildCol in colliders)
            {
                grandChildCol.enabled = false; // Disable colliders to prevent interaction  
            }

            MeshRenderer[] meshes = child.GetComponentsInChildren<MeshRenderer>(); // Get all colliders in children  
            foreach (MeshRenderer grandChildMesh in meshes)
            {
                grandChildMesh.enabled = false; // Disable colliders to prevent interaction  
            }

            Destroy(child.gameObject, 0.5f); // Destroy the child object after a delay to allow audio to play
        }

        _activeButtons.Clear();
    }
}
