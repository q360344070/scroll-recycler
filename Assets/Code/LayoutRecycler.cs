using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;

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
    [ReadOnly] public LayoutGroup LayoutGroup;
    public List<CellRecord> CellRecords = new List<CellRecord>();
    public List<RectTransformDimensions> CellRectTransformDimensions = new List<RectTransformDimensions>();

    [NonSerialized] public ScrollRecycler ScrollRecycler;
    [NonSerialized] public CellPool CellPool;
    [NonSerialized] public bool InitializedByManager = false;

    static Vector3[] _ViewportWorldCorners = new Vector3[4];

    public void AddRecord(CellRecord cellRecord)
    {
        cellRecord.RectTransformDimensions.CopyFromRectTransform((RectTransform)CellPool.CellLayoutProxy.transform); // Watch order of operations here
        cellRecord.RectTransformDimensions.parent = LayoutGroup.transform;
        CellRecords.Add(cellRecord);
        CellRectTransformDimensions.Add(cellRecord.RectTransformDimensions);
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
                Debug.DrawLine(new Vector3(recyclerBounds.Left, recyclerBounds.Top, 0.0f),
                    new Vector3(recyclerBounds.Right, recyclerBounds.Top, 0.0f), Color.green);

                Debug.DrawLine(new Vector3(recyclerBounds.Left, recyclerBounds.Bottom, 0.0f),
                    new Vector3(recyclerBounds.Right, recyclerBounds.Bottom, 0.0f), Color.green);

                Debug.DrawLine(new Vector3(recyclerBounds.Right, recyclerBounds.Top, 0.0f),
                    new Vector3(recyclerBounds.Right, recyclerBounds.Bottom, 0.0f), Color.green);

                Debug.DrawLine(new Vector3(recyclerBounds.Left, recyclerBounds.Top, 0.0f),
                    new Vector3(recyclerBounds.Left, recyclerBounds.Bottom, 0.0f), Color.green);
#endif

                for (int i = 0; i < CellRecords.Count; ++i) // Optimization: use 2D List grouping records by content space position
                {
                    CellRecord cellRecord = CellRecords[i];

                    // If that unit was active and we should cull it, return to pool
                    if (cellRecord.Instance != null && !IsRecordVisible(cellRecord, recyclerBounds))
                    {
                        CellPool.ReturnPooledObject(cellRecord.Instance);
                        cellRecord.Instance = null;
                    }
                }

                for (int i = 0; i < CellRecords.Count; ++i) // Optimization: use 2D List grouping records by content space position
                {
                    CellRecord cellRecord = CellRecords[i];

                    // If that unit was not active and we should show it, find a free pool unit, init and position
                    if (cellRecord.Instance == null && IsRecordVisible(cellRecord, recyclerBounds))
                    {
                        cellRecord.Instance = CellPool.GetPooledObject();

                        if (cellRecord.Instance) // Need to null check for pending cards
                        {
                            var cellInstance = cellRecord.Instance.GetComponent<IRecyclerCell>();
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

    static bool IsRecordVisible(CellRecord cr, RectBounds recyclerBounds)
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
}
