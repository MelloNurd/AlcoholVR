using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroupPhoto : MonoBehaviour
{
    // Serialize list of images for each possible outcome of the group photo
    [SerializeField] private List<Texture2D> Photos = new List<Texture2D>();

    RawImage photoDisplay;

    public void Start()
    {
        photoDisplay = GetComponent<RawImage>();
    }

    public void SetPhoto(bool FemaleFlirt, bool NPCDied, bool DroveDrunk, bool NPCRaged)
    {
        string photoName = GetPhotoName(FemaleFlirt, NPCDied, DroveDrunk, NPCRaged);
        // Find the photo in the list that matches the generated name
        Texture2D selectedPhoto = Photos.Find(photo => photo.name == photoName);
        if (selectedPhoto != null)
        {
            photoDisplay.texture = selectedPhoto;
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
}
