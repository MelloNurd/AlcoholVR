using UnityEngine;

public class Toggler : MonoBehaviour
{
    public void ToggleActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
