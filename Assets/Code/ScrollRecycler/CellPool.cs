using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CellPool : MonoBehaviour
{
    [NonSerialized] public ScrollRecycler ScrollRecycler;
    [NonSerialized] public GameObject CellPrefab;
    [NonSerialized] public GameObject CellLayoutProxy; // Evaluate how we want to use this
    public bool AllCellsInstantiated { get { return AllPooledObjects.Count >= RequiredPoolSize; } }
    int RequiredPoolSize;
    [NonSerialized] public List<GameObject> AllPooledObjects = new List<GameObject>();
    [NonSerialized] public Stack<GameObject> AvailablePooledObjects = new Stack<GameObject>();
    public event Action OnAllObjectsInstantiated;
    bool FirstFrame = true;

    public void InitializePool()
    {
        ClearPooledObjects();
    }

    public void SetRequiredPoolSize(int requiredSize)
    {
        RequiredPoolSize = Math.Max(RequiredPoolSize, requiredSize);
    }

    void Update()
    {
        if (!FirstFrame)
        {
            TryInstantiatePendingObject();
        }

        FirstFrame = false;
    }

    public void ReturnAllPooledObjects()
    {
        foreach (GameObject oldGo in AllPooledObjects)
        {
            ReturnPooledObject(oldGo);
        }
    }

    public void ClearPooledObjects()
    {
        foreach (GameObject oldGo in AllPooledObjects)
        {
            if (oldGo)
                GameObject.Destroy(oldGo);
        }

        AvailablePooledObjects.Clear();
        AllPooledObjects.Clear();
    }

    public void ReturnPooledObject(GameObject go)
    {
        if (go)
        {
            go.transform.position = new Vector3(Screen.width * 4.0f, Screen.height * 4.0f); // Move off-screen
            AvailablePooledObjects.Push(go);
        }
    }

    public GameObject GetPooledObject()
    {
        // has available object
        if (AvailablePooledObjects.Count > 0)
        {
            GameObject freeGo = AvailablePooledObjects.Pop();
            return freeGo;
        }
        // if done paging in all the objects then manually add another
        else if (AllCellsInstantiated)
        {
            ++RequiredPoolSize;

            Assert.Never("RequiredPoolSize is not sufficient. Please increase it. Need count:{0}, Prefab Name:{1}", RequiredPoolSize, CellPrefab.name);

            TryInstantiatePendingObject();
            GameObject freeGo = AvailablePooledObjects.Pop();
            return freeGo;
        }
        // wait until paging is done. We handle returning null
        else
        {
            return null;
        }
    }

    void TryInstantiatePendingObject()
    {
        if (AllPooledObjects.Count < RequiredPoolSize)
        {
            // Get CellPrefab from matching RecyclerLayout
            GameObject newGo = UnityEngine.Object.Instantiate(CellPrefab);
            newGo.transform.SetParent(transform, false);
            newGo.transform.position = new Vector3(Screen.width * 4.0f, Screen.height * 4.0f); // Move off-screen
            AllPooledObjects.Add(newGo.gameObject);
            AvailablePooledObjects.Push(newGo);

            var newRecyclerCell = newGo.GetComponent<IRecyclableCell>();
            newRecyclerCell.OnCellInstantiate();

            if (AllPooledObjects.Count >= RequiredPoolSize)
            {
                if (OnAllObjectsInstantiated != null)
                {
                    OnAllObjectsInstantiated();
                }
            }
        }
    }
}
