using UnityEngine;
using System.Collections;

public class AnimeationEvents : MonoBehaviour
{
    [SerializeField] private AudioClip[] rageSlamSounds;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] _footstepIndoorSounds;
    [SerializeField] private AudioClip[] _footstepOutdoorSounds;

    [Header("Eye Animation")]
    [SerializeField] private float _eyeCloseDuration = 0.3f;
    [SerializeField] private float _eyeOpenDuration = 0.3f;
    [SerializeField] private float _meshInitializationDelay = 0.5f;

    int slamIndex = -1;
    int _groundLayerMask;

    SkinnedMeshRenderer _skinnedMeshRenderer;
    Coroutine _eyeAnimationCoroutine;

    private void Start()
    {
        _groundLayerMask = LayerMask.GetMask("Ground");
        StartCoroutine(InitializeMeshDelayed());
    }

    private IEnumerator InitializeMeshDelayed()
    {
        yield return new WaitForSeconds(_meshInitializationDelay);

        //Get the skinned mesh renderer from either FemalePhoeneticHead/CombinedSkinnedMesh or MalePhoeneticHead/CombinedSkinnedMesh depending on which child exists
        GameObject combinedmesh = transform.Find("FemalePhoeneticHead/CombinedSkinnedMesh")?.gameObject ?? transform.Find("MalePhoeneticHead/CombinedSkinnedMesh")?.gameObject;
        _skinnedMeshRenderer = combinedmesh.GetComponent<SkinnedMeshRenderer>();
    }

    private void CloseEyes()
    {
        // Smoothly close the eyes over _eyeCloseDuration seconds
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for closing eyes.", gameObject);
            return;
        }

        if (_eyeAnimationCoroutine != null)
        {
            StopCoroutine(_eyeAnimationCoroutine);
        }

        _eyeAnimationCoroutine = StartCoroutine(AnimateEyesClosed());
    }

    private void OpenEyes()
    {
        // Smoothly open the eyes over _eyeOpenDuration seconds
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for opening eyes.", gameObject);
            return;
        }

        if (_eyeAnimationCoroutine != null)
        {
            StopCoroutine(_eyeAnimationCoroutine);
        }

        _eyeAnimationCoroutine = StartCoroutine(AnimateEyesOpen());
    }

    private IEnumerator AnimateEyesClosed()
    {
        int rightEyeIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Eyes_UpperClosed_R");
        int leftEyeIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Eyes_UpperClosed_L");

        float elapsed = 0f;

        while (elapsed < _eyeCloseDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / _eyeCloseDuration);

            _skinnedMeshRenderer.SetBlendShapeWeight(rightEyeIndex, progress * 100f);
            _skinnedMeshRenderer.SetBlendShapeWeight(leftEyeIndex, progress * 100f);

            yield return null;
        }

        _skinnedMeshRenderer.SetBlendShapeWeight(rightEyeIndex, 100f);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftEyeIndex, 100f);
    }

    private IEnumerator AnimateEyesOpen()
    {
        int rightEyeIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Eyes_UpperClosed_R");
        int leftEyeIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Eyes_UpperClosed_L");

        float elapsed = 0f;

        while (elapsed < _eyeOpenDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / _eyeOpenDuration);

            _skinnedMeshRenderer.SetBlendShapeWeight(rightEyeIndex, (1f - progress) * 100f);
            _skinnedMeshRenderer.SetBlendShapeWeight(leftEyeIndex, (1f - progress) * 100f);

            yield return null;
        }

        _skinnedMeshRenderer.SetBlendShapeWeight(rightEyeIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftEyeIndex, 0f);
    }

    private int GetRageSlamSounds()
    {
        // Rage sounds are ordered in order of intensity, so we always want to play two in a row
        if (rageSlamSounds.Length == 0)
        {
            Debug.LogError("[PartyScene] Rage sounds have zero clips assigned.");
            return -1;
        }

        return Random.Range(0, rageSlamSounds.Length - 1);
    }

    public void PlayFirstSlam()
    {
        slamIndex = GetRageSlamSounds();
        if (slamIndex == -1) return;

        SoundManager.PlaySoundAtPoint(rageSlamSounds[slamIndex], transform.position);
    }

    public void PlaySecondSlam()
    {
        if (slamIndex == -1) return;
        if (slamIndex + 1 >= rageSlamSounds.Length)
        {
            SoundManager.PlaySoundAtPoint(rageSlamSounds[slamIndex], transform.position);
            return;
        }

        SoundManager.PlaySoundAtPoint(rageSlamSounds[slamIndex + 1], transform.position);
    }

    public void PlayFootstepSound()
    {
        if (_footstepOutdoorSounds.Length == 0 || _footstepIndoorSounds.Length == 0)
        {
            Debug.LogError("[AnimeationEvents] Footstep sounds are not assigned.", gameObject);
            return;
        }

        // Raycast downwards, if it hits a terrain play the dirt sound, otherwise play the hard sound
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f, _groundLayerMask) && hit.collider.gameObject.CompareTag("Terrain"))
        {
            SoundManager.PlaySoundAtPoint(_footstepOutdoorSounds.GetRandom(), transform.position, volume: 0.5f, pitch: Random.Range(0.85f, 1.15f));

            if(transform.parent.parent.name == "Drunk Driving NPC")
                Debug.Log("Playing outdoor footstep sound.", gameObject);
        }
        else
        {
            SoundManager.PlaySoundAtPoint(_footstepIndoorSounds.GetRandom(), transform.position, volume: 0.5f, pitch: Random.Range(0.85f, 1.15f));

            if(transform.parent.parent.name == "Drunk Driving NPC")
                Debug.Log("Playing indoor footstep sound.", gameObject);
        }
    }
}
