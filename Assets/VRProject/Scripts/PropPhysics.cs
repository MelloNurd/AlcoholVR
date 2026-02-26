using UnityEngine;

public class PropPhysics : MonoBehaviour
{
    Rigidbody rb;

    public float bubbleTimeout = 5f; // how long after leaving bubble before sleeping

    float bubbleTimer = 0f;
    bool inBubble = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        SetIdleState();
    }

    void Update()
    {
        if (!inBubble) return;

        bubbleTimer -= Time.deltaTime;

        if (bubbleTimer <= 0f)
        {
            ExitBubble();
        }
    }

    public void RefreshBubble()
    {
        bubbleTimer = bubbleTimeout;

        if (!inBubble)
        {
            EnterBubble();
        }
    }

    void EnterBubble()
    {
        inBubble = true;
        Debug.Log("Entered bubble: " + gameObject.name);
        SetActiveState();
    }

    void ExitBubble()
    {
        inBubble = false;
        Debug.Log("Exited bubble: " + gameObject.name);
        SetIdleState();
        
    }

    void SetIdleState()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.Sleep();
    }

    void SetActiveState()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.WakeUp();
    }
}