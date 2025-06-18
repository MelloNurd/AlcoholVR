using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueButtons : MonoBehaviour
{
    public static DialogueButtons Instance { get; private set; }

    [SerializeField] private GameObject _dialogueButtonPrefab;
    [SerializeField] private AudioClip _buttonAppearSound;

    [SerializeField, Range(0, 360)] private float _buttonAngleSpacing = 30f;
    [SerializeField] private float _spawnDistanceFromPlayer = 0.6f;

    private Vector3 _camPosition;

    private Light _spotLight;

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

        _spotLight = GetComponentInChildren<Light>(true);
        _spotLight.enabled = false;
    }

    private void Start()
    {
        _camPosition = Camera.main.transform.position;
    }

    public bool TryCreateDialogueButtons(DialogueSystem system, bool reverseOrder = false)
    {
        Dialogue currentDialogue = system.currentDialogue;

        if(!TryGenerateSpawnPositions(currentDialogue.options.Count, out Vector3[] spawnPos))
        {
            Debug.LogWarning("Failed to find valid spawn positions for dialogue buttons.");
            return false;
        }

        int middleIndex = currentDialogue.options.Count / 2; // Calculate middle index for reverse order

        for (int i = 0; i < currentDialogue.options.Count; i++)
        {
            int index = reverseOrder ? currentDialogue.options.Count - 1 - i : i; // adjust index for reverse order

            Quaternion spawnRotation = Quaternion.LookRotation(spawnPos[i] - Camera.main.transform.position, Vector3.up) * Quaternion.Euler(-90, 0, 0); // rotate to face camera

            PhysicalButton optionButton = Instantiate(_dialogueButtonPrefab, spawnPos[i], spawnRotation, transform).GetComponent<PhysicalButton>();
            optionButton.name = "DialogueButton: " + currentDialogue.options[index].text;

            int closerIndex = i; // weird behavior needed with lambda function, called a closure
            optionButton.OnButtonDown.AddListener(() => system.SwitchDialogue(closerIndex));

            optionButton.SetButtonText(currentDialogue.options[index].text);

            if (i == middleIndex && _buttonAppearSound != null)
            {
                optionButton.PlaySound(_buttonAppearSound);
            }
        }

        _spotLight.transform.position = Camera.main.transform.position.WithY(_camPosition.y + 1f);
        _spotLight.enabled = true;

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
                Vector3 spawnPosition = Camera.main.transform.position.WithY(_camPosition.y) + angle * _spawnDistanceFromPlayer; // position in world
                spawnPosition.y = _camPosition.y - 0.3f; // adjust height to be slightly below camera

                spawnPositions[i] = spawnPosition;
            }

            foreach (Vector3 pos in spawnPositions)
            {
                Collider[] colliders = Physics.OverlapSphere(pos, 0.1f); // Check for existing colliders at the position
                foreach (Collider col in colliders)
                {
                    if (col.gameObject == gameObject) continue; // Avoid self-collision
                    validSpawn = false;
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
        foreach (Transform child in transform)
        {
            if(child == _spotLight.transform) continue; // Skip spotlight

            if (child.TryGetComponent(out PhysicalButton button))
            {
                button.OnButtonDown.RemoveAllListeners(); // Remove all listeners to prevent memory leaks
                button.IsActive = false;
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

        _spotLight.enabled = false;
    }
}
