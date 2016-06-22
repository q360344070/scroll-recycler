using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class ScrollRecycler : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public IEnumerable<CellPool> CellPools { get { return CellPoolsByRecyclerLayoutPrefab.Values; } }
    public IEnumerable<LayoutRecycler> RecyclerLayouts { get { return RecyclerLayoutPrefabsByRecyclerLayoutInstances.Keys; } }
    // RecyclerLayout instances are mapped to their prefab reference
    Dictionary<LayoutRecycler, GameObject> RecyclerLayoutPrefabsByRecyclerLayoutInstances = 
        new Dictionary<LayoutRecycler, GameObject>();
    // CellPools are mapped to a RecyclerLayout prefab reference
    Dictionary<GameObject, CellPool> CellPoolsByRecyclerLayoutPrefab =
        new Dictionary<GameObject, CellPool>();
    [NonSerialized] public bool RecordChangedThisFrame;

    void Reset()
    {
        ScrollRect = GetComponent<ScrollRect>();
        if (!ScrollRect)
        {
            Debug.LogError(GetType().Name + " failed to find " + typeof(ScrollRect).Name);
        }
    }

    public LayoutRecycler InstantiateRecyclerLayout(LayoutRecycler recycler, Transform parent, int requiredPoolSize = 24)
    {
        return InstantiateRecyclerLayout(recycler.LayoutGroup.gameObject, parent, requiredPoolSize);
    }

    public LayoutRecycler InstantiateRecyclerLayout(string prefabPath, Transform parent, int requiredPoolSize = 24)
    {
        return InstantiateRecyclerLayout(ResourceCache.Inst.Load(prefabPath), parent, requiredPoolSize);
    }

    // The parent needs to be specified in advance for the CellPool
    public LayoutRecycler InstantiateRecyclerLayout(GameObject prefab, Transform parent, int requiredPoolSize = 24)
    {
        // Get or create cell pool
        CellPool cellPool = null;
        CellPoolsByRecyclerLayoutPrefab.TryGetValue(prefab, out cellPool);

        if (!cellPool)
        {
            // InstantiateCellPool()
            cellPool = ResourceCache.Inst.Create("ui_prefab/CellPool").GetComponent<CellPool>();
            cellPool.gameObject.SetActive(true);
            cellPool.gameObject.name = cellPool.GetType().Name + "[" + prefab.name + "](Clone)";
            cellPool.InitializePool();
            cellPool.ScrollRecycler = this;
            cellPool.transform.SetParent(ScrollRect.content.parent, false); // Sibling of ScrollRecycler
            CellPoolsByRecyclerLayoutPrefab[prefab] = cellPool;
        }
        cellPool.SetRequiredPoolSize(requiredPoolSize);

        // Initialize recycler layout
        GameObject recyclerLayoutGO = UnityEngine.Object.Instantiate(prefab);
        LayoutRecycler recyclerLayout = recyclerLayoutGO.GetComponent<IRecyclableLayout>().GetLayoutRecycler();

        recyclerLayoutGO.transform.SetParent(parent, false);
        RecyclerLayoutPrefabsByRecyclerLayoutInstances[recyclerLayout] = prefab;
        recyclerLayout.CellPool = cellPool;
        recyclerLayout.ScrollRecycler = this;
        if (recyclerLayout.CellPool.CellPrefab == null)
        {
            recyclerLayout.CellPool.CellPrefab = recyclerLayout.CellPrefab;
        }
        recyclerLayout.gameObject.SetActive(true);

        if (!cellPool.CellLayoutProxy)
        {
            // TODO: Evaluate how we want to use this
            // InstantiateCellLayoutProxy()
            var layoutProxyRoot = new GameObject();
            { // Copy the layout elements and rect size of the ScrollRect.content
                layoutProxyRoot.transform.SetParent(ScrollRect.content, false);
                var layoutProxyRootRtx = layoutProxyRoot.AddComponent<RectTransform>();
                layoutProxyRootRtx.CopyFromRectTransform(ScrollRect.content);
                layoutProxyRoot.AddComponent<LayoutElement>().ignoreLayout = true; // Ignores topmost layout calculations
                LayoutUtil.DuplicateLayoutElements(ScrollRect.content.gameObject, layoutProxyRoot);
                layoutProxyRoot.name += "(LayoutProxy)";
            }

            cellPool.CellLayoutProxy = Instantiate(recyclerLayout.CellPrefab);
            cellPool.CellLayoutProxy.gameObject.SetActive(true);
            cellPool.CellLayoutProxy.name = recyclerLayout.CellPrefab.name + "(LayoutProxy)";
            cellPool.CellLayoutProxy.transform.SetParent(layoutProxyRoot.transform, false);
            cellPool.CellLayoutProxy.transform.localPosition = Vector2.zero;

            var cellProxyCG = cellPool.CellLayoutProxy.GetComponent<CanvasGroup>();
            if (!cellProxyCG)
            {
                cellProxyCG = cellPool.CellLayoutProxy.AddComponent<CanvasGroup>();
            }
            cellProxyCG.alpha = 0.0f;

            var cellProxyLE = cellPool.CellLayoutProxy.GetComponent<LayoutElement>();
            if (!cellProxyLE)
            {
                cellProxyLE = cellPool.CellLayoutProxy.AddComponent<LayoutElement>();
            }
        }

        recyclerLayout.InitializedByManager = true;

        return recyclerLayout;
    }

    // NOTE: Using a counter is faster, but bug prone if user staggers pool instantiation
    public bool AllCellsInstantiated()
    {
        bool allPoolsInstantiated = true;
        foreach (CellPool pool in CellPoolsByRecyclerLayoutPrefab.Values)
        {
            if (!pool.AllCellsInstantiated)
            {
                allPoolsInstantiated = false;
            }
        }

        return allPoolsInstantiated;
    }

    public void ClearRecyclerLayouts()
    {
        foreach (LayoutRecycler layout in RecyclerLayoutPrefabsByRecyclerLayoutInstances.Keys)
        {
            if (layout != null)
            {
                layout.LayoutGroup.transform.SetParent(null);
                UnityEngine.Object.Destroy(layout.LayoutGroup.gameObject);
            }
        }

        RecyclerLayoutPrefabsByRecyclerLayoutInstances.Clear();

        // Return all cells to pools
        foreach (CellPool pool in CellPoolsByRecyclerLayoutPrefab.Values)
        {
            if (pool)
            {
                pool.ReturnAllPooledObjects();
            }
        }        
    }

    public void ScrollToRecord(CellRecord record)
    {
        if (record != null
            && record.RectTransformDimensions != null
            && record.RectTransformDimensions.parent != null)
        {
            Vector3 unitWorldPos = record.RectTransformDimensions.parent
                .TransformPoint(record.RectTransformDimensions.anchoredPosition);

            // NOTE: FIX THIS TO NOT USE SIZE DELTA NOW WE HAVE ANCHORED -> WORLD TRANSFORM
            ScrollRect.ScrollToChild(unitWorldPos, record.RectTransformDimensions.sizeDelta,
                record.RectTransformDimensions.pivot);
        }
    }

    // NOTE: ScrollRecycler's execution order is set after all other scripts so any logic that changes
    // GameObjects will have already been done by this point
    void LateUpdate()
    {
        // If a record changed in any of the recyclers, we need to rebuild all of them
        if (RecordChangedThisFrame)
        {
            // ForceRebuild for proxies
            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);

            // Calculate the LayoutInput (LayoutDimensions) for RecyclerLayout and LayoutDimensions for records 
            // using proxy
            foreach (LayoutRecycler lr in RecyclerLayoutPrefabsByRecyclerLayoutInstances.Keys)
            {
                lr.LayoutGroup.GetComponent<IRecyclableLayout>().ManualLayoutBuild();
            }

            RecordChangedThisFrame = false;
        }

        // Place records that should be visible, move off-screen records out of view
        foreach (LayoutRecycler lr in RecyclerLayoutPrefabsByRecyclerLayoutInstances.Keys)
        {
            lr.ShowAndPositionVisibleCells();
        }

        // Update cellpool position to reflect the scroll rect content
        foreach (CellPool cp in CellPools)
        {
            cp.transform.position = ScrollRect.content.position;
        }
    }
}

[Serializable]
public class CellRecord
{
    public GameObject Instance; // Instantiated gameObject
    public RectTransformDimensions RectTransformDimensions = new RectTransformDimensions();
    public LayoutDimensions LayoutDimensions = null;
    // TODO Specify if records need more than one Layout calculation per grouping
}
