using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class SliderManager : MonoBehaviour
{
    [SerializeField] OutfitSystem outfitSystem;
    SkinnedMeshRenderer skinnedMeshRenderer;

    [Header("Body Tab Sliders")]
    [SerializeField] private Slider bodySlider;
    [SerializeField] private Slider chestSlider;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private Slider headSlider;
    [SerializeField] private Slider shoulderSlider;

    [Header("Head Tab Sliders (Left)")]
    [SerializeField] private Slider faceShapeSlider;
    [SerializeField] private Slider bodySlider2;
    [SerializeField] private Slider lashSlider;
    [SerializeField] private Slider browSlider;
    [SerializeField] private Slider earSlider;
    [SerializeField] private Slider eyePosXSlider;
    [SerializeField] private Slider eyePosYSlider;
    [SerializeField] private Slider eyePosZSlider;
    [SerializeField] private Slider eyeTiltSlider;
    [SerializeField] private Slider eyeScaleXSlider;
    [SerializeField] private Slider eyeScaleYSlider;
    [SerializeField] private Slider eyeScaleZSlider;

    [Header("Head Tab Sliders (Right)")]
    [SerializeField] private Slider eyeUpSlider;
    [SerializeField] private Slider eyeDownSlider;
    [SerializeField] private Slider eyeSquareSlider;
    [SerializeField] private Slider noseWidthSlider;
    [SerializeField] private Slider noseUpSlider;
    [SerializeField] private Slider noseDownSlider;
    [SerializeField] private Slider noseAngleSlider;
    [SerializeField] private Slider mouthWidthSlider;
    [SerializeField] private Slider mouthThinSlider;

    [Header("Eyes Tab Sliders")]
    [SerializeField] private Slider pupilSizeSlider;
    [SerializeField] private Slider irisSizeSlider;
    [SerializeField] private Slider outerSharpnessSlider;
    [SerializeField] private Slider innerSharpnessSlider;
    [SerializeField] private Slider irisOffsetXSlider;
    [SerializeField] private Slider irisOffsetYSlider;

    public void Start()
    {
        SetRenderer();
    }

    public void InitializeSliders()
    {
        if(skinnedMeshRenderer == null)
        {
            SetRenderer();
        }

        bodySlider.value = outfitSystem.Gender;
        GlobalStats.Instance.SetSex(outfitSystem.Gender);
        chestSlider.value = outfitSystem.ChestSize;
        heightSlider.value = outfitSystem.height;
        headSlider.value = outfitSystem.headSize;
        shoulderSlider.value = outfitSystem.shoulderWidth;

        faceShapeSlider.value = outfitSystem.FaceShape;
        bodySlider2.value = outfitSystem.Gender;
        lashSlider.value = outfitSystem.LashLength;
        browSlider.value = outfitSystem.BrowSize;
        earSlider.value = outfitSystem.EarTipLength;
        eyePosXSlider.value = outfitSystem.EyeSocketPosition.x;
        eyePosYSlider.value = outfitSystem.EyeSocketPosition.y;
        eyePosZSlider.value = outfitSystem.EyeSocketPosition.z;
        eyeTiltSlider.value = outfitSystem.EyeSocketRotation;
        eyeScaleXSlider.value = outfitSystem.EyeSocketScale.x;
        eyeScaleYSlider.value = outfitSystem.EyeSocketScale.y;
        eyeScaleZSlider.value = outfitSystem.EyeSocketScale.z;

        eyeUpSlider.value = outfitSystem.EyeUp;
        eyeDownSlider.value = outfitSystem.EyeDown;
        eyeSquareSlider.value = outfitSystem.EyeSquare;
        noseWidthSlider.value = outfitSystem.NoseWidth;
        noseUpSlider.value = outfitSystem.NoseUp;
        noseDownSlider.value = outfitSystem.NoseDown;
        noseAngleSlider.value = outfitSystem.NoseBridgeAngle;
        mouthWidthSlider.value = outfitSystem.MouthWide;
        mouthThinSlider.value = outfitSystem.MouthThin;

        pupilSizeSlider.value = skinnedMeshRenderer.materials[1].GetFloat("_PupilSize");
        irisSizeSlider.value = skinnedMeshRenderer.materials[1].GetFloat("_IrisSize");
        outerSharpnessSlider.value = skinnedMeshRenderer.materials[1].GetFloat("_OuterIrisColorSharpness");
        innerSharpnessSlider.value = skinnedMeshRenderer.materials[1].GetFloat("_InnerIrisColorShapness");
        var offset = skinnedMeshRenderer.materials[1].GetVector("_InnerIrisColorOffset");
        irisOffsetXSlider.value = offset.x;
        irisOffsetYSlider.value = offset.y;
    }

    public void SetRenderer()
    {
        // Only set skinnedMeshRenderer if outfitSystem and CharacterBody are not null
        if (outfitSystem != null && outfitSystem.CharacterBody != null)
        {
            skinnedMeshRenderer = outfitSystem.CharacterBody;
        }
    }
}
