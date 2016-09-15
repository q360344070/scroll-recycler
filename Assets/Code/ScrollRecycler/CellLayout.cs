using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

// Used for fields and methods on the CellLayouts because inheritance is messy with Unity's interfaces
[Serializable]
public class CellLayout
{
    public GameObject CellPrefab;
    public List<CellData> CellRecords = new List<CellData>();
    public List<RectTransformData> CellRecordsRectTransformData = new List<RectTransformData>();
    public CellPool CellPool;
    public ICellLayout ICellLayout;
    public bool LayoutNeedsRebuild;
    public ScrollRecycler ScrollRecycler;

    public void AddCells(params CellData[] cellRecords)
    {
        foreach (CellData cellData in cellRecords)
        {
            AddCell(cellData);
        }
    }

    public void AddCell(CellData cellRecord)
    {
        cellRecord.RectTransformData.CopyFromRectTransform((RectTransform)CellPool.CellLayoutProxy.transform); // Watch order of operations here
        cellRecord.RectTransformData.parent = ICellLayout.LayoutGroup.transform;
        CellRecords.Add(cellRecord);
        CellRecordsRectTransformData.Add(cellRecord.RectTransformData);
        LayoutNeedsRebuild = true;
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

        return cardWorldBounds.Right >= recyclerBounds.Left
            && cardWorldBounds.Left <= recyclerBounds.Right
            && cardWorldBounds.Bottom <= recyclerBounds.Top
            && cardWorldBounds.Top >= recyclerBounds.Bottom;
    }

