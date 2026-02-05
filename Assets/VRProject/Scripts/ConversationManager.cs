using System.Collections;
using UnityEngine;

public class NPCConversation : MonoBehaviour
{
    public AudioSource npcA;
    public AudioSource npcB;

    public AudioClip[] npcALines;
    public AudioClip[] npcBLines;

    public float pauseBetweenLines = 0.25f;

    void Start()
    {
        StartCoroutine(ConversationRoutine());
    }

    IEnumerator ConversationRoutine()
    {
        int max = Mathf.Max(npcALines.Length, npcBLines.Length);

        for (int i = 0; i < max; i++)
        {
            if (i < npcALines.Length)
            {
                npcA.clip = npcALines[i];
                npcA.Play();
                Debug.Log($"Playing NPC A clip {i}, length: {npcALines[i].length}");
                yield return new WaitForSecondsRealtime(npcALines[i].length + pauseBetweenLines);
            }

            if (i < npcBLines.Length)
            {
                npcB.clip = npcBLines[i];
                npcB.Play();
                Debug.Log($"Playing NPC B clip {i}, length: {npcBLines[i].length}");
                yield return new WaitForSecondsRealtime(npcBLines[i].length + pauseBetweenLines);
            }
        }
    }
}
