using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;

public class Credits : MonoBehaviour
{
    [SerializeField] private float _showDuration = 4f;
    [SerializeField] private float _fadeDuration = 1f;

    private CanvasGroup[] _creditSections;

    private void Awake()
    {
        _creditSections = GetComponentsInChildren<CanvasGroup>(true);

        // Ensure they're sorted in hierarchy order
        System.Array.Sort(_creditSections, (a, b) =>
            a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        foreach(var section in _creditSections)
        {
            section.alpha = 0f;
        }
    }

    private async void Start()
    {
        if (_creditSections.Length == 0) return;

        await PlayCredits();

        ContinuousMusic.Instance?.StopMusic();
        Destroy(SettingsManager.Instance.gameObject);
        SettingsManager.Instance = null;
        Player.Instance.loading.TransitionSceneByName("MainMenu");
    }

    private async UniTask PlayCredits()
    {
        await UniTask.Delay(1000); // Initial delay

        for (int i = 0; i < _creditSections.Length; i++)
        {
            float showDuration = _showDuration;
            Debug.Log($"Trying to parse: {_creditSections[i].gameObject.name.GetLast(2)} for {_creditSections[i].gameObject.name}");
            if (float.TryParse(_creditSections[i].gameObject.name.GetLast(2), out float parsedDuration))
            {
                Debug.Log($"Parsed custom duration {parsedDuration} from {_creditSections[i].gameObject.name}");
                showDuration = parsedDuration;
            }
            else
            {
                Debug.Log($"Using default duration {_showDuration} for {_creditSections[i].gameObject.name}");
            }
            await ShowGroup(_creditSections[i]);
            await UniTask.Delay(showDuration.ToMS());
            await HideGroup(_creditSections[i]);
        }

        await UniTask.Delay(1000);
    }

    private async UniTask ShowGroup(CanvasGroup group)
    {
        await Tween.Alpha(group, 1f, _fadeDuration);
    }

    private async UniTask HideGroup(CanvasGroup group)
    {
        await Tween.Alpha(group, 0f, _fadeDuration);
    }
}
