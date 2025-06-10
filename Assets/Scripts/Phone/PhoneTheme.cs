using UnityEngine;

[CreateAssetMenu(fileName = "PhoneTheme", menuName = "Scriptable Objects/PhoneTheme")]
public class PhoneTheme : ScriptableObject
{
    public Sprite BackgroundImage;
    public Color PrimaryColor;
    public Color SecondaryColor;
}
