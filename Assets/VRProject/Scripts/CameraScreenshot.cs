using UnityEngine;
using System.IO;
using EditorAttributes;

public class CameraScreenshot : MonoBehaviour
{
    public Camera targetCamera;
    public int width = 1920;
    public int height = 1080;

    [Button("Take Screenshot")]
    void TakeScreenshot()
    {
        RenderTexture rt = new RenderTexture(width, height, 24);
        targetCamera.targetTexture = rt;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        targetCamera.Render();

        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        targetCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        byte[] bytes = tex.EncodeToPNG();
        
        string filePath = Path.Combine(Application.dataPath, "CameraShot.png");
        int counter = 1;
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(Application.dataPath, $"cameraShot_{counter}.png");
            counter++;
        }
        
        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Saved camera screenshot to {filePath}!");
        
        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    [Button("Take Transparent Screenshot")]
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

        byte[] bytes = tex.EncodeToPNG();

        string filePath = Path.Combine(Application.dataPath, "CameraShot.png");
        int counter = 1;
        while (File.Exists(filePath))
        {
            filePath = Path.Combine(Application.dataPath, $"CameraShot_{counter}.png");
            counter++;
        }

        File.WriteAllBytes(filePath, bytes);

        Debug.Log($"Saved camera screenshot to {filePath}!");

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
