using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceCache
{
    public static ResourceCache Inst
    {
        get
        {
            if (_Inst == null)
            {
                _Inst = new ResourceCache();
            }
            return _Inst;
        }
    }

    static Dictionary<string, GameObject> cachedObjects = new Dictionary<string, GameObject>();

    static ResourceCache _Inst;

    public GameObject Load(string prefabName)
    {
        GameObject loadedPrefabAsset = null;

        if (!cachedObjects.TryGetValue(prefabName, out loadedPrefabAsset))
        {
            loadedPrefabAsset = cachedObjects[prefabName] = Resources.Load<GameObject>(prefabName);
        }

        return loadedPrefabAsset;
    }

    public GameObject Create(string prefabName)
    {
        return (GameObject)Object.Instantiate(Load(prefabName));
    }
}