    public void ShowAndPositionVisibleCells()
    {
        if (CellPrefab == null)
        {
            Assert.Never(ICellLayout.LayoutGroup.gameObject.name + ": ChildCell prefab was null");
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
                    var cellInstanceRtx = ((RectTransform)cellRecord.Instance.transform);

                    // Position cell
                    cellInstanceRtx.CopyFromRectTransformDimensions(cellRecord.RectTransformData);
                    cellInstanceRtx.position = cellRecord.RectTransformData.parent.TransformPoint(cellRecord.RectTransformData.localPosition);
                    cellInstanceRtx.SetAnchoredPosition3DZ(0.0f);
                    cellInstance.OnCellShow(cellRecord);
                }
            }
        }
       
    }

    public LayoutDimensions CreateLayoutDimensionsFromCellData(CellData cellData)
    {
        float minWidth = 0.0f;
        float preferredWidth = 0.0f;
        float flexibleWidth = 0.0f;

        float minHeight = 0.0f;
        float preferredHeight = 0.0f;
        float flexibleHeight = 0.0f;
        
        bool isVertical = ICellLayout.LayoutGroup is VerticalCellLayout;

        GetLayoutDimensionsFromCellData(
            cellData, 
            ICellLayout, 
            LayoutAxis.Horizontal, 
            isVertical, 
            ref minWidth, 
            ref preferredWidth, 
            ref flexibleWidth);

        GetLayoutDimensionsFromCellData(
            cellData, 
            ICellLayout, 
            LayoutAxis.Vertical, 
            isVertical, 
            ref minHeight, 
            ref preferredHeight, 
            ref flexibleHeight);

        return new LayoutDimensions()
        {
            minWidth = minWidth,
            preferredWidth = preferredWidth,
            flexibleWidth = flexibleWidth,
            minHeight = minHeight,
            preferredHeight = preferredHeight,
            flexibleHeight = flexibleHeight
        };
    }

    public void GetLayoutDimensionsFromCellData(
        CellData cellData,
        ICellLayout iCellLayout,
        LayoutAxis axis,
        bool isVertical,
        ref float minSize, 
        ref float preferredSize,
        ref float flexibleSize)
    {
        var rectTransform = (RectTransform)iCellLayout.LayoutGroup.transform;

        if (iCellLayout.CellLayout != null && iCellLayout.CellLayout.CellPrefab)
        {
            float rectSizeOnCurrAxis = rectTransform.rect.size[axis];
            bool flag = isVertical ^ axis == 1;

            if (flag)
            {
                float value = rectSizeOnCurrAxis - (float)((axis != 0)
                    ? iCellLayout.LayoutGroup.padding.vertical
                    : iCellLayout.LayoutGroup.padding.horizontal);
                for (int i = 0; i < iCellLayout.CellLayout.CellRecords.Count; i++)
                {
                    CellData currRecord = iCellLayout.CellLayout.CellRecords[i];

                    var cellInstanceRtx = (RectTransform)cellData.Instance.transform;

                    minSize = RecyclerUtil.GetMinSize(cellInstanceRtx, axis);
                    preferredSize = RecyclerUtil.GetPreferredSize(cellInstanceRtx, axis);
                    flexibleSize = RecyclerUtil.GetFlexibleSize(cellInstanceRtx, axis);
                    if ((axis != 0) 
                        ? ((HorizontalOrVerticalLayoutGroup)iCellLayout.LayoutGroup).childForceExpandHeight 
                        : ((HorizontalOrVerticalLayoutGroup)iCellLayout.LayoutGroup).childForceExpandWidth)
                    {
                        flexibleSize = Mathf.Max(flexibleSize, 1f);
                    }
                    float childSize = Mathf.Clamp(value, minSize, (flexibleSize <= 0f)
                        ? preferredSize
                        : rectSizeOnCurrAxis);
                    float startOffset = RecyclerUtil.GetStartOffset(iCellLayout.LayoutGroup, axis, childSize);
                    RecyclerUtil.SetChildAlongAxis(currRecord.RectTransformData, axis, startOffset, childSize);
                }
            }
            else
            {
                float childPos = (float)((axis != 0)
                    ? iCellLayout.LayoutGroup.padding.top
                    : iCellLayout.LayoutGroup.padding.left);
                if (RecyclerUtil.GetTotalFlexibleSize(iCellLayout.LayoutGroup, axis) == 0f
                    && RecyclerUtil.GetTotalPreferredSize(iCellLayout.LayoutGroup, axis) < rectSizeOnCurrAxis)
                {
                    childPos = RecyclerUtil.GetStartOffset(iCellLayout.LayoutGroup, axis,
                        RecyclerUtil.GetTotalPreferredSize(iCellLayout.LayoutGroup, axis) - (float)((axis != 0)
                        ? iCellLayout.LayoutGroup.padding.vertical
                        : iCellLayout.LayoutGroup.padding.horizontal));
                }
                float t = 0f;
                if (RecyclerUtil.GetTotalMinSize(iCellLayout.LayoutGroup, axis) 
                    != RecyclerUtil.GetTotalPreferredSize(iCellLayout.LayoutGroup, axis))
                {
                    t = Mathf.Clamp01((rectSizeOnCurrAxis - RecyclerUtil.GetTotalMinSize(iCellLayout.LayoutGroup, axis))
                        / (RecyclerUtil.GetTotalPreferredSize(iCellLayout.LayoutGroup, axis) - RecyclerUtil.GetTotalMinSize(iCellLayout.LayoutGroup, axis)));
                }
                float sizeRatio = 0f;
                if (rectSizeOnCurrAxis > RecyclerUtil.GetTotalPreferredSize(iCellLayout.LayoutGroup, axis)
                    && RecyclerUtil.GetTotalFlexibleSize(iCellLayout.LayoutGroup, axis) > 0f)
                {
                    sizeRatio = (rectSizeOnCurrAxis - RecyclerUtil.GetTotalPreferredSize(iCellLayout.LayoutGroup, axis))
                        / RecyclerUtil.GetTotalFlexibleSize(iCellLayout.LayoutGroup, axis);
                }
                for (int j = 0; j < iCellLayout.CellLayout.CellRecords.Count; j++)
                {
                    CellData currRecord = iCellLayout.CellLayout.CellRecords[j];

                    RectTransform childRect = (RectTransform)iCellLayout.CellLayout.CellPrefab.transform;
                    minSize = RecyclerUtil.GetMinSize(childRect, axis);
                    preferredSize = RecyclerUtil.GetPreferredSize(childRect, axis);
                    flexibleSize = RecyclerUtil.GetFlexibleSize(childRect, axis);

                    if ((axis != 0) 
                        ? ((HorizontalOrVerticalLayoutGroup)iCellLayout.LayoutGroup).childForceExpandHeight 
                        : ((HorizontalOrVerticalLayoutGroup)iCellLayout.LayoutGroup).childForceExpandWidth)
                    {
                        flexibleSize = Mathf.Max(flexibleSize, 1f);
                    }
                    float childSize = Mathf.Lerp(minSize, preferredSize, t);
                    childSize += flexibleSize * sizeRatio;
                    RecyclerUtil.SetChildAlongAxis(currRecord.RectTransformData, axis, childPos,
                        childSize);

                    childPos += childSize + ((HorizontalOrVerticalLayoutGroup)iCellLayout.LayoutGroup).spacing;
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

public interface ICellLayout
{
    CellLayout CellLayout { get; }
    LayoutGroup LayoutGroup { get; }
    void ManualLayoutBuild();
}
