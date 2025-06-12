using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(AudioSource))]
public class Typewriter : MonoBehaviour
{
    [SerializeField] private float _typingSpeed = 0.02f; // Time in seconds between each character typed

    [SerializeField] private AudioClip _typingSound;

    private AudioSource _audioSource;

    private TMP_Text _textComponent;

    public UnityEvent OnCharacterType;
    public UnityEvent OnTextComplete;

    void Awake()
    {
        _textComponent = GetComponentInChildren<TMP_Text>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void SetWritingSpeed(float speedInSeconds)
    {
        _typingSpeed = speedInSeconds;
    }

    public async UniTask StartWriting(string text) => await StartWriting(text, _typingSpeed);
    public async UniTask StartWriting(string text, float typingSpeedOverride)
    {
        if(_textComponent == null)
        {
            Debug.LogError($"TMP_Text component not found for {gameObject.name}'s {this.name}.");
            return;
        }

        _textComponent.text = text;

        int delay = Mathf.RoundToInt(typingSpeedOverride * 1000);
        for (int i = 0; i < text.Length; i++)
        {
            _textComponent.maxVisibleCharacters = i + 1; // Show one more character each iteration
            if (_typingSound != null)
            {
                _audioSource.PlayOneShot(_typingSound);
            }
            OnCharacterType?.Invoke();
            await UniTask.Delay(delay);
        }

        OnTextComplete?.Invoke();
    }

    public int GetWritingSpeed()
    {
        return Mathf.RoundToInt(_typingSpeed * 1000);
    }
}
