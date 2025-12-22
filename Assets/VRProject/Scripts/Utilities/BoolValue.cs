using UnityEngine;

[CreateAssetMenu(fileName = "BoolValue", menuName = "ValueFields/BoolValue")]
public class BoolValue : ScriptableObject
{
    [field: SerializeField] public bool Value { get; set; } = false;

    public static implicit operator bool(BoolValue boolValue)
    {
        return boolValue.Value;
    }
}
