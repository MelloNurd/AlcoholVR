using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PoolType
{
    Arcade,
    Phone,
    None
}

public class ObjectPoolManager : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

    private GameObject _objectPoolParent;

    public static GameObject _arcadePoolParent;
    public static GameObject _phonePoolParent;

    private void Awake()
    {
        SetupPoolParentObjects();
    }

    private void SetupPoolParentObjects()
    {
        _objectPoolParent = new GameObject("Pooled Objects");
        DontDestroyOnLoad(_objectPoolParent);

        _arcadePoolParent = new GameObject("Arcade Sprites");
        _arcadePoolParent.transform.SetParent(_objectPoolParent.transform);

        _phonePoolParent = new GameObject("Phone Sprites");
        _phonePoolParent.transform.SetParent(_objectPoolParent.transform);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, PoolType poolType = PoolType.None)
    {
        return SpawnObject(objectToSpawn, Vector3.zero, Quaternion.identity, poolType);
    }
    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPosition, Quaternion spawnRotation, PoolType poolType = PoolType.None)
    {
        PooledObjectInfo pool = ObjectPools.Find(x => x.LookupString == objectToSpawn.name);

        if (pool == null)
        {
            pool = new PooledObjectInfo() { LookupString = objectToSpawn.name };
            ObjectPools.Add(pool);
        }

        // Check for inactive objects in the pool
        GameObject spawnableObj = pool.InactiveObjects.FirstOrDefault();

        if(spawnableObj == null)
        {
            GameObject parentObject = SetParentObject(poolType);
            spawnableObj = Instantiate(objectToSpawn, spawnPosition, spawnRotation);

            if(parentObject != null)
            {
                spawnableObj.transform.SetParent(parentObject.transform);
            }
        }
        else
        {
            spawnableObj.transform.position = spawnPosition;
            spawnableObj.transform.rotation = spawnRotation;
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    public static void ReturnObjectToPool(GameObject obj)
    {
        string goName = obj.name.Substring(0, obj.name.Length - 7); // Remove the "(Clone)" from the name

        PooledObjectInfo pool = ObjectPools.Find(x => x.LookupString == goName);

        if(pool == null)
        {
            Debug.LogWarning("Trying to release an object that is not pooled: " + obj.name);
        }
        else
        {
            obj.SetActive(false);
            pool.InactiveObjects.Add(obj);
        }
    }

    private static GameObject SetParentObject(PoolType poolType)
    {
        return poolType switch
        {
            PoolType.Arcade => _arcadePoolParent,
            PoolType.Phone => _phonePoolParent,
            _ => null, // Default, includes PoolType.None
        };
    }
}

public class PooledObjectInfo // Think of this as one pool of objects
{ 
    public string LookupString;
    public List<GameObject> InactiveObjects = new List<GameObject>();
}
