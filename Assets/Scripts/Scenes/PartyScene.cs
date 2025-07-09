using UnityEngine;

public class PartyScene : MonoBehaviour
{
    [Header("NPC References")]
    [SerializeField] private SequencedNPC _introNPC;
    [SerializeField] private SequencedNPC _drunkDrivingFriendNPC;
    [SerializeField] private SequencedNPC _soberFriendNPC;

    [Header("Dialogue References")]
    [SerializeField] private DialogueTree _broughtAlcohol;
    [SerializeField] private DialogueTree _broughtSnacks;
    [SerializeField] private DialogueTree _broughtNothing;

    [SerializeField] private DialogueTree _couchDrunk;
    [SerializeField] private DialogueTree _couchSober;

    public bool IsOnSecondFloor => Player.Instance.transform.position.y > 3f;

    private bool _hasEnteredHouse = false;

    private void OnTriggerEnter(Collider other)
    {
        if(!_hasEnteredHouse && other.gameObject.layer == LayerMask.NameToLayer("PlayerBody"))
        {
            _hasEnteredHouse = true;
            Debug.Log("Player has entered the house, starting intro sequence.");
            _introNPC.StartNextSequence();
        }
    }
}
