using PrimeTween;
using TMPro;
using UnityEngine;

public class TutorialText : MonoBehaviour
{
    public static TutorialText Instance { get; private set; }

    private TMP_Text _text;
    private Vector3 _textScale;

    [Header("Follow Settings")]
    [SerializeField] private bool _followPlayer = true;
    [SerializeField, Tooltip("Distance in front of the camera to place the text")] private float distanceFromPlayer = 0.6f;
    [SerializeField, Tooltip("Vertical offset relative to the camera position")] private float verticalOffset = -0.2f;
    [SerializeField, Tooltip("How fast the in-front direction pans/smooths")] private float panSpeed = 2f;
    [SerializeField, Tooltip("Only rotate around Y so the text stays upright")] private bool onlyYaw = true;

    private Vector3 _smoothedDirection;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        _text = GetComponent<TMP_Text>();
        if (_text == null)
        {
            _text = GetComponentInChildren<TMP_Text>();
        }
        if (_text != null)
        {
            _textScale = _text.transform.localScale;
        }

        // Initialize smoothed direction
        var initialDir = Vector3.forward;
        if (Player.Instance != null)
        {
            initialDir = Player.Instance.Forward.WithY(0).normalized;
        }
        _smoothedDirection = initialDir;

        HideText();
    }

    private void Update()
    {
        if (!_followPlayer || Player.Instance == null || Player.Instance.Camera == null) return;

        // Target forward from player on XZ
        Vector3 targetForward = Player.Instance.Forward.WithY(0).normalized;

        // Smooth only the direction used for positioning (keeps position relative to player instant)
        _smoothedDirection = Vector3.Slerp(_smoothedDirection, targetForward, Time.deltaTime * panSpeed);
        if (_smoothedDirection.sqrMagnitude < 1e-6f)
        {
            _smoothedDirection = targetForward;
        }

        // Instant position relative to player, smoothed relative to direction
        Vector3 targetPos = Player.Instance.CamPosition + _smoothedDirection * distanceFromPlayer;
        targetPos.y += verticalOffset;
        transform.position = targetPos;

        // Instant rotation (no smoothing)
        Vector3 toCam = Player.Instance.CamPosition - transform.position;
        if (onlyYaw) toCam = toCam.WithY(0f);
        if (toCam.sqrMagnitude > 1e-6f)
        {
            transform.rotation = Quaternion.LookRotation(-toCam.normalized, Vector3.up);
        }
    }

    public string CurrentText => _text != null ? _text.text : string.Empty;

    public void ShowText(string message)
    {
        if (_text == null) return;
        _text.text = message;

        Debug.Log($"Displaying tutorial text: {message}");

        Tween.StopAll(_text.transform);

        _text.transform.localScale = Vector3.zero;
        Tween.Scale(_text.transform, _textScale, 0.3f, ease: Ease.OutBack);
    }

    public void HideText()
    {
        if (_text == null) return;

        Tween.StopAll(_text.transform);

        _text.transform.localScale = _textScale;
        Tween.Scale(_text.transform, Vector3.zero, 0.3f, ease: Ease.InBack)
            .OnComplete(() => _text.text = string.Empty);
    }
}