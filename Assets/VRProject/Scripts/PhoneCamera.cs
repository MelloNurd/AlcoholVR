using System.Collections;
using UnityEngine;
using System.IO;
using EditorAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhoneCamera : MonoBehaviour
{
    [SerializeField] private Camera _phoneCamera;
    [SerializeField] private Material _cameraScreenMaterial;
    private int _pictureCount = 0;
    private string _photosFolderPath;

    private void Start()
    {
        // Create CurrentPhotos folder if it doesn't exist
        _photosFolderPath = Path.Combine(Application.persistentDataPath, "CurrentPhotos");
        if (!Directory.Exists(_photosFolderPath))
        {
            Directory.CreateDirectory(_photosFolderPath);
            Debug.Log($"Created photos folder at: {_photosFolderPath}");
        }
    }

    public void TakePicture()
    {
        if (_phoneCamera == null)
        {
            Debug.LogError("Phone camera not found. Cannot take picture.");
            return;
        }

        StartCoroutine(CapturePictureCoroutine());
    }

    private IEnumerator CapturePictureCoroutine()
    {
        // Wait for end of frame to ensure rendering is complete
        yield return new WaitForEndOfFrame();

        // Get the material's render texture
        RenderTexture renderTexture = null;
        if (_cameraScreenMaterial != null)
        {
            renderTexture = _cameraScreenMaterial.mainTexture as RenderTexture;
        }

        // Fallback: create a render texture from the camera
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(_phoneCamera.pixelWidth, _phoneCamera.pixelHeight, 24);
            _phoneCamera.targetTexture = renderTexture;
            _phoneCamera.Render();
        }

        // Create a temporary texture to read pixels into
        Texture2D screenshot = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
        RenderTexture.active = renderTexture;
        screenshot.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        screenshot.Apply();
        RenderTexture.active = null;

        // Generate filename with timestamp
        string filename = $"PhonePicture_{_pictureCount}_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        string filepath = Path.Combine(_photosFolderPath, filename);

        // Save as PNG
        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(filepath, bytes);

        Debug.Log($"Picture saved to: {filepath}");

        // Cleanup
        Destroy(screenshot);
        _pictureCount++;

        // Cleanup fallback render texture if we created it
        if (_cameraScreenMaterial == null && renderTexture != null)
        {
            _phoneCamera.targetTexture = null;
            Destroy(renderTexture);
        }
    }

    [Button]
    public void EditorTakePicture()
    {
        if (_photosFolderPath == null)
        {
            _photosFolderPath = Path.Combine(Application.persistentDataPath, "CurrentPhotos");
            if (!Directory.Exists(_photosFolderPath))
            {
                Directory.CreateDirectory(_photosFolderPath);
                Debug.Log($"Created photos folder at: {_photosFolderPath}");
            }
        }

        TakePicture();
    }
}
