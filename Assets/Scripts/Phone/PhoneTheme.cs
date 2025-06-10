using UnityEngine;

[CreateAssetMenu(fileName = "PhoneTheme", menuName = "Scriptable Objects/PhoneTheme")]
public class PhoneTheme : ScriptableObject
{
    public Sprite BackgroundImage;
    public Color ClockBackgroundColor = Color.black;
    public Color PrimaryColor = Color.white;
    public Color SecondaryColor = Color.gray;
    public Color TertiaryColor = new Color(1, 1, 1, 0.08f);
}
