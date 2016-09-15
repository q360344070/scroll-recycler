using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

// Used for fields and methods on the CellLayouts because inheriting Unity API classes is messy
[Serializable]
public class HorizontalOrVerticalCellLayout : CellLayout
{
    public LayoutDimensions SetLayoutInput(LayoutAxis axis)
    {
        var hovLayoutGroup = (HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup;
        bool isVertical = hovLayoutGroup is VerticalLayoutGroup;
        var layoutDimensions = new LayoutDimensions();

        float layoutPadding = (axis != 0) ? hovLayoutGroup.padding.vertical : hovLayoutGroup.padding.horizontal;
        bool initialAxis = isVertical ^ (axis == LayoutAxis.Vertical);

        layoutDimensions.Min = layoutPadding;
        layoutDimensions.Preferred = layoutPadding;
        layoutDimensions.Flexible = 0f;

        foreach (CellData cellRecord in CellRecords)
        {
            if (axis == LayoutAxis.Horizontal)
            {
                if (!cellRecord.LayoutDataCalculatedHorizontal)
                {
                    cellRecord.LayoutData.SetWidth(PrecalculateCellLayoutData(cellRecord, LayoutAxis.Horizontal));
                    cellRecord.LayoutDataCalculatedHorizontal = true;
                }
            }
            else
            {
                if (!cellRecord.LayoutDataCalculatedVertical)
                {
                    cellRecord.LayoutData.SetHeight(PrecalculateCellLayoutData(cellRecord, LayoutAxis.Vertical));
                    cellRecord.LayoutDataCalculatedVertical = true;
                }
            }

            float minSize = RecyclerUtil.GetMinSize(cellRecord.LayoutData, (int)axis);
            float preferredSize = RecyclerUtil.GetPreferredSize(cellRecord.LayoutData, (int)axis);
            float flexibleSize = RecyclerUtil.GetFlexibleSize(cellRecord.LayoutData, (int)axis);

            if ((axis != 0) ? hovLayoutGroup.childForceExpandHeight : hovLayoutGroup.childForceExpandWidth)
            {
                flexibleSize = Mathf.Max(flexibleSize, 1f);
            }

            if (initialAxis)
            {
                layoutDimensions.Min = Mathf.Max(minSize + layoutPadding, layoutDimensions.Min);
                layoutDimensions.Preferred = Mathf.Max(preferredSize + layoutPadding, layoutDimensions.Preferred);
                layoutDimensions.Flexible = Mathf.Max(flexibleSize, layoutDimensions.Flexible);
            }
            else
            {
                layoutDimensions.Min += minSize + hovLayoutGroup.spacing;
                layoutDimensions.Preferred += preferredSize + hovLayoutGroup.spacing;
                layoutDimensions.Flexible += flexibleSize;
            }
        }

        if (!initialAxis && CellRecords.Count > 0)
        {
            layoutDimensions.Min -= hovLayoutGroup.spacing;
            layoutDimensions.Preferred -= hovLayoutGroup.spacing;
        }

        layoutDimensions.Preferred = Mathf.Max(layoutDimensions.Min, layoutDimensions.Preferred);
        return layoutDimensions;
    }

    public void SetAllCellsDimensions(LayoutAxis axis)
    {
        var hovLayoutGroup = (HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup;
        var cellLayoutRect = (RectTransform)hovLayoutGroup.transform;
        bool isVertical = hovLayoutGroup is VerticalLayoutGroup;

        float rectSizeOnCurrAxis = cellLayoutRect.rect.size[(int)axis];
        bool initialAxis = isVertical ^ (axis == LayoutAxis.Vertical);

        if (initialAxis)
        {
            float value = rectSizeOnCurrAxis - ((axis != 0)
                ? hovLayoutGroup.padding.vertical
                : hovLayoutGroup.padding.horizontal);

            foreach (CellData cellRecord in CellRecords)
            {
                float minSize = RecyclerUtil.GetMinSize(cellRecord.LayoutData, (int)axis);
                float preferredSize = RecyclerUtil.GetPreferredSize(cellRecord.LayoutData, (int)axis);
                float flexibleSize = RecyclerUtil.GetFlexibleSize(cellRecord.LayoutData, (int)axis);

                if ((axis != 0) ? hovLayoutGroup.childForceExpandHeight : hovLayoutGroup.childForceExpandWidth)
                {
                    flexibleSize = Mathf.Max(flexibleSize, 1f);
                }

                float childSize = Mathf.Clamp
                    (value, minSize,
                    (flexibleSize <= 0f) ? preferredSize : rectSizeOnCurrAxis);
                float startOffset = RecyclerUtil.GetStartOffset(hovLayoutGroup, (int)axis, childSize);

                cellRecord.RectTransformData.SetInsetAndSizeFromParentEdge(
                    (axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left,
                    startOffset,
                    childSize);
            }
        }
        else
        {
            float childPos = ((axis != 0) ? hovLayoutGroup.padding.top : hovLayoutGroup.padding.left);

            if (RecyclerUtil.GetTotalFlexibleSize(hovLayoutGroup, (int)axis) == 0f
                && RecyclerUtil.GetTotalPreferredSize(hovLayoutGroup, (int)axis) < rectSizeOnCurrAxis)
            {
                childPos = RecyclerUtil.GetStartOffset(hovLayoutGroup, (int)axis,
                    RecyclerUtil.GetTotalPreferredSize(hovLayoutGroup, (int)axis) - ((axis != 0)
                    ? hovLayoutGroup.padding.vertical
                    : hovLayoutGroup.padding.horizontal));
            }

            float minToPreferredRatio = 0f;

            if (RecyclerUtil.GetTotalMinSize(hovLayoutGroup, (int)axis)
                != RecyclerUtil.GetTotalPreferredSize(hovLayoutGroup, (int)axis))
            {
                float rectSizeAfterMinSize =
                    rectSizeOnCurrAxis - RecyclerUtil.GetTotalMinSize(hovLayoutGroup, (int)axis);
                float deltaBetweenMinAndPreferred =
                    RecyclerUtil.GetTotalPreferredSize(hovLayoutGroup, (int)axis)
                    - RecyclerUtil.GetTotalMinSize(hovLayoutGroup, (int)axis);

                minToPreferredRatio = Mathf.Clamp01(rectSizeAfterMinSize / deltaBetweenMinAndPreferred);
            }

            float sizeRatio = 0f;

            if (rectSizeOnCurrAxis > RecyclerUtil.GetTotalPreferredSize(hovLayoutGroup, (int)axis)
                && RecyclerUtil.GetTotalFlexibleSize(hovLayoutGroup, (int)axis) > 0f)
            {
                sizeRatio = (rectSizeOnCurrAxis - RecyclerUtil.GetTotalPreferredSize(hovLayoutGroup, (int)axis))
                    / RecyclerUtil.GetTotalFlexibleSize(hovLayoutGroup, (int)axis);
            }

            foreach (CellData cellRecord in CellRecords)
            {
                float minSize = RecyclerUtil.GetMinSize(cellRecord.LayoutData, (int)axis);
                float preferredSize = RecyclerUtil.GetPreferredSize(cellRecord.LayoutData, (int)axis);
                float flexibleSize = RecyclerUtil.GetFlexibleSize(cellRecord.LayoutData, (int)axis);

                if ((axis != 0) ? hovLayoutGroup.childForceExpandHeight : hovLayoutGroup.childForceExpandWidth)
                {
                    flexibleSize = Mathf.Max(flexibleSize, 1f);
                }

                float childSize = Mathf.Lerp(minSize, preferredSize, minToPreferredRatio);
                childSize += flexibleSize * sizeRatio;

                // RectTransformDimensions for record set here
                cellRecord.RectTransformData.SetInsetAndSizeFromParentEdge(
                    (axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left,
                    childPos,
                    childSize);
                childPos += childSize + hovLayoutGroup.spacing;
            }
        }
    }


    public LayoutDimensions PrecalculateCellLayoutData(CellData cellData, LayoutAxis axis)
    {
        // Set up the proxy cell to reflect the same layout and rect as our cell would be instantiated as
        var recyclerCell = CellPool.CellProxy.GetComponent<IRecyclableCell>();
        var proxyRtx = (RectTransform)CellPool.CellProxy.transform;
        recyclerCell.OnCellInstantiate();
        recyclerCell.OnCellShow(cellData);
        LayoutRebuilder.ForceRebuildLayoutImmediate(proxyRtx);

        return GetProxyLayoutDimensions(axis);
    }

    public void SetProxyRect(int axis, float inset, float size)
    {
        ((RectTransform)CellPool.CellProxy.transform).SetInsetAndSizeFromParentEdge(
            (axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left,
            inset,
            size);
    }

    LayoutDimensions GetProxyLayoutDimensions(LayoutAxis axis)
    {
        return GetCellLayoutDimensions((RectTransform)CellPool.CellProxy.transform, axis);
    }

    LayoutDimensions GetCellLayoutDimensions(RectTransform cellRtx, LayoutAxis axis)
    {
        var layoutDims = new LayoutDimensions();
        var rectTransform = (RectTransform)ICellLayout.LayoutGroup.transform;
        var hovLayoutGroup = (HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup;
        bool isVertical = hovLayoutGroup is VerticalLayoutGroup;

        if (ICellLayout.CellLayout != null && ICellLayout.CellLayout.CellPrefab)
        {
            float rectSizeOnCurrAxis = rectTransform.rect.size[(int)axis];
            bool initialAxis = isVertical ^ axis == LayoutAxis.Vertical;

            float layoutPadding = rectSizeOnCurrAxis -
                ((axis != 0)
                ? ICellLayout.LayoutGroup.padding.vertical
                : ICellLayout.LayoutGroup.padding.horizontal);

            if (initialAxis)
            {
                for (int i = 0; i < ICellLayout.CellLayout.CellRecords.Count; i++)
                {
                    CellData currRecord = ICellLayout.CellLayout.CellRecords[i];

                    layoutDims.Min = RecyclerUtil.GetMinSize(cellRtx, (int)axis);
                    layoutDims.Preferred = RecyclerUtil.GetPreferredSize(cellRtx, (int)axis);
                    layoutDims.Flexible = RecyclerUtil.GetFlexibleSize(cellRtx, (int)axis);

                    if ((axis != 0)
                        ? hovLayoutGroup.childForceExpandHeight
                        : hovLayoutGroup.childForceExpandWidth)
                    {
                        layoutDims.Flexible = Mathf.Max(layoutDims.Flexible, 1f);
                    }

                    float childSize = Mathf.Clamp(
                        layoutPadding,
                        layoutDims.Min,
                        (layoutDims.Flexible <= 0f) ? layoutDims.Preferred : rectSizeOnCurrAxis);

                    float startOffset = RecyclerUtil.GetStartOffset(ICellLayout.LayoutGroup, (int)axis, childSize);

                    currRecord.RectTransformData.SetInsetAndSizeFromParentEdge(
                        (axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left,
                        startOffset,
                        childSize);
                }
            }
            else
            {
                float childPos =
                    ((axis != 0)
                    ? ICellLayout.LayoutGroup.padding.top
                    : ICellLayout.LayoutGroup.padding.left);

                float totalLayoutMinSize = RecyclerUtil.GetTotalMinSize(ICellLayout.LayoutGroup, (int)axis);
                float totalLayoutPreferredSize = RecyclerUtil.GetTotalPreferredSize(ICellLayout.LayoutGroup, (int)axis);
                float totalLayoutFlexibleSize = RecyclerUtil.GetTotalFlexibleSize(ICellLayout.LayoutGroup, (int)axis);

                bool flexibleSizeDisabled = totalLayoutFlexibleSize == 0f;

                bool layoutRectLargeEnoughForPreferred =  totalLayoutPreferredSize < rectSizeOnCurrAxis;

                if (flexibleSizeDisabled && layoutRectLargeEnoughForPreferred)
                {
                    float requiredSpaceWithoutPadding = totalLayoutPreferredSize - layoutPadding;

                    childPos = RecyclerUtil.GetStartOffset(
                        ICellLayout.LayoutGroup, 
                        (int)axis, 
                        requiredSpaceWithoutPadding);
                }

                float flexibleScalar = 0f;

                bool minSizeIsPreferredSize = totalLayoutMinSize == totalLayoutPreferredSize;

                if (!minSizeIsPreferredSize)
                {
                    flexibleScalar = Mathf.Clamp01(
                        (rectSizeOnCurrAxis - totalLayoutMinSize)
                            / totalLayoutPreferredSize
                            - totalLayoutMinSize);
                }

                float sizeRatio = 0f;
                if (rectSizeOnCurrAxis > totalLayoutPreferredSize && totalLayoutFlexibleSize > 0f)
                {
                    sizeRatio = (rectSizeOnCurrAxis - totalLayoutPreferredSize) / totalLayoutFlexibleSize;
                }

                for (int j = 0; j < ICellLayout.CellLayout.CellRecords.Count; j++)
                {
                    CellData currRecord = ICellLayout.CellLayout.CellRecords[j];

                    RectTransform childRect = (RectTransform)ICellLayout.CellLayout.CellPrefab.transform;
                    layoutDims.Min = RecyclerUtil.GetMinSize(childRect, (int)axis);
                    layoutDims.Preferred = RecyclerUtil.GetPreferredSize(childRect, (int)axis);
                    layoutDims.Flexible = RecyclerUtil.GetFlexibleSize(childRect, (int)axis);

                    bool forceExpand = (axis != 0)
                        ? hovLayoutGroup.childForceExpandHeight
                        : hovLayoutGroup.childForceExpandWidth;

                    if (forceExpand)
                    {
                        layoutDims.Flexible = Mathf.Max(layoutDims.Flexible, 1f);
                    }

                    float childSize = Mathf.Lerp(layoutDims.Min, layoutDims.Preferred, flexibleScalar)
                        + (layoutDims.Flexible * sizeRatio);

                    currRecord.RectTransformData.SetInsetAndSizeFromParentEdge(
                        (axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left,
                        childPos,
                        childSize);

                    childPos += childSize + hovLayoutGroup.spacing;
                }
            }
        }

        return layoutDims;
    }
}
