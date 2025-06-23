using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    public List<ObjectiveSystem> objectives = new List<ObjectiveSystem>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.parent);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
