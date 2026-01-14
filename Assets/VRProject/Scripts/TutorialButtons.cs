using UnityEngine;

public enum LeftControllerMaterialIndex
{
    X_BUTTON = 0,
    Y_BUTTON = 3,
    MENU_BUTTON = 4,
    LEFT_JOYSTICK = 5,
    LEFT_GRIP = 6,
    LEFT_TRIGGER = 7,
}
public enum RightControllerMaterialIndex
{
    A_BUTTON = 0,
    B_BUTTON = 2,
    OCULUS_BUTTON = 3,
    RIGHT_JOYSTICK = 4,
    RIGHT_GRIP = 5,
    RIGHT_TRIGGER = 7
}

public class TutorialButtons : MonoBehaviour
{
    public static TutorialButtons Instance { get; private set; }

    [SerializeField] private Material defaultMat;
    [SerializeField] private Material highlightedMat;

    [SerializeField] private SkinnedMeshRenderer leftControllerRenderer;
    [SerializeField] private SkinnedMeshRenderer rightControllerRenderer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void HighlightButton(LeftControllerMaterialIndex button)
    {
        if(leftControllerRenderer == null) return;

        var mats = leftControllerRenderer.sharedMaterials;
        mats[(int)button] = highlightedMat;

        leftControllerRenderer.sharedMaterials = mats;
    }

    public void HighlightButton(RightControllerMaterialIndex button)
    {
        if (rightControllerRenderer == null) return;

        var mats = rightControllerRenderer.sharedMaterials;
        mats[(int)button] = highlightedMat;

        rightControllerRenderer.sharedMaterials = mats;
    }
    
    public void ResetButton(LeftControllerMaterialIndex button)
    {
        if (leftControllerRenderer == null) return;
        var mats = leftControllerRenderer.sharedMaterials;
        mats[(int)button] = defaultMat;
        leftControllerRenderer.sharedMaterials = mats;
    }

    public void ResetButton(RightControllerMaterialIndex button)
    {
        if (rightControllerRenderer == null) return;
        var mats = rightControllerRenderer.sharedMaterials;
        mats[(int)button] = defaultMat;
        rightControllerRenderer.sharedMaterials = mats;
    }
}
