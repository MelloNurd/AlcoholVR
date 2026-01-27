using UnityEngine;
using Bozo.ModularCharacters;
using System.Collections;

public class FaceSaver : MonoBehaviour
{
    [System.Serializable]
    public class FaceData
    {
        // Face shape values
        public float faceShape;
        public float lash;
        public float brow;
        public float ear;
        public float earAngle;
        
        // Eye socket position
        public float eyePosX;
        public float eyePosY;
        public float eyePosZ;
        public float eyeTilt;
        
        // Eye socket scale
        public float eyeScaleX;
        public float eyeScaleY;
        public float eyeScaleZ;
        
        // Eye shape values
        public float eyeUp;
        public float eyeDown;
        public float eyeSquare;
        
        // Nose values
        public float noseWidth;
        public float noseUp;
        public float noseDown;
        public float noseAngle;
        
        // Mouth values
        public float mouthWidth;
        public float mouthThin;
    }

    private FaceData savedFaceData;

    /// <summary>
    /// Saves the current face slider values from the OutfitSystem
    /// </summary>
    public void SaveFaceSliders(OutfitSystem outfitSystem)
    {
        if (outfitSystem == null)
        {
            Debug.LogWarning("FaceSaver: OutfitSystem is null, cannot save face data");
            return;
        }

        savedFaceData = new FaceData();

        // Save face shape values using the shape system
        savedFaceData.faceShape = outfitSystem.GetShapeValue("Squareness");
        savedFaceData.lash = outfitSystem.GetShapeValue("LashLength");
        savedFaceData.brow = outfitSystem.GetShapeValue("BrowsThickness");
        savedFaceData.ear = outfitSystem.GetShapeValue("EarsElf");
        savedFaceData.earAngle = outfitSystem.GetShapeValue("EarAngle");

        // Save eye socket modifier values
        if (outfitSystem.bodyModifiers.ContainsKey("eyeRoot_l"))
        {
            var eyeMod = outfitSystem.bodyModifiers["eyeRoot_l"];
            savedFaceData.eyePosX = eyeMod.xPosValue;
            savedFaceData.eyePosY = eyeMod.yPosValue;
            savedFaceData.eyePosZ = eyeMod.zPosValue;
            savedFaceData.eyeTilt = eyeMod.rotation;
            savedFaceData.eyeScaleX = eyeMod.xScaleValue;
            savedFaceData.eyeScaleY = eyeMod.yScaleValue;
            savedFaceData.eyeScaleZ = eyeMod.zScaleValue;
        }

        // Save more face shape values
        savedFaceData.eyeUp = outfitSystem.GetShapeValue("EyesOuterCornersHigh");
        savedFaceData.eyeDown = outfitSystem.GetShapeValue("EyesOuterCornersLow");
        savedFaceData.eyeSquare = outfitSystem.GetShapeValue("EyesSquare");
        savedFaceData.noseWidth = outfitSystem.GetShapeValue("NoseWidth");
        savedFaceData.noseUp = outfitSystem.GetShapeValue("NoseTiltUp");
        savedFaceData.noseDown = outfitSystem.GetShapeValue("NoseTiltDown");
        savedFaceData.noseAngle = outfitSystem.GetShapeValue("NoseBridgeCurve");
        savedFaceData.mouthWidth = outfitSystem.GetShapeValue("MouthWide");
        savedFaceData.mouthThin = outfitSystem.GetShapeValue("MouthThin");

        Debug.Log($"FaceSaver: Face data saved - Brow: {savedFaceData.brow}, Ear: {savedFaceData.earAngle}");
    }

    /// <summary>
    /// Loads the saved face slider values back to the OutfitSystem with a delay
    /// to ensure the new head is fully initialized
    /// </summary>
    public void LoadFaceSliders(OutfitSystem outfitSystem)
    {
        StartCoroutine(LoadFaceSlidersDelayed(outfitSystem));
    }

    private IEnumerator LoadFaceSlidersDelayed(OutfitSystem outfitSystem)
    {
        if (outfitSystem == null)
        {
            Debug.LogWarning("FaceSaver: OutfitSystem is null, cannot load face data");
            yield break;
        }

        if (savedFaceData == null)
        {
            Debug.LogWarning("FaceSaver: No saved face data to load");
            yield break;
        }

        // Wait a frame to ensure the new head is fully attached and initialized
        yield return null;

        // Load face shape values using the shape system
        outfitSystem.SetShape("Squareness", savedFaceData.faceShape);
        outfitSystem.SetShape("LashLength", savedFaceData.lash);
        outfitSystem.SetShape("BrowsThickness", savedFaceData.brow);
        outfitSystem.SetShape("EarsElf", savedFaceData.ear);
        outfitSystem.SetShape("EarAngle", savedFaceData.earAngle);

        // Load eye socket modifier values
        if (outfitSystem.bodyModifiers.ContainsKey("eyeRoot_l"))
        {
            var eyeMod = outfitSystem.bodyModifiers["eyeRoot_l"];
            
            // Set position
            eyeMod.SetPosition(savedFaceData.eyePosX, savedFaceData.eyePosY, savedFaceData.eyePosZ);
            
            // Set rotation
            eyeMod.SetRotation(savedFaceData.eyeTilt);
            
            // Set scale
            eyeMod.SetScale(savedFaceData.eyeScaleX, savedFaceData.eyeScaleY, savedFaceData.eyeScaleZ, eyeMod.scaleValue);
        }

        // Load more face shape values
        outfitSystem.SetShape("EyesOuterCornersHigh", savedFaceData.eyeUp);
        outfitSystem.SetShape("EyesOuterCornersLow", savedFaceData.eyeDown);
        outfitSystem.SetShape("EyesSquare", savedFaceData.eyeSquare);
        outfitSystem.SetShape("NoseWidth", savedFaceData.noseWidth);
        outfitSystem.SetShape("NoseTiltUp", savedFaceData.noseUp);
        outfitSystem.SetShape("NoseTiltDown", savedFaceData.noseDown);
        outfitSystem.SetShape("NoseBridgeCurve", savedFaceData.noseAngle);
        outfitSystem.SetShape("MouthWide", savedFaceData.mouthWidth);
        outfitSystem.SetShape("MouthThin", savedFaceData.mouthThin);

        Debug.Log($"FaceSaver: Face data loaded - Brow: {savedFaceData.brow}, Ear: {savedFaceData.earAngle}");
    }

    /// <summary>
    /// Returns true if face data has been saved
    /// </summary>
    public bool HasSavedData()
    {
        return savedFaceData != null;
    }

    /// <summary>
    /// Clears the saved face data
    /// </summary>
    public void ClearSavedData()
    {
        savedFaceData = null;
        Debug.Log("FaceSaver: Saved face data cleared");
    }
}
