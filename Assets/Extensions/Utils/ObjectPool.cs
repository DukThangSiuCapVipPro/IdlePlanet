using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool
{
    private static Dictionary<GameObject, Queue<GameObject>> poolDict = new Dictionary<GameObject, Queue<GameObject>>();
    private static Dictionary<GameObject, GameObject> instanceToPrefab = new Dictionary<GameObject, GameObject>();

    // Use MonoBehaviour directly, no need for .gameObject from the caller
    public static T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour
    {
        GameObject prefabGO = prefab.gameObject;

        if (!poolDict.ContainsKey(prefabGO))
            poolDict[prefabGO] = new Queue<GameObject>();

        GameObject obj;
        if (poolDict[prefabGO].Count > 0)
        {
            obj = poolDict[prefabGO].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
        }
        else
        {
            obj = GameObject.Instantiate(prefabGO, position, rotation);
            instanceToPrefab[obj] = prefabGO;
        }

        T component = obj.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"Component {typeof(T).Name} not found on instantiated prefab!");
        }

        return component;
    }
    public static GameObject Spawn(GameObject prefabGO, Vector3 position, Quaternion rotation)
    {
        if (!poolDict.ContainsKey(prefabGO))
            poolDict[prefabGO] = new Queue<GameObject>();

        GameObject obj;
        if (poolDict[prefabGO].Count > 0)
        {
            obj = poolDict[prefabGO].Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
        }
        else
        {
            obj = GameObject.Instantiate(prefabGO, position, rotation);
            instanceToPrefab[obj] = prefabGO;
        }

        return obj;
    }

    public static T Spawn<T>(T prefab, Transform parent) where T : Component
    {
        GameObject prefabGO = prefab.gameObject;

        if (!poolDict.ContainsKey(prefabGO))
            poolDict[prefabGO] = new Queue<GameObject>();

        GameObject obj;
        if (poolDict[prefabGO].Count > 0)
        {
            obj = poolDict[prefabGO].Dequeue();
            obj.transform.SetParent(parent, false);
            obj.SetActive(true);
        }
        else
        {
            obj = GameObject.Instantiate(prefabGO, parent);
            instanceToPrefab[obj] = prefabGO;
        }

        T component = obj.GetComponent<T>();
        if (component == null)
        {
            Debug.LogError($"Component {typeof(T).Name} not found on prefab!");
        }

        return component;
    }

    public static void DeSpawn(Component obj)
    {
        if (obj == null) return;

        GameObject go = obj.gameObject;

        if (instanceToPrefab.TryGetValue(go, out GameObject prefab))
        {
            go.SetActive(false);
            poolDict[prefab].Enqueue(go);
        }
        else
        {
            GameObject.Destroy(go);
        }
    }

    public static void Clear()
    {
        // Destroy all pooled objects before clearing dictionaries
        foreach (var queue in poolDict.Values)
        {
            while (queue.Count > 0)
            {
                GameObject obj = queue.Dequeue();
                if (obj != null)
                {
                    GameObject.Destroy(obj);
                }
            }
        }

        // Also destroy any active instances still tracked
        foreach (var instance in instanceToPrefab.Keys)
        {
            if (instance != null)
            {
                GameObject.Destroy(instance);
            }
        }

        poolDict.Clear();
        instanceToPrefab.Clear();
    }
}
