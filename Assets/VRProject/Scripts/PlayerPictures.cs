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

        // Apply images to polaroids
        int appliedCount = 0;

        for (int imageIndex = 0; imageIndex < loadedImages.Count && usedObjects.Count < meshRenderers.Count; imageIndex++)
        {
            int randomPolaroidIndex = PickRandomPolaroid();
            Material materialInstance = new Material(meshRenderers[randomPolaroidIndex].material);
            materialInstance.color = Color.white; // Ensure the material color is white to display the texture correctly
            materialInstance.mainTexture = loadedImages[imageIndex];
            meshRenderers[randomPolaroidIndex].material = materialInstance;

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
