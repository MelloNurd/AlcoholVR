using System.Collections;
using UnityEngine;

public class HeadHeightAdjuster : MonoBehaviour
{
    [SerializeField] GameObject objectToMatch;
    public float heightOffset = 0.0f; // Optional offset to adjust the height further
    public float minHeight = 0.0f; // Minimum height to prevent going below this value
    public float maxHeight = 10.0f; // Maximum height to prevent going above this value
    public float delay = 1.0f; // Delay before setting the position

    public void Start()
    {
        StartCoroutine(CoroutineSetPosition());
    }

    public void SetPosition()
    {
         StartCoroutine(CoroutineSetPosition());
    }

    IEnumerator CoroutineSetPosition()
    {
        yield return new WaitForSeconds(delay); // Wait for a short duration before setting the position
        Debug.Log("running event SetPosition");
        if (objectToMatch.transform.position.y < minHeight)
        {
            gameObject.transform.position = new Vector3(
                gameObject.transform.position.x,
                minHeight,
                gameObject.transform.position.z
            );
            Debug.Log("Adjusted to height: " + minHeight);
        }
        else if (objectToMatch.transform.position.y > maxHeight)
        {
            gameObject.transform.position = new Vector3(
                gameObject.transform.position.x,
                maxHeight,
                gameObject.transform.position.z
            );
            Debug.Log("Adjusted to height: " + maxHeight);
        }
        else
        {
            gameObject.transform.position = new Vector3(
                gameObject.transform.position.x,
                objectToMatch.transform.position.y + heightOffset,
                gameObject.transform.position.z
            );
            Debug.Log("Adjusted to height: " + (objectToMatch.transform.position.y + heightOffset));
        }
    }
}
