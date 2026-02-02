using System.Collections;
using uLipSync;
using UnityEngine;

public class LipSyncAutoset : MonoBehaviour
{
    private uLipSyncBlendShape uLipSyncBlendShape;
    private bool isInitialized = false;
    
    void Start()
    {
        // Start a coroutine to handle initialization with retries
        StartCoroutine(InitializeLipSync());
    }
    
    private IEnumerator InitializeLipSync()
    {
        // Wait a frame to ensure hierarchy is set up
        yield return null;
        
        uLipSyncBlendShape = GetComponent<uLipSyncBlendShape>();
        
        // Retry up to 10 times with a delay, in case the head hasn't loaded yet
        int retryCount = 0;
        const int maxRetries = 10;
        const float retryDelay = 0.5f;
        
        while (retryCount < maxRetries && !isInitialized)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = transform.Find("Body/BSMC_CharacterBase/MalePhoeneticHead/CombinedSkinnedMesh")?.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer == null)
            {
                skinnedMeshRenderer = transform.Find("Body/BSMC_CharacterBase/FemalePhoeneticHead/CombinedSkinnedMesh")?.GetComponent<SkinnedMeshRenderer>();
            }

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
                
                isInitialized = true;
                Debug.Log("LipSyncAutoset: Successfully initialized lip sync.");
                yield break;
            }
            
            retryCount++;
            yield return new WaitForSeconds(retryDelay);
        }
        
        if (!isInitialized)
        {
            Debug.LogError("LipSyncAutoset: Could not find SkinnedMeshRenderer for phonetic head after " + maxRetries + " retries.");
        }
    }
}
