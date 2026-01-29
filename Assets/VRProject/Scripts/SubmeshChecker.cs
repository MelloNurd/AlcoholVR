using UnityEngine;

public class SubmeshChecker : MonoBehaviour
{
    void Start()
    {
        foreach (var r in FindObjectsOfType<MeshRenderer>())
        {
            var mesh = r.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh && r.sharedMaterials.Length > mesh.subMeshCount)
            {
                Debug.LogError($"BROKEN: {r.name}", r);
            }
        }
    }
}
