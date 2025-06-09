using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UBS_Logic_AND: MonoBehaviour {

    public List<GameObject> inputList = new List<GameObject>();
    public bool output;

 	// Update is called once per frame
	void Update ()
    {
        output = true;
        foreach (var input in inputList)
        {
            output = (output && input);
        }
    }


}
