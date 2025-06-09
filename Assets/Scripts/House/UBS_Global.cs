using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class UBS_Global : MonoBehaviour
{
    public static UBS_Global instance = null;
    public static LightBulbPreset lightBulbPreset;
    public DefaultObject defaultObjects;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

       }
}


[Serializable]
public class LightBulbPreset
{
    public Color color;
    public float range;
    public float intensity;
    public float shadowBias;
    public float shadowNearPlane;
    public LightRenderMode renderMode;
    public LightShadows shadows;
}


[Serializable]
public class DefaultObject
{
    public Material highlightMaterial;
}

