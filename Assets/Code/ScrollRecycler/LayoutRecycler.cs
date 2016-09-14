//
// RecyclerLayout.cs
//
// Copyright 2016 MunkyFun. All rights reserved.
//

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Linq;
//using Core.Diagnostics;

public interface IRecyclableLayout
{
    LayoutRecycler GetLayoutRecycler();
    void ManualLayoutBuild();
    void ProxyLayoutBuild();
}

public class LayoutRecycler : MonoBehaviour
{
    struct RectBounds
    {
        public float Top;
        public float Bottom;
        public float Right;
        public float Left;
    }

    // Actual prefab asset reference assigned by user, CellPools mirror this particular asset reference
    public GameObject CellPrefab;
    [ReadOnly]
    public LayoutGroup LayoutGroup;
    public List<CellData> CellRecords = new List<CellData>(); // #donotcommit - Should be NonSerialized
    public List<RectTransformData> CellRectTransformDimensions = new List<RectTransformData>();

    [NonSerialized]
    public ScrollRecycler ScrollRecycler;
    [NonSerialized]
    public CellPool CellPool;
    // Allows control of when layouts should rebuild
    [NonSerialized]
    public bool InitializedByManager = false;

    static Vector3[] _ViewportWorldCorners = new Vector3[4];

    // NOTE: Contains() is expensive for large data sets, calling code should check if a duplicate is being added   
    public void AddRecord(CellData cellRecord)
    {
        cellRecord.RectTransformData.CopyFromRectTransform((RectTransform)CellPool.CellLayoutProxy.transform); // Watch order of operations here
        cellRecord.RectTransformData.parent = LayoutGroup.transform;
        CellRecords.Add(cellRecord);
        CellRectTransformDimensions.Add(cellRecord.RectTransformData);
        ScrollRecycler.RecordChangedThisFrame = true;
    }

    public void ShowAndPositionVisibleCells()
    {
        if (InitializedByManager)
        {
            if (CellPrefab)
            {
                // Cache bounds for the viewport
                ScrollRecycler.ScrollRect.viewport.GetWorldCorners(_ViewportWorldCorners);

                // TODO: Add thresholds for each of these measurements (if actually needed)
                RectBounds recyclerBounds = new RectBounds()
                {
                    Top = Vector3Extensions.GetMaxY(_ViewportWorldCorners),
                    Bottom = Vector3Extensions.GetMinY(_ViewportWorldCorners),
                    Left = Vector3Extensions.GetMinX(_ViewportWorldCorners),
                    Right = Vector3Extensions.GetMaxX(_ViewportWorldCorners)
                };

#if UNITY_EDITOR
                Debug.DrawLine(
                    new Vector3(recyclerBounds.Left, recyclerBounds.Top, 0.0f),
                    new Vector3(recyclerBounds.Right, recyclerBounds.Top, 0.0f),
                    Color.green);

                Debug.DrawLine(
                    new Vector3(recyclerBounds.Left, recyclerBounds.Bottom, 0.0f),
                    new Vector3(recyclerBounds.Right, recyclerBounds.Bottom, 0.0f),
                    Color.green);

                Debug.DrawLine(
                    new Vector3(recyclerBounds.Right, recyclerBounds.Top, 0.0f),
                    new Vector3(recyclerBounds.Right, recyclerBounds.Bottom, 0.0f),
                    Color.green);

                Debug.DrawLine(
                    new Vector3(recyclerBounds.Left, recyclerBounds.Top, 0.0f),
                    new Vector3(recyclerBounds.Left, recyclerBounds.Bottom, 0.0f),
                    Color.green);
#endif

                for (int i = 0; i < CellRecords.Count; ++i) // Optimization: use 2D List grouping records by content space position
                {
                    CellData cellRecord = CellRecords[i];

                    // If that unit was active and we should cull it, return to pool
                    if (cellRecord.Instance != null && !IsRecordVisible(cellRecord, recyclerBounds))
                    {
                        CellPool.ReturnPooledObject(cellRecord.Instance);
                        cellRecord.Instance = null;
                    }
                }

                for (int i = 0; i < CellRecords.Count; ++i) // Optimization: use 2D List grouping records by content space position
                {
                    CellData cellRecord = CellRecords[i];

                    // If that unit was not active and we should show it, find a free pool unit, init and position
                    if (cellRecord.Instance == null && IsRecordVisible(cellRecord, recyclerBounds))
                    {
                        cellRecord.Instance = CellPool.GetPooledObject();

                        if (cellRecord.Instance) // Need to null check for pending cards
                        {
                            var cellInstance = cellRecord.Instance.GetComponent<IRecyclableCell>();
                            var cellInstanceRtx = (RectTransform)cellRecord.Instance.transform;

                            // Position cell
                            cellInstanceRtx.CopyFromRectTransformDimensions(cellRecord.RectTransformData);
                            cellInstanceRtx.position =
                                cellRecord.RectTransformData.parent.TransformPoint(
                                    cellRecord.RectTransformData.localPosition);
                            cellInstanceRtx.SetAnchoredPosition3DZ(0.0f);

                            cellInstance.OnCellShow(cellRecord);
                        }
                    }
                }
            }
            else
            {
                Assert.Never(LayoutGroup.gameObject.name + ": ChildCell prefab was null");
            }
        }
        else
        {
            Assert.Never("RecyclerLayout was not instantiated by RecyclerManager, do not use UnityEngine.Object.Instantiate");
        }
    }

