using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UBS_Logic_OR : MonoBehaviour {

    public List<GameObject> inputList = new List<GameObject>();
    public bool output;

    // Update is called once per frame
    void Update()
    {
        output = false;
        foreach (var input in inputList)
        {
            output = (output || input);
        }
    }
}
