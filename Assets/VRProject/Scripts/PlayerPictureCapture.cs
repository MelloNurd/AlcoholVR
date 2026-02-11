using System.Collections;
using System.IO;
using UnityEngine;
using Bozo.ModularCharacters;

public class PlayerPictureCapture : MonoBehaviour
{
    public Camera targetCamera;
    public int width = 1920;
    public int height = 1080;

    [SerializeField] GameObject ScreenshotParent;
    [SerializeField] OutfitSystem characterSystem;
    
    [Tooltip("Delay in seconds to wait after OutfitSystem initializes before taking screenshot")]
    [SerializeField] float delayAfterInit = 1f;
    
    [Tooltip("Maximum time in seconds to wait for character to initialize")]
    [SerializeField] float initializationTimeout = 10f;

    [SerializeField] GameObject CharacterBody;
    [SerializeField] GameObject Head;
    SkinnedMeshRenderer headRenderer;

    void Awake()
    {
        if (characterSystem == null)
        {
            characterSystem = FindFirstObjectByType<OutfitSystem>();
        }

        StartCoroutine(WaitForCharacterAndCapture());
    }

    void Update()
    {
    }

    /// <summary>
    /// Waits for the BSMC_CharacterBase (OutfitSystem) to initialize and sit down,
    /// then takes a transparent screenshot and disables the screenshot parent.
    /// </summary>
    private IEnumerator WaitForCharacterAndCapture()
    {
        // If no character system is found, log a warning and exit
        if (characterSystem == null)
        {
            Debug.LogWarning("PlayerPictureCapture: No OutfitSystem found in scene. Cannot capture player picture.");
            yield break;
        }

        // Wait for the OutfitSystem to initialize FIRST
        float timeWaited = 0f;
        while (!characterSystem.initalized)
        {
            yield return null;
            timeWaited += Time.deltaTime;
            
            if (timeWaited >= initializationTimeout)
            {
                Debug.LogError($"PlayerPictureCapture: Timeout waiting for OutfitSystem to initialize (waited {initializationTimeout}s)");
                yield break;
            }
        }

        Debug.Log("PlayerPictureCapture: OutfitSystem initialized. Loading player character...");

        // NOW load the player character after OutfitSystem is ready
        yield return StartCoroutine(LoadPlayerCharacter());

        // Wait for the additional delay to allow animations to play and character to sit
        yield return new WaitForSeconds(delayAfterInit);

        // Find MalePhoeneticHead or FemalePhoeneticHead in the character body
        headRenderer = CharacterBody.transform.Find("MalePhoeneticHead")?.Find("CombinedSkinnedMesh").GetComponent<SkinnedMeshRenderer>();
        if (headRenderer == null)
        {
            headRenderer = CharacterBody.transform.Find("FemalePhoeneticHead")?.Find("CombinedSkinnedMesh").GetComponent<SkinnedMeshRenderer>();
        }

        // Set blend shapes for the screenshot pose

        headRenderer.SetBlendShapeWeight(headRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brows_Raised_R"), 1);
        headRenderer.SetBlendShapeWeight(headRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brows_Raised_L"), 10);
        headRenderer.SetBlendShapeWeight(headRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Wide_R"), 30);
        headRenderer.SetBlendShapeWeight(headRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Wide_L"), 30);
        headRenderer.SetBlendShapeWeight(headRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Happy_R"), 50);
        headRenderer.SetBlendShapeWeight(headRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Happy_L"), 50);
        headRenderer.SetBlendShapeWeight(headRenderer.sharedMesh.GetBlendShapeIndex("Expression_Jaw_Open"), 20);

        Animator animator = CharacterBody.GetComponent<Animator>();
        animator.enabled = false;
        Head.transform.localRotation = Quaternion.Euler(0, 0, -20);

        yield return new WaitForSeconds(2);

        Debug.Log("PlayerPictureCapture: Taking transparent screenshot...");

        // Capture the screenshot
        TakeTransparentScreenshot();

        // Disable the screenshot parent GameObject
        if (ScreenshotParent != null)
        {
            ScreenshotParent.SetActive(false);
            Debug.Log("PlayerPictureCapture: Screenshot parent disabled.");
        }
        else
        {
            Debug.LogWarning("PlayerPictureCapture: ScreenshotParent is not assigned.");
        }
    }

    /// <summary>
    /// Loads the player character from the X_Characters folder
    /// </summary>
    private IEnumerator LoadPlayerCharacter()
    {
        string playerCharacterName = "PlayerCharacter";
        string savesFolderPath = Path.Combine(Application.persistentDataPath, "BoZo_StylizedModularCharacters/CustomCharacters/X_Characters/PlayerCharacter.json");

        // Check if the player character JSON exists
        if (!File.Exists(savesFolderPath))
        {
            Debug.LogWarning($"PlayerPictureCapture: PlayerCharacter.json not found at {savesFolderPath}. Using default character.");
            yield break;
        }

        CharacterData characterData = null;
        bool loadFailed = false;

        try
        {
            // Read the JSON file
            string json = File.ReadAllText(savesFolderPath);
            characterData = JsonUtility.FromJson<CharacterData>(json);

            if (characterData == null)
            {
                Debug.LogWarning("PlayerPictureCapture: Failed to deserialize PlayerCharacter.json");
                loadFailed = true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"PlayerPictureCapture: Error loading player character: {ex.Message}");
            loadFailed = true;
        }

        if (loadFailed || characterData == null)
        {
            yield break;
        }

        Debug.Log($"PlayerPictureCapture: Loading player character '{characterData.characterName}'");

        // Load the character asynchronously using BMAC_SaveSystem
        yield return StartCoroutine(LoadCharacterAsync(characterData));
    }

    /// <summary>
    /// Loads character data asynchronously into the OutfitSystem
    /// </summary>
    private IEnumerator LoadCharacterAsync(CharacterData characterData)
    {
        // Load the character with manualShapeApply=false to apply shapes properly
        var task = BMAC_SaveSystem.LoadCharacter(characterSystem, characterData, manualShapeApply: false, async: true);
        
        // Wait for the async operation to complete
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            Debug.LogError($"PlayerPictureCapture: Error during character load: {task.Exception}");
        }
        else
        {
            Debug.Log("PlayerPictureCapture: Player character loaded successfully");
        }
    }

    void TakeTransparentScreenshot()
    {
        RenderTexture rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
        
        targetCamera.targetTexture = rt;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        targetCamera.Render();

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        // Flip the texture vertically to correct the upside-down orientation
        FlipTextureVertically(tex);

        DesaturateTexture(tex, saturationAmount: 0.7f);

        // Apply blur BEFORE encoding
        BlurTexture(tex, blurRadius: 1);
        FeatherAlphaEdges(tex, featherDistance: 5);

        byte[] bytes = tex.EncodeToPNG();

        string filePath = Path.Combine(Application.persistentDataPath, "PlayerImage.png");
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Saved player picture to {filePath}!");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    /// <summary>
    /// Flips a texture vertically by reversing pixel rows.
    /// </summary>
    private void FlipTextureVertically(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        Color[] flipped = new Color[pixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flipped[(height - 1 - y) * width + x] = pixels[y * width + x];
            }
        }

        texture.Apply();
    }

    private void BlurTexture(Texture2D texture, int blurRadius = 1)
    {
        Color[] pixels = texture.GetPixels();
        Color[] blurred = new Color[pixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color sum = Color.clear;
                int count = 0;

                for (int by = -blurRadius; by <= blurRadius; by++)
                {
                    for (int bx = -blurRadius; bx <= blurRadius; bx++)
                    {
                        int nx = x + bx;
                        int ny = y + by;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            sum += pixels[ny * width + nx];
                            count++;
                        }
                    }
                }

                blurred[y * width + x] = sum / count;
            }
        }

        texture.SetPixels(blurred);
        texture.Apply();
    }

    /// <summary>
    /// Feathers the alpha channel at edges for smoother blending with backgrounds.
    /// </summary>
    private void FeatherAlphaEdges(Texture2D texture, int featherDistance = 5)
    {
        Color[] pixels = texture.GetPixels();
        Color[] feathered = new Color[pixels.Length];
        System.Array.Copy(pixels, feathered, pixels.Length);

        // Find alpha edges and gradually reduce opacity
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                float alpha = pixels[idx].a;

                // Only process semi-transparent or edge pixels
                if (alpha > 0 && alpha < 1)
                {
                    float featherAmount = 0;
                    int edgeCount = 0;

                    // Check surrounding pixels for alpha transitions
                    for (int dy = -featherDistance; dy <= featherDistance; dy++)
                    {
                        for (int dx = -featherDistance; dx <= featherDistance; dx++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                            {
                                float neighborAlpha = pixels[ny * width + nx].a;
                                if (Mathf.Abs(alpha - neighborAlpha) > 0.1f)
                                {
                                    featherAmount += Mathf.Abs(alpha - neighborAlpha);
                                    edgeCount++;
                                }
                            }
                        }
                    }

                    // Reduce alpha at edges for smoother blending
                    if (edgeCount > 0)
                    {
                        feathered[idx].a = alpha * 0.85f;
                    }
                }
            }
        }

        texture.SetPixels(feathered);
        texture.Apply();
    }

    /// <summary>
    /// Reduces color saturation of the texture.
    /// saturationAmount: 0 = fully grayscale, 1 = original colors
    /// </summary>
    private void DesaturateTexture(Texture2D texture, float saturationAmount = 0.7f)
    {
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color pixel = pixels[i];
            
            // Calculate grayscale value
            float gray = pixel.r * 0.299f + pixel.g * 0.587f + pixel.b * 0.114f;
            
            // Blend between grayscale and original color
            pixels[i].r = Mathf.Lerp(gray, pixel.r, saturationAmount);
            pixels[i].g = Mathf.Lerp(gray, pixel.g, saturationAmount);
            pixels[i].b = Mathf.Lerp(gray, pixel.b, saturationAmount);
        }

        texture.SetPixels(pixels);
        texture.Apply();
    }
}
