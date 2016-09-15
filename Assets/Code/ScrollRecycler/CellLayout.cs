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
        cellRecord.RectTransformData.CopyFromRectTransform((RectTransform)CellPool.CellProxy.transform); // Watch order of operations here
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

    public LayoutData PrecalculateCellLayoutData(CellData cellData)
    {
        // Set up the proxy cell to reflect the same layout and rect as our cell would be instantiated as
        var recyclerCell = CellPool.CellProxy.GetComponent<IRecyclableCell>();
        var proxyRtx = (RectTransform)CellPool.CellProxy.transform;
        recyclerCell.OnCellInstantiate();
        recyclerCell.OnCellShow(cellData);
        LayoutRebuilder.ForceRebuildLayoutImmediate(proxyRtx);

        return GetProxyLayoutData();
    }

    LayoutData GetProxyLayoutData()
    {
        LayoutDimensions layoutDims = new LayoutDimensions();
        bool isVertical = ICellLayout.LayoutGroup is VerticalCellLayout;

        GetCellLayoutDimensions(
            (RectTransform)CellPool.CellProxy.transform,
            (int)LayoutAxis.Horizontal,
            isVertical,
            ref layoutDims.minSize.x,
            ref layoutDims.preferredSize.x,
            ref layoutDims.flexibleSize.x);

        GetCellLayoutDimensions(
            (RectTransform)CellPool.CellProxy.transform,
            (int)LayoutAxis.Vertical,
            isVertical,
            ref layoutDims.minSize.y,
            ref layoutDims.preferredSize.y,
            ref layoutDims.flexibleSize.y);
        
        return new LayoutData(layoutDims);
    }

    void GetCellLayoutDimensions(
        RectTransform cellRtx,
        int axis,
        bool isVertical,
        ref float minSize,
        ref float preferredSize,
        ref float flexibleSize)
    {
        var rectTransform = (RectTransform)ICellLayout.LayoutGroup.transform;

        if (ICellLayout.CellLayout != null && ICellLayout.CellLayout.CellPrefab)
        {
            float rectSizeOnCurrAxis = rectTransform.rect.size[axis];
            bool flag = isVertical ^ axis == 1;

            if (flag)
            {
                float value = rectSizeOnCurrAxis -
                    ((axis != 0)
                    ? ICellLayout.LayoutGroup.padding.vertical
                    : ICellLayout.LayoutGroup.padding.horizontal);
                for (int i = 0; i < ICellLayout.CellLayout.CellRecords.Count; i++)
                {
                    CellData currRecord = ICellLayout.CellLayout.CellRecords[i];

                    minSize = RecyclerUtil.GetMinSize(cellRtx, axis);
                    preferredSize = RecyclerUtil.GetPreferredSize(cellRtx, axis);
                    flexibleSize = RecyclerUtil.GetFlexibleSize(cellRtx, axis);

                    if ((axis != 0)
                        ? ((HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup).childForceExpandHeight
                        : ((HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup).childForceExpandWidth)
                    {
                        flexibleSize = Mathf.Max(flexibleSize, 1f);
                    }

                    float childSize = Mathf.Clamp(
                        value,
                        minSize,
                        (flexibleSize <= 0f) ? preferredSize : rectSizeOnCurrAxis);
                    float startOffset = RecyclerUtil.GetStartOffset(ICellLayout.LayoutGroup, axis, childSize);
                    SetChildAlongAxis(currRecord.RectTransformData, axis, startOffset, childSize);
                }
            }
            else
            {
                float childPos =
                    ((axis != 0)
                    ? ICellLayout.LayoutGroup.padding.top
                    : ICellLayout.LayoutGroup.padding.left);
                if (RecyclerUtil.GetTotalFlexibleSize(ICellLayout.LayoutGroup, axis) == 0f
                    && RecyclerUtil.GetTotalPreferredSize(ICellLayout.LayoutGroup, axis) < rectSizeOnCurrAxis)
                {
                    childPos = RecyclerUtil.GetStartOffset(ICellLayout.LayoutGroup, axis,
                        RecyclerUtil.GetTotalPreferredSize(ICellLayout.LayoutGroup, axis) -
                        ((axis != 0)
                        ? ICellLayout.LayoutGroup.padding.vertical
                        : ICellLayout.LayoutGroup.padding.horizontal));
                }
                float t = 0f;
                if (RecyclerUtil.GetTotalMinSize(ICellLayout.LayoutGroup, axis)
                    != RecyclerUtil.GetTotalPreferredSize(ICellLayout.LayoutGroup, axis))
                {
                    t = Mathf.Clamp01((rectSizeOnCurrAxis - RecyclerUtil.GetTotalMinSize(ICellLayout.LayoutGroup, axis))
                        / (RecyclerUtil.GetTotalPreferredSize(ICellLayout.LayoutGroup, axis)
                        - RecyclerUtil.GetTotalMinSize(ICellLayout.LayoutGroup, axis)));
                }
                float sizeRatio = 0f;
                if (rectSizeOnCurrAxis > RecyclerUtil.GetTotalPreferredSize(ICellLayout.LayoutGroup, axis)
                    && RecyclerUtil.GetTotalFlexibleSize(ICellLayout.LayoutGroup, axis) > 0f)
                {
                    sizeRatio = (rectSizeOnCurrAxis - RecyclerUtil.GetTotalPreferredSize(ICellLayout.LayoutGroup, axis))
                        / RecyclerUtil.GetTotalFlexibleSize(ICellLayout.LayoutGroup, axis);
                }
                for (int j = 0; j < ICellLayout.CellLayout.CellRecords.Count; j++)
                {
                    CellData currRecord = ICellLayout.CellLayout.CellRecords[j];

                    RectTransform childRect = (RectTransform)ICellLayout.CellLayout.CellPrefab.transform;
                    minSize = RecyclerUtil.GetMinSize(childRect, axis);
                    preferredSize = RecyclerUtil.GetPreferredSize(childRect, axis);
                    flexibleSize = RecyclerUtil.GetFlexibleSize(childRect, axis);

                    if ((axis != 0)
                        ? ((HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup).childForceExpandHeight
                        : ((HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup).childForceExpandWidth)
                    {
                        flexibleSize = Mathf.Max(flexibleSize, 1f);
                    }
                    float childSize = Mathf.Lerp(minSize, preferredSize, t);
                    childSize += flexibleSize * sizeRatio;
                    SetChildAlongAxis(currRecord.RectTransformData, axis, childPos, childSize);

                    childPos += childSize + ((HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup).spacing;
                }
            }
        }
    }

        // =========== HorizontalOrVerticalLayoutGroup calculations ===========
    public static void SetLayoutInput(
        HorizontalOrVerticalLayoutGroup layoutGroup,
        ICellLayout iCellLayout,
        int axis,
        bool isVertical,
        ref float totalMin,
        ref float totalPreferred,
        ref float totalFlexible)
    {
        float padding = ((axis != 0) ? layoutGroup.padding.vertical : layoutGroup.padding.horizontal);
        bool flag = isVertical ^ (axis == 1);

        totalMin = padding;
        totalPreferred = padding;
        totalFlexible = 0f;

        foreach (CellData cellRecord in iCellLayout.CellLayout.CellRecords)
        {
            RectTransformData currRect = cellRecord.RectTransformData;

            if (cellRecord.LayoutData == null)
            {
                cellRecord.LayoutData = iCellLayout.CellLayout.PrecalculateCellLayoutData(cellRecord);
            }

            float minSize = RecyclerUtil.GetMinSize(cellRecord.LayoutData, axis);
            float preferredSize = RecyclerUtil.GetPreferredSize(cellRecord.LayoutData, axis);
            float flexibleSize = RecyclerUtil.GetFlexibleSize(cellRecord.LayoutData, axis);

            if ((axis != 0) ? layoutGroup.childForceExpandHeight : layoutGroup.childForceExpandWidth)
            {
                flexibleSize = Mathf.Max(flexibleSize, 1f);
            }

            if (flag)
            {
                totalMin = Mathf.Max(minSize + padding, totalMin);
                totalPreferred = Mathf.Max(preferredSize + padding, totalPreferred);
                totalFlexible = Mathf.Max(flexibleSize, totalFlexible);
            }
            else
            {
                totalMin += minSize + layoutGroup.spacing;
                totalPreferred += preferredSize + layoutGroup.spacing;
                totalFlexible += flexibleSize;
            }
        }

        if (!flag && iCellLayout.CellLayout.CellRecords.Count > 0)
        {
            totalMin -= layoutGroup.spacing;
            totalPreferred -= layoutGroup.spacing;
        }

        totalPreferred = Mathf.Max(totalMin, totalPreferred);
    }

    public static void SetAllCellsDimensions(
        HorizontalOrVerticalLayoutGroup layoutGroup,
        ICellLayout cellLayout,
        int axis,
        bool isVertical)
    {
        var cellLayoutRect = (RectTransform)layoutGroup.transform;
        CellLayout cellLayoutData = cellLayout.CellLayout;

        float rectSizeOnCurrAxis = cellLayoutRect.rect.size[axis];
        bool flag = isVertical ^ (axis == 1);

        if (flag)
        {
            float value = rectSizeOnCurrAxis - ((axis != 0)
                ? layoutGroup.padding.vertical
                : layoutGroup.padding.horizontal);

            foreach (CellData cellRecord in cellLayoutData.CellRecords)
            {
                RectTransformData currRect = cellRecord.RectTransformData;

                float minSize = RecyclerUtil.GetMinSize(cellRecord.LayoutData, axis);
                float preferredSize = RecyclerUtil.GetPreferredSize(cellRecord.LayoutData, axis);
                float flexibleSize = RecyclerUtil.GetFlexibleSize(cellRecord.LayoutData, axis);

                if ((axis != 0) ? layoutGroup.childForceExpandHeight : layoutGroup.childForceExpandWidth)
                {
                    flexibleSize = Mathf.Max(flexibleSize, 1f);
                }

                float childSize = Mathf.Clamp
                    (value, minSize, 
                    (flexibleSize <= 0f) ? preferredSize : rectSizeOnCurrAxis);
                float startOffset = RecyclerUtil.GetStartOffset(layoutGroup, axis, childSize);

                SetChildAlongAxis(currRect, axis, startOffset, childSize);
            }
        }
        else
        {
            float childPos = ((axis != 0) ? layoutGroup.padding.top : layoutGroup.padding.left);

            if (RecyclerUtil.GetTotalFlexibleSize(layoutGroup, axis) == 0f
                && RecyclerUtil.GetTotalPreferredSize(layoutGroup, axis) < rectSizeOnCurrAxis)
            {
                childPos = RecyclerUtil.GetStartOffset(layoutGroup, axis,
                    RecyclerUtil.GetTotalPreferredSize(layoutGroup, axis) - ((axis != 0)
                    ? layoutGroup.padding.vertical
                    : layoutGroup.padding.horizontal));
            }

            float minToPreferredRatio = 0f;

            if (RecyclerUtil.GetTotalMinSize(layoutGroup, axis) 
                != RecyclerUtil.GetTotalPreferredSize(layoutGroup, axis))
            {
                float rectSizeAfterMinSize =
                    rectSizeOnCurrAxis - RecyclerUtil.GetTotalMinSize(layoutGroup, axis);
                float deltaBetweenMinAndPreferred =
                    RecyclerUtil.GetTotalPreferredSize(layoutGroup, axis)
                    - RecyclerUtil.GetTotalMinSize(layoutGroup, axis);

                minToPreferredRatio = Mathf.Clamp01(rectSizeAfterMinSize / deltaBetweenMinAndPreferred);
            }

            float sizeRatio = 0f;

            if (rectSizeOnCurrAxis > RecyclerUtil.GetTotalPreferredSize(layoutGroup, axis)
                && RecyclerUtil.GetTotalFlexibleSize(layoutGroup, axis) > 0f)
            {
                sizeRatio = (rectSizeOnCurrAxis - RecyclerUtil.GetTotalPreferredSize(layoutGroup, axis))
                    / RecyclerUtil.GetTotalFlexibleSize(layoutGroup, axis);
            }

            foreach (CellData cellRecord in cellLayoutData.CellRecords)
            {
                RectTransformData currRect = cellRecord.RectTransformData;

                float minSize = RecyclerUtil.GetMinSize(cellRecord.LayoutData, axis);
                float preferredSize = RecyclerUtil.GetPreferredSize(cellRecord.LayoutData, axis);
                float flexibleSize = RecyclerUtil.GetFlexibleSize(cellRecord.LayoutData, axis);

                if ((axis != 0) ? layoutGroup.childForceExpandHeight : layoutGroup.childForceExpandWidth)
                {
                    flexibleSize = Mathf.Max(flexibleSize, 1f);
                }

                float childSize = Mathf.Lerp(minSize, preferredSize, minToPreferredRatio);
                childSize += flexibleSize * sizeRatio;

                // RectTransformDimensions for record set here
                SetChildAlongAxis(currRect, axis, childPos, childSize);
                childPos += childSize + layoutGroup.spacing;
            }
        }
    }
    public static void SetChildAlongAxis(RectTransformData rectDims, int axis, float pos, float size)
    {
        rectDims.SetInsetAndSizeFromParentEdge(
            (axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left,
            pos,
            size);
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
