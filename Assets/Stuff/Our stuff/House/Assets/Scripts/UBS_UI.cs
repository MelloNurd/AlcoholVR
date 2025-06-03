using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// Startup functions for UBS features - 
/// Attach to UBS empty object in Hierarchy
/// </summary>


public class UBS_UI : MonoBehaviour
{
    public bool showMessageText1;
    public Text messageText1;
    public bool showMessageText2;
    public Text messageText2;
    public Canvas canvas;

    void Start ()
    {
        canvas.enabled = true;
        messageText1.enabled = showMessageText1;
        messageText2.enabled = showMessageText2;
        StartCoroutine(TitleScreenText());
        
    }

    IEnumerator TitleScreenText()
    {
        yield return new WaitForSeconds(8f);
        messageText1.text = "";
    }

    void Update ()
    {
		
	}
}
