using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public interface IRecyclableLayout
{
    LayoutRecycler GetLayoutRecycler();
    void ManualLayoutBuild();
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
    public CellPool CellPool;

    public LayoutGroup LayoutGroup;
    //public GameObject CellLayoutProxy; // Evaluate how we want to use this

    public ScrollRecycler ScrollRecycler;

    static Vector3[] _ViewportWorldCorners = new Vector3[4];
    [NonSerialized]
    public bool InitializedByManager = false;

    public List<CellRecord> CellRecords = new List<CellRecord>();

    public void AddRecord(CellRecord cellRecord)
    {
        cellRecord.RectTransformDimensions.CopyFromRectTransform((RectTransform)CellPool.CellLayoutProxy.transform); // Watch order of operations here
        cellRecord.RectTransformDimensions.parent = LayoutGroup.transform;
        CellRecords.Add(cellRecord);
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
                            cellInstanceRtx.position = cellRecord.RectTransformDimensions.position;
                            cellInstanceRtx.SetAnchoredPosition3DZ(0.0f);
                            cellInstance.OnCellShow(cellRecord);
                        }
                    }
                }
            }
            else
            {
                // #debug - UNCOMMENT IN SHADOW
                //Assert.Never(LayoutGroup.gameObject.name + ": ChildCell prefab was null");
            }
        }
        else
        {
            // #debug - UNCOMMENT IN SHADOW
            //Assert.Never("RecyclerLayout was not instantiated by RecyclerManager, do not use UnityEngine.Object.Instantiate");
        }
    }

    static bool IsRecordVisible(CellRecord cr, RectBounds recyclerBounds)
    {
        Vector2 halfCardSize = cr.RectTransformDimensions.rect.size / 2.0f;
        Vector2 cardWorldPos = cr.RectTransformDimensions.parent.TransformPoint(cr.RectTransformDimensions.anchoredPosition);
        var cardWorldBounds = new RectBounds()
        {
            Top = cardWorldPos.y + halfCardSize.y,
            Bottom = cardWorldPos.y - halfCardSize.y,
            Right = cardWorldPos.x + halfCardSize.x,
            Left = cardWorldPos.x - halfCardSize.x
        };

        return cardWorldBounds.Right >= recyclerBounds.Left
            && cardWorldBounds.Left <= recyclerBounds.Right
            && cardWorldBounds.Bottom <= recyclerBounds.Top
            && cardWorldBounds.Top >= recyclerBounds.Bottom;
    }
}
