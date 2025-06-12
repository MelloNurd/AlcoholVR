using UnityEngine;

public class MinigameCollider : MonoBehaviour
{
    private Minigame _minigame;
    private ScoringCollider _scoringData;

    public void Initialize(Minigame minigame, ScoringCollider scoringData)
    {
        _minigame = minigame;
        _scoringData = scoringData;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision enter with {collision.gameObject.name} on layer {collision.gameObject.layer}");
        if ((_minigame.LayerMask & (1 << collision.gameObject.layer)) == 0) return;

        _minigame.AddScore(_scoringData.OnEnterScoreValue);

        if (_scoringData.OverrideScoreCooldown) _minigame.StartCooldown(_scoringData.ScoringCooldown);
        else _minigame.StartCooldown();
    }

    private void OnCollisionExit(Collision collision)
    {
        Debug.Log($"Collision exit with {collision.gameObject.name} on layer {collision.gameObject.layer}");
        if ((_minigame.LayerMask & (1 << collision.gameObject.layer)) == 0) return;

        _minigame.AddScore(_scoringData.OnLeaveScoreValue);

        if (_scoringData.OverrideScoreCooldown) _minigame.StartCooldown(_scoringData.ScoringCooldown);
        else _minigame.StartCooldown();
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log($"Trigger enter with {collision.gameObject.name} on layer {collision.gameObject.layer}");
        if ((_minigame.LayerMask & (1 << collision.gameObject.layer)) == 0) return;

        _minigame.AddScore(_scoringData.OnEnterScoreValue);

        if (_scoringData.OverrideScoreCooldown) _minigame.StartCooldown(_scoringData.ScoringCooldown);
        else _minigame.StartCooldown();
    }

    private void OnTriggerExit(Collider collision)
    {
        Debug.Log($"Trigger exit with {collision.gameObject.name} on layer {collision.gameObject.layer}");
        if ((_minigame.LayerMask & (1 << collision.gameObject.layer)) == 0) return;

        _minigame.AddScore(_scoringData.OnLeaveScoreValue);

        if (_scoringData.OverrideScoreCooldown) _minigame.StartCooldown(_scoringData.ScoringCooldown);
        else _minigame.StartCooldown();
    }
}
