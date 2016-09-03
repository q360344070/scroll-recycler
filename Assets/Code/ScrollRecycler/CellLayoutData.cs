using UnityEngine;
using System.Collections.Generic;
using System;

// Used for fields and methods on the CellLayouts because inheritance is messy with Unity's interfaces
[Serializable]
public class CellLayout
{
    public GameObject CellPrefab;
    public List<CellData> AllCellsData = new List<CellData>();
    public List<RectTransformData> AllCellsRectTransformData = new List<RectTransformData>();
    public CellPool CellPool;
    public ICellLayout ICellLayout;
    public bool RecordChangedThisFrame;
    public ScrollRecycler ScrollRecycler;

    public void AddCell(CellData cellRecord)
    {
        cellRecord.RectTransformDimensions.CopyFromRectTransform((RectTransform)CellPool.CellLayoutProxy.transform); // Watch order of operations here
        cellRecord.RectTransformDimensions.parent = ICellLayout.GetLayoutGroup().transform;
        AllCellsData.Add(cellRecord);
        RecordChangedThisFrame = true;
    }

    static bool IsRecordVisible(CellData cr, RectBounds recyclerBounds)
    {
        Vector2 halfCardSize = cr.RectTransformDimensions.rect.size / 2.0f;
        Vector2 cardWorldPos = cr.RectTransformDimensions.parent.TransformPoint(
            cr.RectTransformDimensions.localPosition);

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


        return cardWorldBounds.Right >= recyclerBounds.Left
            && cardWorldBounds.Left <= recyclerBounds.Right
            && cardWorldBounds.Bottom <= recyclerBounds.Top
            && cardWorldBounds.Top >= recyclerBounds.Bottom;
    }

    public void ShowAndPositionVisibleCells()
    {
        if (CellPrefab == null)
        {
            Assert.Never(ICellLayout.GetLayoutGroup().gameObject.name + ": ChildCell prefab was null");
            return;
        }

        // Cache bounds for the viewport
        ScrollRecycler.ScrollRect.viewport.GetWorldCorners(ScrollRecycler.ViewportWorldCorners);

        // TODO: Add thresholds for each of these measurements (if actually needed)
        RectBounds recyclerBounds = new RectBounds()
        {
            Top = Vector3Extensions.GetMaxY(ScrollRecycler.ViewportWorldCorners),
            Bottom = Vector3Extensions.GetMinY(ScrollRecycler.ViewportWorldCorners),
            Left = Vector3Extensions.GetMinX(ScrollRecycler.ViewportWorldCorners),
            Right = Vector3Extensions.GetMaxX(ScrollRecycler.ViewportWorldCorners)
        };

#if UNITY_EDITOR
        Debug.DrawLine(new Vector3(recyclerBounds.Left, recyclerBounds.Top, 0.0f),
            new Vector3(recyclerBounds.Right, recyclerBounds.Top, 0.0f), Color.green);

        Debug.DrawLine(new Vector3(recyclerBounds.Left, recyclerBounds.Bottom, 0.0f),
            new Vector3(recyclerBounds.Right, recyclerBounds.Bottom, 0.0f), Color.green);

        Debug.DrawLine(new Vector3(recyclerBounds.Right, recyclerBounds.Top, 0.0f),
            new Vector3(recyclerBounds.Right, recyclerBounds.Bottom, 0.0f), Color.green);

        Debug.DrawLine(new Vector3(recyclerBounds.Left, recyclerBounds.Top, 0.0f),
            new Vector3(recyclerBounds.Left, recyclerBounds.Bottom, 0.0f), Color.green);
#endif

        for (int i = 0; i < AllCellsData.Count; ++i) // Optimization: use 2D List grouping records by content space position
        {
            CellData cellRecord = AllCellsData[i];

            // If that unit was active and we should cull it, return to pool
            if (cellRecord.Instance != null && !IsRecordVisible(cellRecord, recyclerBounds))
            {
                CellPool.ReturnPooledObject(cellRecord.Instance);
                cellRecord.Instance = null;
            }
        }

        for (int i = 0; i < AllCellsData.Count; ++i) // Optimization: use 2D List grouping records by content space position
        {
            CellData cellRecord = AllCellsData[i];

            // If that unit was not active and we should show it, find a free pool unit, init and position
            if (cellRecord.Instance == null && IsRecordVisible(cellRecord, recyclerBounds))
            {
                cellRecord.Instance = CellPool.GetPooledObject();

                if (cellRecord.Instance) // Need to null check for pending cards
                {
                    var cellInstance = cellRecord.Instance.GetComponent<IRecyclableCell>();
                    var cellInstanceRtx = ((RectTransform)cellRecord.Instance.transform);

                    // Position cell
                    cellInstanceRtx.CopyFromRectTransformDimensions(cellRecord.RectTransformDimensions);
                    cellInstanceRtx.position = cellRecord.RectTransformDimensions.parent.TransformPoint(cellRecord.RectTransformDimensions.localPosition);
                    cellInstanceRtx.SetAnchoredPosition3DZ(0.0f);
                    cellInstance.OnCellShow(cellRecord);
                }
            }
        }
       
    }

    struct RectBounds
    {
        public float Top;
        public float Bottom;
        public float Right;
        public float Left;
    }
}
