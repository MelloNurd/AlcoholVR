using UnityEngine;

public class ArcadeBG : MonoBehaviour
{
    private MeshRenderer bg;

    private void Awake()
    {
        bg = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        bg.sharedMaterial.mainTextureOffset -= new Vector2(Arcade.Instance.GameSpeed * Time.fixedDeltaTime, 0);
    }
}
