using uLipSync;
using UnityEngine;

public class LipSyncAutoset : MonoBehaviour
{
    uLipSyncBlendShape uLipSyncBlendShape;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uLipSyncBlendShape = GetComponent<uLipSyncBlendShape>();
        
        //Get skinned mesh renderer from this/Body/BSMC_CharacterBase/MalePhoeneticHead/CombinedSkinnedMesh or FemalePhoeneticHead/CombinedSkinnedMesh
        SkinnedMeshRenderer skinnedMeshRenderer = transform.Find("Body/BSMC_CharacterBase/MalePhoeneticHead/CombinedSkinnedMesh")?.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer == null)
        {
            skinnedMeshRenderer = transform.Find("Body/BSMC_CharacterBase/FemalePhoeneticHead/CombinedSkinnedMesh")?.GetComponent<SkinnedMeshRenderer>();
        }

        // Assign the found SkinnedMeshRenderer to the uLipSyncBlendShape component
        if (skinnedMeshRenderer != null)
        {
            uLipSyncBlendShape.skinnedMeshRenderer = skinnedMeshRenderer;
            
            // Clear any existing blend shapes and set up the phoneme blend shape table
            uLipSyncBlendShape.blendShapes.Clear();
            
            uLipSyncBlendShape.AddBlendShape("A", "A");
            uLipSyncBlendShape.AddBlendShape("E", "E");
            uLipSyncBlendShape.AddBlendShape("I", "I");
            uLipSyncBlendShape.AddBlendShape("O", "O");
            uLipSyncBlendShape.AddBlendShape("U", "U");
            uLipSyncBlendShape.AddBlendShape("-", "-");
        }
        else
        {
            Debug.LogError("LipSyncAutoset: Could not find SkinnedMeshRenderer for phonetic head.");
        }
    }
}
