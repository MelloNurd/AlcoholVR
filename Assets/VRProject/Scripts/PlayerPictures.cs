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

        // Get or create the SimpleLit shader
        Shader simpleLitShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (simpleLitShader == null)
        {
            Debug.LogError("SimpleLit shader not found! Trying 'Simple Lit' fallback.");
            simpleLitShader = Shader.Find("Simple Lit");
        }
        
        if (simpleLitShader == null)
        {
            Debug.LogError("Could not find SimpleLit shader. Using default shader instead.");
            simpleLitShader = Shader.Find("Standard");
        }

        // Apply images to polaroids
        int appliedCount = 0;

        for (int imageIndex = 0; imageIndex < loadedImages.Count && usedObjects.Count < meshRenderers.Count; imageIndex++)
        {
            int randomPolaroidIndex = PickRandomPolaroid();
            
            // Create a new material with SimpleLit shader
            Material materialInstance = new Material(simpleLitShader);
            materialInstance.name = "PolaroidMaterial_" + randomPolaroidIndex;
            materialInstance.color = Color.white;
            materialInstance.mainTexture = loadedImages[imageIndex];
            
            // Set the material on the mesh renderer
            meshRenderers[randomPolaroidIndex].material = materialInstance;

            Debug.Log($"Applied image {imageIndex} to polaroid {randomPolaroidIndex} with shader: {simpleLitShader.name}");
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
