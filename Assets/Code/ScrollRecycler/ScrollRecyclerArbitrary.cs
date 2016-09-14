﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class ScrollRecyclerArbitrary : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public IEnumerable<CellPool> CellPools { get { return CellLayoutPrefabsToCellPools.Values; } }
    public IEnumerable<GameObject> CellLayoutPrefabs { get { return CellLayoutPrefabsToCellPools.Keys; } }
    public Vector3[] ViewportWorldCorners = new Vector3[4];

    Dictionary<GameObject, CellPool> CellLayoutPrefabsToCellPools = new Dictionary<GameObject, CellPool>();

    List<ICellLayout> ICellLayouts = new List<ICellLayout>();

    void Awake()
    {
        ScrollRect.viewport.GetWorldCorners(ViewportWorldCorners);
    }

    void OnDestroy()
    {
    }

    void Reset()
    {
        ScrollRect = GetComponent<ScrollRect>();
        if (!ScrollRect)
        {
            Debug.LogError(GetType().Name + " failed to find " + typeof(ScrollRect).Name);
        }
    }

    public ICellLayout InstantiateCellLayout(string prefabPath, int requiredPoolSize = 24)
    {
        return InstantiateCellLayout(ResourceCache.Inst.Load(prefabPath), requiredPoolSize);
    }

    // The parent needs to be specified in advance for the CellPool
    public ICellLayout InstantiateCellLayout(GameObject cellLayoutPrefab, int requiredPoolSize = 24)
    {
        // Get or create cell pool
        CellPool cellPool = null;
        CellLayoutPrefabsToCellPools.TryGetValue(cellLayoutPrefab, out cellPool);

        if (!cellPool)
        {
            // InstantiateCellPool()
            cellPool = ResourceCache.Inst.Create("CellPool").GetComponent<CellPool>();
            cellPool.gameObject.SetActive(true);
            cellPool.gameObject.name = cellPool.GetType().Name + "[" + cellLayoutPrefab.name + "](Clone)";
            cellPool.InitializePool();
            cellPool.ScrollRecycler = this;
            cellPool.transform.SetParent(ScrollRect.content.parent, false); // Sibling of ScrollRecycler
            CellLayoutPrefabsToCellPools[cellLayoutPrefab] = cellPool;
        }
        cellPool.SetRequiredPoolSize(requiredPoolSize);

        // Initialize recycler layout
        GameObject cellLayoutGO = UnityEngine.Object.Instantiate(cellLayoutPrefab);
        ICellLayout iCellLayout = cellLayoutGO.GetComponent<ICellLayout>();
        CellLayout cellLayoutData = iCellLayout.GetCellLayout();
        ICellLayouts.Add(iCellLayout);

        cellLayoutGO.transform.SetParent(ScrollRect.content, false);
        cellLayoutData.CellPool = cellPool;
        cellLayoutData.ScrollRecycler = this;
        if (cellLayoutData.CellPool.CellPrefab == null)
        {
            cellLayoutData.CellPool.CellPrefab = cellLayoutData.CellPrefab;
        }
        cellLayoutGO.SetActive(true);

        if (!cellPool.CellLayoutProxy)
        {
            // TODO: Evaluate how we want to use this
            // InstantiateCellLayoutProxy()
            //var layoutProxyRoot = new GameObject();
            //{ // Copy the layout elements and rect size of the ScrollRect.content
            //    layoutProxyRoot.transform.SetParent(ScrollRect.content, false);
            //    var layoutProxyRootRtx = layoutProxyRoot.AddComponent<RectTransform>();
            //    layoutProxyRootRtx.CopyFromRectTransform(ScrollRect.content);
            //    layoutProxyRoot.AddComponent<LayoutElement>().ignoreLayout = true; // Ignores topmost layout calculations
            //    LayoutUtil.DuplicateLayoutElements(ScrollRect.content.gameObject, layoutProxyRoot);
            //    layoutProxyRoot.name += "(LayoutProxy)";
            //}

            cellPool.CellLayoutProxy = Instantiate(cellLayoutData.CellPrefab);
            cellPool.CellLayoutProxy.gameObject.SetActive(true);
            cellPool.CellLayoutProxy.name = cellLayoutData.CellPrefab.name + "(LayoutProxy)";
            cellPool.CellLayoutProxy.transform.SetParent(cellLayoutGO.transform, false);
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

        return iCellLayout;
    }

    // NOTE: Using a counter is faster, but bug prone if user staggers pool instantiation
    public bool AllCellsInstantiated()
    {
        bool allPoolsInstantiated = true;
        foreach (CellPool pool in CellPools)
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
        foreach (ICellLayout layout in ICellLayouts)
        {
            if (layout != null)
            {
                layout.GetLayoutGroup().transform.SetParent(null);
                UnityEngine.Object.Destroy(layout.GetLayoutGroup().gameObject);
            }
        }

        // Return all cells to pools
        foreach (CellPool pool in CellPools)
        {
            if (pool)
            {
                pool.ReturnAllPooledObjects();
            }
        }
    }

    public void ScrollToRecord(CellData record)
    {
        if (record != null
            && record.RectTransformData != null
            && record.RectTransformData.parent != null)
        {
            Vector3 unitWorldPos = record.RectTransformData.parent
                .TransformPoint(record.RectTransformData.anchoredPosition);

            // NOTE: FIX THIS TO NOT USE SIZE DELTA NOW WE HAVE ANCHORED -> WORLD TRANSFORM
            ScrollRect.ScrollToChild(unitWorldPos, record.RectTransformData.sizeDelta,
                record.RectTransformData.pivot);
        }
    }

    // NOTE: ScrollRecycler's execution order is set after all other scripts
    void LateUpdate()
    {
        // Update cell position
        foreach (CellPool cp in CellPools)
        {
            cp.transform.position = ScrollRect.content.position;
        }

        foreach (ICellLayout iCellLayout in ICellLayouts)
        {
            CellLayout cl = iCellLayout.GetCellLayout();
            if (cl.LayoutNeedsRebuild)
            {
                iCellLayout.ManualLayoutBuild();
                cl.LayoutNeedsRebuild = false;
            }
        }

        // Place records that should be visible, move off-screen records out of view
        foreach (ICellLayout icl in ICellLayouts)
        {
            CellLayout cl = icl.GetCellLayout();
            cl.ShowAndPositionVisibleCells();
        }
    }
}