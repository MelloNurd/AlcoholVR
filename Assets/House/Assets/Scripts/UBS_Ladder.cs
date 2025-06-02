
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
 public class UBS_Ladder : MonoBehaviour
{

    GameObject playerOBJ ;
    public bool canClimb = false;
    public float speed = 1;
    public string state;

    private void Start()
    {
        playerOBJ = GameObject.Find("Player");
        state = "Undefined";
    }

    void OnTriggerEnter(Collider coll1)
    {
        state = "Touching Ladder";
        if (coll1.gameObject.tag == "Player")
        {
            state = "On Ladder";
            canClimb = true;
            playerOBJ = coll1.gameObject;
        }
    }

    void OnTriggerExit(Collider coll2)
    {
        state = "Not Touching Ladder";
        if (coll2.gameObject.tag == "Player")
        {
            state = "Exited Ladder";
            canClimb = false;
            playerOBJ = null;
        }
    }

    void Update()
    {
        if (canClimb)
        {
            if (Input.GetKey(KeyCode.W))
            {
                playerOBJ.transform.Translate(new Vector3(0, 1, 0) * Time.deltaTime * speed);
            }
            if (Input.GetKey(KeyCode.S))
            {
                playerOBJ.transform.Translate(new Vector3(0, -1, 0) * Time.deltaTime * speed);
            }
        }
    }
}
