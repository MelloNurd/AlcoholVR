using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerPictures : MonoBehaviour
{
    //List of game objects
    [SerializeField] List<GameObject> PlayerPolaroids = new List<GameObject>();
    List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    List<int> usedObjects = new List<int>();
    List<Texture2D> loadedImages = new List<Texture2D>();
    [SerializeField] float imageBrightness = 1.5f; // Adjustable brightness multiplier for the image
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < PlayerPolaroids.Count; i++)
        {
            //Get the mesh renderer from each game object's child called "Image"
            MeshRenderer meshRenderer = PlayerPolaroids[i].transform.Find("Image").GetComponent<MeshRenderer>();
            meshRenderers.Add(meshRenderer);
        }

        LoadPhotosAndApplyToPolaroids();
        DisableUnusedPolaroids();
    }

    void LoadPhotosAndApplyToPolaroids()
    {
        // Get the photos folder path
        string photosFolderPath = Path.Combine(Application.persistentDataPath, "CurrentPhotos");

        // Check if folder exists
        if (!Directory.Exists(photosFolderPath))
        {
            Debug.LogWarning($"Photos folder not found at: {photosFolderPath}");
            return;
        }

        // Get all PNG files from the folder
        string[] imageFiles = Directory.GetFiles(photosFolderPath, "*.png");

        if (imageFiles.Length == 0)
        {
            Debug.LogWarning("No images found in CurrentPhotos folder.");
            return;
        }

        // Load all images
        foreach (string filePath in imageFiles)
        {
            Texture2D texture = LoadTextureFromFile(filePath);
            if (texture != null)
            {
                loadedImages.Add(texture);
            }
        }

        Debug.Log($"Loaded {loadedImages.Count} images from CurrentPhotos folder.");

        // Get the Unlit/Texture shader
        Shader unlitShader = Shader.Find("Unlit/Texture");
        if (unlitShader == null)
        {
            Debug.LogError("Unlit/Texture shader not found! Trying URP fallback.");
            unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
        }
        
        if (unlitShader == null)
        {
            Debug.LogError("Could not find Unlit shader. Using Standard shader instead.");
            unlitShader = Shader.Find("Standard");
        }

        // Apply images to polaroids
        int appliedCount = 0;

        for (int imageIndex = 0; imageIndex < loadedImages.Count && usedObjects.Count < meshRenderers.Count; imageIndex++)
        {
            int randomPolaroidIndex = PickRandomPolaroid();
            
            // Create a new material with Unlit/Texture shader
            Material materialInstance = new Material(unlitShader);
            materialInstance.name = "PolaroidMaterial_" + randomPolaroidIndex;
            
            // Brighten the loaded image texture
            Texture2D brightenedTexture = BrightenTexture(loadedImages[imageIndex], imageBrightness);
            materialInstance.mainTexture = brightenedTexture;
            
            // Set the material on the mesh renderer
            meshRenderers[randomPolaroidIndex].material = materialInstance;

            Debug.Log($"Applied image {imageIndex} to polaroid {randomPolaroidIndex} with shader: {unlitShader.name}");
            appliedCount++;
        }

        Debug.Log($"Applied {appliedCount} images to {appliedCount} polaroids. Total polaroids: {meshRenderers.Count}");
    }

    Texture2D LoadTextureFromFile(string filePath)
    {
        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            
            // Create texture with RGBA32 for better color handling
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            texture.LoadImage(fileData, markNonReadable: false);
            
            texture.name = System.IO.Path.GetFileNameWithoutExtension(filePath);
            texture.filterMode = FilterMode.Trilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            
            Debug.Log($"Successfully loaded texture from: {filePath}");
            return texture;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading texture from {filePath}: {ex.Message}");
            return null;
        }
    }

    Texture2D BrightenTexture(Texture2D original, float brightness)
    {
        Texture2D brightened = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
        Color[] pixels = original.GetPixels();
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i].r = Mathf.Clamp01(pixels[i].r * brightness);
            pixels[i].g = Mathf.Clamp01(pixels[i].g * brightness);
            pixels[i].b = Mathf.Clamp01(pixels[i].b * brightness);
        }
        
        brightened.SetPixels(pixels);
        brightened.Apply();
        
        return brightened;
    }

    int PickRandomPolaroid()
    {
        int randomIndex = Random.Range(0, meshRenderers.Count);
        while (usedObjects.Contains(randomIndex))
        {
            randomIndex = Random.Range(0, meshRenderers.Count);
        }
        usedObjects.Add(randomIndex);
        return randomIndex;
    }

    void DisableUnusedPolaroids()
    {
        for (int i = 0; i < meshRenderers.Count; i++)
        {
            if (!usedObjects.Contains(i))
            {
                PlayerPolaroids[i].SetActive(false);
            }
        }
    }
}
