using UnityEngine;

public class PropBubble : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PropPhysics prop = other.GetComponent<PropPhysics>();
        if (prop != null)
        {
            prop.RefreshBubble();
        }
    }
}
