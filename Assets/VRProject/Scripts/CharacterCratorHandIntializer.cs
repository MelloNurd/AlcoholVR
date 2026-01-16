using Bozo.ModularCharacters;
using UnityEngine;

public class CharacterCratorHandIntializer : MonoBehaviour
{
    [SerializeField] HandColorer leftHand;
    [SerializeField] HandColorer rightHand;
    [SerializeField] DataObject characterObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftHand.SetColorsFromCharacterObject(characterObject);
        rightHand.SetColorsFromCharacterObject(characterObject);
    }
}
