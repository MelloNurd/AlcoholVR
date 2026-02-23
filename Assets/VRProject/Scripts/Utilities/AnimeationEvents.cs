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

        QueueAnimation(AnimateResetEyebrows());
    }

    private void AngryEyebrows(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartEyebrowsMad");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for angry eyebrows.", gameObject);
            return;
        }
        QueueAnimation(AnimateAngryEyebrows(targetWeight));
    }

    private void SadEyebrows(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartEyebrowsSad");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for sad eyebrows.", gameObject);
            return;
        }
        QueueAnimation(AnimateSadEyebrows(targetWeight));
    }

    private void RaisedEyebrows(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartEyebrowsRaised");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for raised eyebrows.", gameObject);
            return;
        }
        QueueAnimation(AnimateRaisedEyebrows(targetWeight));
    }

    private void LoweredEyebrows(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartEyebrowsLowered");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for lowered eyebrows.", gameObject);
            return;
        }
        QueueAnimation(AnimateLoweredEyebrows(targetWeight));
    }

    private void ResetMouth()
    {
        _animator.ResetTrigger("StartResetMouth");
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for resetting mouth.", gameObject);
            return;
        }
        // Reset both smile and frown mouths to 0
        QueueAnimation(AnimateResetMouth());
    }

    private void SmileMouth(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartSmileMouth");

        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for smiling mouth.", gameObject);
            return;
        }
        // Smile mouth is a single blend shape, so we can directly set it without queuing an animation
        QueueAnimation(AnimateSmileMouth(targetWeight));
    }

    private void FrownMouth(float targetWeight = 100f)
    {
        _animator.ResetTrigger("StartFrownMouth");
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("[AnimeationEvents] SkinnedMeshRenderer not found for frowning mouth.", gameObject);
            return;
        }
        // Frown mouth is a single blend shape, so we can directly set it without queuing an animation
        QueueAnimation(AnimateFrownMouth(targetWeight));
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

    private IEnumerator AnimateResetEyebrows()
    {
        int leftBrowAngryIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Angry_L");
        int rightBrowAngryIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Angry_R");
        int leftBrowSadIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Sad_L");
        int rightBrowSadIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Sad_R");
        int leftBrowRaisedIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Raised_L");
        int rightBrowRaisedIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Raised_R");
        int leftBrowLoweredIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Lowered_L");
        int rightBrowLoweredIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Lowered_R");

        float elapsed = 0f;

        while(elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowAngryIndex, (1f - progress) * _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowAngryIndex));
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowAngryIndex, (1f - progress) * _skinnedMeshRenderer.GetBlendShapeWeight(rightBrowAngryIndex));
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowSadIndex, (1f - progress) * _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowSadIndex));
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowSadIndex, (1f - progress) * _skinnedMeshRenderer.GetBlendShapeWeight(rightBrowSadIndex));
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowRaisedIndex, (1f - progress) * _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowRaisedIndex));
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowRaisedIndex, (1f - progress) * _skinnedMeshRenderer.GetBlendShapeWeight(rightBrowRaisedIndex));
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowLoweredIndex, (1f - progress) * _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowLoweredIndex));
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowLoweredIndex, (1f - progress) * _skinnedMeshRenderer.GetBlendShapeWeight(rightBrowLoweredIndex));
            yield return null;
        }

        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowAngryIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowAngryIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowSadIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowSadIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowRaisedIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowRaisedIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowLoweredIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowLoweredIndex, 0f);
    }

    private IEnumerator AnimateAngryEyebrows(float targetWeight)
    {
        int leftBrowAngryIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Angry_L");
        int rightBrowAngryIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Angry_R");
        
        float startWeight = _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowAngryIndex);
        float elapsed = 0f;
        
        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);
            
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowAngryIndex, currentWeight);
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowAngryIndex, currentWeight);
            yield return null;
        }
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowAngryIndex, targetWeight);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowAngryIndex, targetWeight);
    }

    private IEnumerator AnimateSadEyebrows(float targetWeight)
    {
        int leftBrowSadIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Sad_L");
        int rightBrowSadIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Sad_R");
        
        float startWeight = _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowSadIndex);
        float elapsed = 0f;
        
        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);
            
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowSadIndex, currentWeight);
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowSadIndex, currentWeight);
            yield return null;
        }
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowSadIndex, targetWeight);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowSadIndex, targetWeight);
    }

    private IEnumerator AnimateRaisedEyebrows(float targetWeight)
    {
        int leftBrowRaisedIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Raised_L");
        int rightBrowRaisedIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Raised_R");
        
        float startWeight = _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowRaisedIndex);
        float elapsed = 0f;
        
        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);
            
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowRaisedIndex, currentWeight);
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowRaisedIndex, currentWeight);
            yield return null;
        }
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowRaisedIndex, targetWeight);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowRaisedIndex, targetWeight);
    }

    private IEnumerator AnimateLoweredEyebrows(float targetWeight)
    {
        int leftBrowLoweredIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Lowered_L");
        int rightBrowLoweredIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Brow_Lowered_R");
        
        float startWeight = _skinnedMeshRenderer.GetBlendShapeWeight(leftBrowLoweredIndex);
        float elapsed = 0f;
        
        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);
            
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowLoweredIndex, currentWeight);
            _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowLoweredIndex, currentWeight);
            yield return null;
        }
        _skinnedMeshRenderer.SetBlendShapeWeight(leftBrowLoweredIndex, targetWeight);
        _skinnedMeshRenderer.SetBlendShapeWeight(rightBrowLoweredIndex, targetWeight);
    }

    private IEnumerator AnimateResetMouth()
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
            float currentSmileWeightR = Mathf.Lerp(startSmileWeightR, 0f, progress);
            float currentSmileWeightL = Mathf.Lerp(startSmileWeightL, 0f, progress);
            float currentFrownWeightR = Mathf.Lerp(startFrownWeightR, 0f, progress);
            float currentFrownWeightL = Mathf.Lerp(startFrownWeightL, 0f, progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthRIndex, currentSmileWeightR);
            _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthLIndex, currentSmileWeightL);
            _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthRIndex, currentFrownWeightR);
            _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthLIndex, currentFrownWeightL);
            yield return null;
        }
        _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthRIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthLIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthRIndex, 0f);
        _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthLIndex, 0f);
    }

    private IEnumerator AnimateSmileMouth(float targetWeight)
    {
        int smileMouthRIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Happy_R");
        int smileMouthLIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Happy_L");
        float startWeight = _skinnedMeshRenderer.GetBlendShapeWeight(smileMouthRIndex);
        float elapsed = 0f;
        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthRIndex, currentWeight);
            _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthLIndex, currentWeight);
            yield return null;
        }
        _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthRIndex, targetWeight);
        _skinnedMeshRenderer.SetBlendShapeWeight(smileMouthLIndex, targetWeight);
    }

    private IEnumerator AnimateFrownMouth(float targetWeight)
    {
        int frownMouthRIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Sad_R");
        int frownMouthLIndex = _skinnedMeshRenderer.sharedMesh.GetBlendShapeIndex("Expression_Mouth_Sad_L");
        float startWeight = _skinnedMeshRenderer.GetBlendShapeWeight(frownMouthRIndex);
        float elapsed = 0f;
        while (elapsed < smoothDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / smoothDuration);
            float currentWeight = Mathf.Lerp(startWeight, targetWeight, progress);
            _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthRIndex, currentWeight);
            _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthLIndex, currentWeight);
            yield return null;
        }
        _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthRIndex, targetWeight);
        _skinnedMeshRenderer.SetBlendShapeWeight(frownMouthLIndex, targetWeight);
    }

}


