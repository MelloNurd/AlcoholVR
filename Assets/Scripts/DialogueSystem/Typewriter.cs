using Cysharp.Threading.Tasks;
using TMPro;
using Unity.Hierarchy;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NaughtyAttributes;
using System;

public class Typewriter : MonoBehaviour
{
    [Header("Typewriter Settings")]
    [Tooltip("Base typing speed in seconds. This is the default speed for characters without a specific speed tag.")]
    [SerializeField] private float _baseTypingSpeed = 0.02f; // Time in seconds between each character typed

    public float DefaultWritingSpeed => _baseTypingSpeed * typingSpeedMultiplier; // The effective writing speed, adjusted by typeFaster
    public int DefaultWritingSpeedInMS => Mathf.RoundToInt(DefaultWritingSpeed * 1000); // Writing speed in milliseconds

    [Tooltip("Audio clip to play for each character typed. If null, no sound will be played.")]
    [SerializeField] private AudioClip _typingSound;

    [Tooltip("While true, typing speed will be increased, as if holding down a key.")]
    public bool typeFaster = false;
    private float typingSpeedMultiplier => typeFaster ? 0.4f : 1f;

    [Header("Events")]
    public UnityEvent OnCharacterType;
    public UnityEvent OnTextComplete;

    private CancellationTokenSource _cancelToken;

    private AudioSource _audioSource;

    private TMP_Text _textComponent;

    void Awake()
    {
        _textComponent = GetComponentInChildren<TMP_Text>();
        _audioSource = GetComponent<AudioSource>();
        if(_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            var text = DateTime.Now.ToString("HH:mm:ss") + " - " + "This is a test message with <speed=0.01>FAST WORDS NOW<speed=0.05>variable typing speeds to demonstrate the typewriter effect.";
            StartWriting(text);
            Debug.Log("clean text: " + PreprocessText(text, out float[] speeds));
            Debug.Log("times: " + String.Join(", ", speeds));

        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            CancelWriting();
        }
        else if(Input.GetKeyDown(KeyCode.Return))
        {
            SkipToEnd();
        }

        typeFaster = Input.GetKey(KeyCode.LeftShift);
    }

    /// <summary>
    /// Internal function to preprocess the text and extract typing speeds from tags.
    /// </summary>
    /// <param name="initialText">The original text to process (containing tags)</param>
    /// <param name="typingSpeeds">The outputted arary of typing speeds for each character</param>
    /// <returns>The cleaned string with no tags</returns>
    private string PreprocessText(string initialText, out float[] typingSpeeds)
    {
        // A speed tag resembles <speed=0.05>

        string text = initialText;
        
        // Create a new clean text without tags
        StringBuilder cleanText = new StringBuilder();
        List<float> speedsList = new List<float>();
        float currentSpeed = _baseTypingSpeed;
        
        int i = 0;
        while (i < text.Length)
        {
            int tagStartIndex = text.IndexOf("<speed=", i);
            
            if (tagStartIndex == -1 || tagStartIndex > i)
            {
                // Add characters before the next tag (or all remaining if no more tags)
                int end = (tagStartIndex == -1) ? text.Length : tagStartIndex;
                for (int j = i; j < end; j++)
                {
                    cleanText.Append(text[j]);
                    speedsList.Add(currentSpeed);
                }
                
                if (tagStartIndex == -1)
                    break;
                    
                i = tagStartIndex;
            }
            
            // Process the speed tag
            int tagEndIndex = text.IndexOf('>', i);
            if (tagEndIndex == -1)
            {
                Debug.LogWarning("Malformed speed tag found in text. Using base typing speed for remaining characters.");
                for (int j = i; j < text.Length; j++)
                {
                    cleanText.Append(text[j]);
                    speedsList.Add(_baseTypingSpeed);
                }
                break;
            }
            
            // Extract the speed value
            string speedValue = text.Substring(i + 7, tagEndIndex - i - 7);
            if (float.TryParse(speedValue, out float speed))
            {
                currentSpeed = speed;
            }
            else
            {
                Debug.LogWarning($"Invalid speed value '{speedValue}' found in text. Using base typing speed.");
            }
            
            // Skip past this tag
            i = tagEndIndex + 1;
        }
        
        typingSpeeds = speedsList.ToArray();
        return cleanText.ToString();
    }

    /// <summary>
    /// Initiates the process of writing the specified text.
    /// </summary>
    /// <param name="text">The text to be written.</param>
    public void StartWriting(string text) => StartWritingAsync(text).Forget();
    /// <summary>
    /// Initiates the process of writing the specified text.
    /// </summary>
    /// <param name="text">The text to be written.</param>
    /// <returns>An awaitable UniTask signalling when writing is completed or cancelled.</returns>
    public async UniTask StartWritingAsync(string text)
    {
        if(_textComponent == null)
        {
            Debug.LogError($"TMP_Text component not found for {gameObject.name}'s {this.name}.");
            return;
        }

        if (_cancelToken != null)
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
        }
        _cancelToken = new CancellationTokenSource();

        string cleanText = PreprocessText(text, out float[] typingSpeeds);
        _textComponent.text = cleanText;

        for (int i = 0; i < cleanText.Length; i++)
        {
            _textComponent.maxVisibleCharacters = i + 1; // Show one more character each iteration
            if (_typingSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_typingSound);
            }
            OnCharacterType?.Invoke();

            int delay = Mathf.RoundToInt(typingSpeeds[i] * 1000 * typingSpeedMultiplier);
            await UniTask.Delay(delay, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
            if(_cancelToken.Token.IsCancellationRequested)
            {
                return;
            }
        }

        OnTextComplete?.Invoke();
    }

    /// <summary>
    /// Cancels the current writing operation, if one is in progress. Will NOT trigger OnTextComplete event.
    /// </summary>
    public void CancelWriting()
    {
        if (_cancelToken != null)
        {
            _cancelToken.Cancel();
            _cancelToken.Dispose();
            _cancelToken = null;
        }
    }

    /// <summary>
    /// Skips to the end of the text, displaying all characters immediately and invoking OnTextComplete event.
    /// </summary>
    public void SkipToEnd()
    {
        CancelWriting();
        _textComponent.maxVisibleCharacters = _textComponent.text.Length; // Show all characters
        OnTextComplete?.Invoke();
    }
}
