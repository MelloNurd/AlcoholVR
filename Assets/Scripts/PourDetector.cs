using UnityEngine;

public class PourDetector : MonoBehaviour
{
    public Transform pourPoint; // tip of the bottle
    public float pourAngleThreshold = 45f; // degrees
    public GameObject pourEffectPrefab;

    bool isPouring = false;
    private Stream currentStream;

    private void Update()
    {
        bool pourCheck = CalculatePourAngle() < pourAngleThreshold;

        if(isPouring != pourCheck)
        {
            isPouring = pourCheck;

            if (isPouring)
            {
                StartPour();
            }
            else
            {
                StopPour();
            }
        }
    }

    private void StartPour()
    {
        Debug.Log("Started pouring");
        currentStream = CreateStream();
        currentStream.Begin();
    }

    private void StopPour()
    {
        Debug.Log("Stopped pouring");
    }

    private float CalculatePourAngle()
    {
        return transform.up.y * Mathf.Rad2Deg;
    }

    private Stream CreateStream()
    {
        GameObject streamObject = Instantiate(pourEffectPrefab, pourPoint.position, Quaternion.identity, transform);
        return streamObject.GetComponent<Stream>();
    }
}
