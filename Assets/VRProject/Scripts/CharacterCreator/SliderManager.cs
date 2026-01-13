using Bozo.ModularCharacters;
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

        // Body shape sliders - using new shape system
        float genderValue = outfitSystem.GetShapeValue("BodyType");
        bodySlider.value = genderValue;
        GlobalStats.Instance.SetSex(genderValue);
        bodySlider2.value = genderValue; // Second body slider also uses gender
        
        chestSlider.value = outfitSystem.GetShapeValue("Chest");

        // Body modifiers - using new modifier system
        if (outfitSystem.bodyModifiers.ContainsKey("root"))
        {
            heightSlider.value = outfitSystem.bodyModifiers["root"].scaleValue - 1f;
        }
        
        if (outfitSystem.bodyModifiers.ContainsKey("head"))
        {
            headSlider.value = outfitSystem.bodyModifiers["head"].scaleValue - 1f;
        }
        
        if (outfitSystem.bodyModifiers.ContainsKey("clavicle_l"))
        {
            shoulderSlider.value = outfitSystem.bodyModifiers["clavicle_l"].scaleValue - 1f;
        }

        // Face shape sliders - using new shape system
        faceShapeSlider.value = outfitSystem.GetShapeValue("Squareness");
        lashSlider.value = outfitSystem.GetShapeValue("LashLength");
        browSlider.value = outfitSystem.GetShapeValue("BrowThickness");
        earSlider.value = outfitSystem.GetShapeValue("EarsElf");
        
        // Eye socket modifiers - using new modifier system
        if (outfitSystem.bodyModifiers.ContainsKey("eyeRoot_l"))
        {
            var eyeMod = outfitSystem.bodyModifiers["eyeRoot_l"];
            eyePosXSlider.value = eyeMod.xPosValue;
            eyePosYSlider.value = eyeMod.yPosValue;
            eyePosZSlider.value = eyeMod.zPosValue;
            eyeTiltSlider.value = eyeMod.rotation;
            eyeScaleXSlider.value = eyeMod.xScaleValue;
            eyeScaleYSlider.value = eyeMod.yScaleValue;
            eyeScaleZSlider.value = eyeMod.zScaleValue;
        }

        // More face shape sliders - using new shape system
        eyeUpSlider.value = outfitSystem.GetShapeValue("EyesOuterCornersHigh");
        eyeDownSlider.value = outfitSystem.GetShapeValue("EyesOuterCornersLow");
        eyeSquareSlider.value = outfitSystem.GetShapeValue("EyesSquare");
        noseWidthSlider.value = outfitSystem.GetShapeValue("NoseWidth");
        noseUpSlider.value = outfitSystem.GetShapeValue("NoseTiltUp");
        noseDownSlider.value = outfitSystem.GetShapeValue("NoseTiltDown");
        noseAngleSlider.value = outfitSystem.GetShapeValue("NoseBridgeCurve");
        mouthWidthSlider.value = outfitSystem.GetShapeValue("MouthWide");
        mouthThinSlider.value = outfitSystem.GetShapeValue("MouthThin");

        // Eye material properties (unchanged)
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
