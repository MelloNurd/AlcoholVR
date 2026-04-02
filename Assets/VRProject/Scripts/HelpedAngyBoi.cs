using UnityEngine;

public class HelpedAngyBoi : MonoBehaviour
{
    AudioSource audiosource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
      audiosource = GetComponent<AudioSource>();
      if(!GlobalStats.helpedRagingDrunk)
        {
            audiosource.enabled = true;
        }
    }
}
