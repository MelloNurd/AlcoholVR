using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Foot
{
    Left,
    Right
}

public class AnimeationEvents : MonoBehaviour
{
    [SerializeField] private AudioClip[] rageSlamSounds;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] _footstepIndoorSounds;
    [SerializeField] private AudioClip[] _footstepOutdoorSounds;

    [Header("Eye Animation")]
    [SerializeField] private float smoothDuration = 0.3f;
    [SerializeField] private float _meshInitializationDelay = 0.5f;
    [SerializeField] private Transform _leftFoot;
    [SerializeField] private Transform _rightFoot;

    int slamIndex = -1;
    int _groundLayerMask;

    SkinnedMeshRenderer _skinnedMeshRenderer;
    Coroutine _eyeAnimationCoroutine;
    bool _isAnimationPlaying = false;
    Queue<IEnumerator> _animationQueue = new Queue<IEnumerator>();
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    private void Start()
    {
        _groundLayerMask = LayerMask.GetMask("Ground");
        StartCoroutine(InitializeMeshDelayed());

        //Pick a random time 1-6 seconds to trigger animator.SetTrigger("StartBlink")
        float randomTime = Random.Range(1f, 10f);
        StartCoroutine(StartBlink(randomTime));
    }

    private IEnumerator InitializeMeshDelayed()
    {
        yield return new WaitForSeconds(_meshInitializationDelay);

        //Get the skinned mesh renderer from either FemalePhoeneticHead/CombinedSkinnedMesh or MalePhoeneticHead/CombinedSkinnedMesh depending on which child exists
        GameObject combinedmesh = transform.Find("FemalePhoeneticHead/CombinedSkinnedMesh")?.gameObject ?? transform.Find("MalePhoeneticHead/CombinedSkinnedMesh")?.gameObject;
        _skinnedMeshRenderer = combinedmesh.GetComponent<SkinnedMeshRenderer>();
    }

    private void QueueAnimation(IEnumerator animationCoroutine)
    {
        _animationQueue.Enqueue(animationCoroutine);
        if (!_isAnimationPlaying)
        {
            _eyeAnimationCoroutine = StartCoroutine(ProcessAnimationQueue());
        }
    }

    private IEnumerator ProcessAnimationQueue()
    {
        while (_animationQueue.Count > 0)
        {
            _isAnimationPlaying = true;
            IEnumerator currentAnimation = _animationQueue.Dequeue();
            yield return StartCoroutine(currentAnimation);
        }
        _isAnimationPlaying = false;
    }

    private System.Collections.IEnumerator StartBlink(float delay)
    {
        yield return new WaitForSeconds(delay);
        _animator.SetTrigger("StartBlink");
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

    public void PlayFootstepSound(Foot foot)
    {
        if (_footstepOutdoorSounds.Length == 0 || _footstepIndoorSounds.Length == 0)
        {
            Debug.LogError("[AnimeationEvents] Footstep sounds are not assigned.", gameObject);
            return;
        }

        Transform footTransform = (foot == Foot.Left) ? _leftFoot : _rightFoot;

        // Raycast downwards, if it hits a terrain play the dirt sound, otherwise play the hard sound
        if (Physics.Raycast(footTransform.position, Vector3.down, out RaycastHit hit, 1f, _groundLayerMask) && hit.collider.gameObject.CompareTag("Terrain"))
        {
            SoundManager.PlaySoundAtPoint(_footstepOutdoorSounds.GetRandom(), footTransform.position, volume: 0.5f);
        }
        else
        {
            SoundManager.PlaySoundAtPoint(_footstepIndoorSounds.GetRandom(), footTransform.position, volume: 0.4f);
        }
    }

    private void CloseEyes(float targetWeight = 100f)
    {
        // Smoothly close the eyes over smoothDuration seconds
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for closing eyes.", gameObject);
            return;
        }

        QueueAnimation(AnimateEyesClosed(targetWeight));
    }

    private void ResetEyebrows()
    {
        _animator.ResetTrigger("StartResetEyebrows");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for resetting eyebrows.", gameObject);
            return;
        }

        QueueAnimation(AnimateEyebrows(0f, 0f, 0f, 0f));
    }

    private void AngryEyebrows(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartEyebrowsMad");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for angry eyebrows.", gameObject);
            return;
        }
        QueueAnimation(AnimateEyebrows(targetWeight, 0f, 0f, 0f));
    }

    private void SadEyebrows(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartEyebrowsSad");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for sad eyebrows.", gameObject);
            return;
        }
        QueueAnimation(AnimateEyebrows(0f, targetWeight, 0f, 0f));
    }

    private void RaisedEyebrows(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartEyebrowsRaised");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for raised eyebrows.", gameObject);
            return;
        }
        QueueAnimation(AnimateEyebrows(0f, 0f, targetWeight, 0f));
    }

    private void LoweredEyebrows(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartEyebrowsLowered");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for lowered eyebrows.", gameObject);
            return;
        }
        QueueAnimation(AnimateEyebrows(0f, 0f, 0f, targetWeight));
    }

    private IEnumerator AnimateEyebrows(float angryTarget, float sadTarget, float raisedTarget, float loweredTarget)
    {
        int leftBrowAngryIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Angry_L");
        int rightBrowAngryIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Angry_R");
        int leftBrowSadIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brows_Sad_L");
        int rightBrowSadIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brows_Sad_R");
        int leftBrowRaisedIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brows_Raised_L");
        int rightBrowRaisedIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brows_Raised_R");
        int leftBrowLoweredIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brows_Lowered_L");
        int rightBrowLoweredIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brows_Lowered_R");

        float leftBrowAngryStart = _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowAngryIndex);
        float rightBrowAngryStart = _skinnedMeshRenderer.GetBlendShapeWeight(rightBrowAngryIndex);
        float leftBrowSadStart = _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowSadIndex);
        float rightBrowSadStart = _skinnedMeshRenderer.GetBlendShapeWeight(rightBrowSadIndex);
        float leftBrowRaisedStart = _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowRaisedIndex);
        float rightBrowRaisedStart = _skinnedMeshRenderer.GetBlendShapeWeight(rightBrowRaisedIndex);
        float leftBrowLoweredStart = _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowLoweredIndex);
        float rightBrowLoweredStart = _skinnedMeshRenderer.GetBlendShapeWeight(rightBrowLoweredIndex);

        float elapsed = 0f;

        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);

            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowAngryIndex, Mathf.Lerp(leftBrowAngryStart, angryTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowAngryIndex, Mathf.Lerp(rightBrowAngryStart, angryTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowSadIndex, Mathf.Lerp(leftBrowSadStart, sadTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowSadIndex, Mathf.Lerp(rightBrowSadStart, sadTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowRaisedIndex, Mathf.Lerp(leftBrowRaisedStart, raisedTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowRaisedIndex, Mathf.Lerp(rightBrowRaisedStart, raisedTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowLoweredIndex, Mathf.Lerp(leftBrowLoweredStart, loweredTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowLoweredIndex, Mathf.Lerp(rightBrowLoweredStart, loweredTarget, progress));

            yield return null;
        }

        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowAngryIndex, angryTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowAngryIndex, angryTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowSadIndex, sadTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowSadIndex, sadTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowRaisedIndex, raisedTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowRaisedIndex, raisedTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowLoweredIndex, loweredTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowLoweredIndex, loweredTarget);
    }

    private IEnumerator AnimateEyesClosed(float targetWeight)
    {
        int rightEyeIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Eyes_UpperClosed_R");
        int leftEyeIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Eyes_UpperClosed_L");

        float startWeight = _skinnedMeshRenderer.GetBlendShapeWeight(rightEyeIndex);
        float elapsed = 0f;

        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);

            float currentWeight = Mathf.Lerp(startWeight, targetWeight, progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(rightEyeIndex, currentWeight);
            _skinnedMeshRenderer.SetBlendShapeWeight(leftEyeIndex, currentWeight);

            yield return null;
        }

        _skinnedMeshRenderer.SetBlendShapeWeight(rightEyeIndex, targetWeight);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftEyeIndex, targetWeight);
    }

    private void ResetMouth()
    {
        _animator.ResetTrigger("StartResetMouth");
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for resetting mouth.", gameObject);
            return;
        }
        QueueAnimation(AnimateMouth(0f, 0f));
    }

    private void SmileMouth(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartSmileMouth");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for smiling mouth.", gameObject);
            return;
        }
        QueueAnimation(AnimateMouth(targetWeight, 0f));
    }

    private void FrownMouth(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartFrownMouth");
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for frowning mouth.", gameObject);
            return;
        }
        QueueAnimation(AnimateMouth(0f, targetWeight));
    }

    private IEnumerator AnimateMouth(float smileTarget, float frownTarget)
    {
        int smileMouthRIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Happy_R");
        int smileMouthLIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Happy_L");
        int frownMouthRIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Sad_R");
        int frownMouthLIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Sad_L");

        float startSmileWeightR = _skinnedMeshRenderer.GetBlendShapeWeight(smileMouthRIndex);
        float startSmileWeightL = _skinnedMeshRenderer.GetBlendShapeWeight(smileMouthLIndex);
        float startFrownWeightR = _skinnedMeshRenderer.GetBlendShapeWeight(frownMouthRIndex);
        float startFrownWeightL = _skinnedMeshRenderer.GetBlendShapeWeight(frownMouthLIndex);

        float elapsed = 0f;
        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);

            _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthRIndex, Mathf.Lerp(startSmileWeightR, smileTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthLIndex, Mathf.Lerp(startSmileWeightL, smileTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthRIndex, Mathf.Lerp(startFrownWeightR, frownTarget, progress));
            _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthLIndex, Mathf.Lerp(startFrownWeightL, frownTarget, progress));

            yield return null;
        }

        _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthRIndex, smileTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthLIndex, smileTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthRIndex, frownTarget);
        _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthLIndex, frownTarget);
    }
}