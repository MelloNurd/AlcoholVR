using UnityEngine;

public class DemoScene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Phone.Instance.ShowNotification("Markus", "Party at my house, bring snacks and beer if you can");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
