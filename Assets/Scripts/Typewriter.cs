using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.Events;

public class Typewriter : MonoBehaviour
{
    [SerializeField] private float _typingSpeed = 0.05f; // Time in seconds between each character typed

    private TMP_Text _textComponent;

    private string _builtText;

    public UnityEvent OnCharacterType;
    public UnityEvent OnTextComplete;

    void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
    }

    public void SetWritingSpeed(float speedInSeconds)
    {
        _typingSpeed = speedInSeconds;
    }

    public void StartWriting(string text) => StartWriting(text, _typingSpeed);
    public async void StartWriting(string text, float typingSpeedOverride)
    {
        _textComponent.text = "";

        int delay = Mathf.RoundToInt(typingSpeedOverride * 1000);
        for (int i = 0; i < text.Length; i++)
        {
            _builtText += text[i];
            _textComponent.text = _builtText;
            OnCharacterType?.Invoke();
            await UniTask.Delay(delay);
        }

        OnTextComplete?.Invoke();
    }
}