    static bool IsRecordVisible(CellData cr, RectBounds recyclerBounds)
    {
        Vector2 halfCardSize = cr.RectTransformData.rect.size / 2.0f;
        Vector2 cardWorldPos = cr.RectTransformData.parent.TransformPoint(
            cr.RectTransformData.localPosition);

        var cardWorldBounds = new RectBounds()
        {
            Top = cardWorldPos.y + halfCardSize.y,
            Bottom = cardWorldPos.y - halfCardSize.y,
            Right = cardWorldPos.x + halfCardSize.x,
            Left = cardWorldPos.x - halfCardSize.x
        };

        //{// #debug
        //    Debug.Log("LayoutRecycler.IsRecordVisible() : "
        //        + "  unit = " + ((CardRecord)cr).unitInstance
        //        + "  Check = " + (cardWorldBounds.Right >= recyclerBounds.Left && cardWorldBounds.Left <= recyclerBounds.Right && cardWorldBounds.Bottom <= recyclerBounds.Top && cardWorldBounds.Top >= recyclerBounds.Bottom)

        //        + "  cardWorldBounds.Right >= recyclerBounds.Left = " + (cardWorldBounds.Right >= recyclerBounds.Left)
        //        + "  cardWorldBounds.Left <= recyclerBounds.Right = " + (cardWorldBounds.Left <= recyclerBounds.Right)
        //        + "  cardWorldBounds.Bottom <= recyclerBounds.Top = " + (cardWorldBounds.Bottom <= recyclerBounds.Top)
        //        + "  cardWorldBounds.Top >= recyclerBounds.Bottom = " + (cardWorldBounds.Top >= recyclerBounds.Bottom)

        //        + "  cardWorldBounds.Right = " + cardWorldBounds.Right
        //        + "  cardWorldBounds.Left = " + cardWorldBounds.Left
        //        + "  cardWorldBounds.Bottom = " + cardWorldBounds.Bottom
        //        + "  cardWorldBounds.Top = " + cardWorldBounds.Top

        //        + "  recyclerBounds.Left = " + recyclerBounds.Left
        //        + "  recyclerBounds.Right = " + recyclerBounds.Right
        //        + "  recyclerBounds.Top = " + recyclerBounds.Top
        //        + "  recyclerBounds.Bottom = " + recyclerBounds.Bottom);
        //}


        return
            cardWorldBounds.Right >= recyclerBounds.Left
            && cardWorldBounds.Left <= recyclerBounds.Right
            && cardWorldBounds.Bottom <= recyclerBounds.Top
            && cardWorldBounds.Top >= recyclerBounds.Bottom;
    }
}
