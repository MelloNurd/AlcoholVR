using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GroupPhoto : MonoBehaviour
{
    // Serialize list of images for each possible outcome of the group photo
    [SerializeField] private List<Texture2D> Photos = new List<Texture2D>();

    RawImage BackgroundPhoto;
    RawImage PlayerPhoto;

    public void Awake()
    {
        BackgroundPhoto = GameObject.Find("BackgroundImage").GetComponent<RawImage>();
        PlayerPhoto = GameObject.Find("PlayerImage").GetComponent<RawImage>();
    }

    public void SetPhoto(bool FemaleFlirt, bool NPCDied, bool DroveDrunk, bool NPCRaged)
    {
        string photoName = GetPhotoName(FemaleFlirt, NPCDied, DroveDrunk, NPCRaged);
        // Find the photo in the list that matches the generated name
        Texture2D selectedPhoto = Photos.Find(photo => photo.name == photoName);
        if (selectedPhoto != null)
        {
            Texture2D playerImage = LoadPlayerImage();
            if (playerImage != null)
            {
                // Create a duplicate of the background to work with (preserves original)
                Texture2D backgroundCopy = DuplicateTexture(selectedPhoto);
                
                // Composite player image onto the copy
                Texture2D compositePhoto = CompositePlayerOntoBackground(backgroundCopy, playerImage, offsetX: 31, offsetY: 0);
                if (compositePhoto != null)
                {
                    BackgroundPhoto.texture = compositePhoto;
                    PlayerPhoto.gameObject.SetActive(false); // Hide the separate player photo since it's now composited
                    
                    // Optionally save the composite to disk for later use
                    SaveCompositePhoto(compositePhoto);
                }
            }
        }
        else
        {
            Debug.LogError("Photo not found: " + photoName);
        }
    }

    private string GetPhotoName(bool FemaleFlirt, bool NPCDied, bool DroveDrunk, bool NPCRaged)
    {
        string gender = FemaleFlirt ? "Female" : "Male";
        string suffix = "";

        // Build suffix based on outcomes
        if (DroveDrunk)
            suffix += "Drunk";
        
        if (NPCRaged)
            suffix += "Rage";
        
        if (NPCDied)
            suffix += "Dead";

        return gender + suffix;
    }

    /// <summary>
    /// Loads the player image saved by PlayerPictureCapture from persistent data path.
    /// </summary>
    private Texture2D LoadPlayerImage()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "PlayerImage.png");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"Player image not found at {filePath}");
            return null;
        }

        byte[] imageData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        
        if (texture.LoadImage(imageData))
        {
            Debug.Log($"Successfully loaded player image from {filePath}");
            return texture;
        }
        else
        {
            Debug.LogError($"Failed to load player image from {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Creates a duplicate of a texture to preserve the original.
    /// </summary>
    private Texture2D DuplicateTexture(Texture2D source)
    {
        // Convert to readable if needed
        if (!source.isReadable)
        {
            source = ConvertToReadableTexture(source);
        }

        Texture2D duplicate = new Texture2D(source.width, source.height, source.format, false);
        duplicate.SetPixels(source.GetPixels());
        duplicate.Apply();
        
        return duplicate;
    }

    /// <summary>
    /// Composites the player image onto the background photo at a specified offset.
    /// Creates a single flat image with no transparency to avoid mipmap issues.
    /// offsetX and offsetY are in pixels from the top-left corner.
    /// </summary>
    private Texture2D CompositePlayerOntoBackground(Texture2D backgroundPhoto, Texture2D playerPhoto, int offsetX = 0, int offsetY = 0)
    {
        if (backgroundPhoto == null || playerPhoto == null)
        {
            Debug.LogError("Cannot composite: background or player photo is null");
            return null;
        }

        // Convert to readable if needed
        if (!backgroundPhoto.isReadable)
        {
            backgroundPhoto = ConvertToReadableTexture(backgroundPhoto);
        }
        if (!playerPhoto.isReadable)
        {
            playerPhoto = ConvertToReadableTexture(playerPhoto);
        }

        int bgWidth = backgroundPhoto.width;
        int bgHeight = backgroundPhoto.height;
        int playerWidth = playerPhoto.width;
        int playerHeight = playerPhoto.height;

        Color[] backgroundPixels = backgroundPhoto.GetPixels();
        Color[] playerPixels = playerPhoto.GetPixels();

        Color[] compositedPixels = new Color[backgroundPixels.Length];
        System.Array.Copy(backgroundPixels, compositedPixels, backgroundPixels.Length);

        // Composite player image at offset position
        for (int py = 0; py < playerHeight; py++)
        {
            for (int px = 0; px < playerWidth; px++)
            {
                // Calculate background position with offset
                int bgX = px + offsetX;
                int bgY = py + offsetY;

                // Skip if outside background bounds
                if (bgX < 0 || bgX >= bgWidth || bgY < 0 || bgY >= bgHeight)
                    continue;

                int playerIdx = py * playerWidth + px;
                int bgIdx = bgY * bgWidth + bgX;

                Color playerColor = playerPixels[playerIdx];
                float playerAlpha = playerColor.a;

                // Blend: player * alpha + background * (1 - alpha)
                compositedPixels[bgIdx].r = Mathf.Lerp(compositedPixels[bgIdx].r, playerColor.r, playerAlpha);
                compositedPixels[bgIdx].g = Mathf.Lerp(compositedPixels[bgIdx].g, playerColor.g, playerAlpha);
                compositedPixels[bgIdx].b = Mathf.Lerp(compositedPixels[bgIdx].b, playerColor.b, playerAlpha);
            }
        }

        Texture2D compositeTexture = new Texture2D(bgWidth, bgHeight, TextureFormat.RGB24, false);
        compositeTexture.SetPixels(compositedPixels);
        compositeTexture.Apply();

        Debug.Log("Player image composited onto background");
        return compositeTexture;
    }

    /// <summary>
    /// Converts a non-readable texture to a readable one using RenderTexture.
    /// </summary>
    private Texture2D ConvertToReadableTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readable;
    }

    /// <summary>
    /// Saves the composite photo to disk, overwriting the previous composite.
    /// </summary>
    private void SaveCompositePhoto(Texture2D compositePhoto)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "CompositePhoto.png");
        
        byte[] bytes = compositePhoto.EncodeToPNG();
        File.WriteAllBytes(filePath, bytes);
        
        Debug.Log($"Saved composite photo to {filePath}");
    }
}
