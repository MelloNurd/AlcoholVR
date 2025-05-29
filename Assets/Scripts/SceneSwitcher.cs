using UnityEngine;
using UnityEngine.InputSystem;

public class SceneSwitcher : MonoBehaviour
{
    bool isPlayerInside = false;
    [SerializeField] string sceneToLoad; // Name of the scene to load
    private XRIDefaultInputActions map;
    GameObject text;

    private void OnEnable()
    {
        // Initialize the input actions
        map = new XRIDefaultInputActions();
        map.Enable(); // Enable the input actions
    }

    private void OnDisable()
    {
        // Disable the input actions
        map.Disable();
    }

    private void Start()
    {
        text = GameObject.Find("Text");
        text.SetActive(false); // Hide the text initially
    }

    // Update is called once per frame
    void Update()
    {
        if(isPlayerInside)
        {
            //if player presses right trigger for vr headset, switch scene
            if(map.XRIRightInteraction.Activate.triggered)
            {
                SwitchScene();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            text.SetActive(true); 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            text.SetActive(false); 
        }
    }

    private void SwitchScene()
    {
        if (isPlayerInside)
        {
            Debug.Log("Switching to scene: " + sceneToLoad);
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
    }
}
