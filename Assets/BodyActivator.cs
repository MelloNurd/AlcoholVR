using UnityEngine;

public class BodyActivator : MonoBehaviour
{
    [SerializeField] GameObject Simulator;
    [SerializeField] GameObject Body;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(Simulator.activeSelf)
        {
            Body.SetActive(true);
        }
        else
        {
            Body.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
