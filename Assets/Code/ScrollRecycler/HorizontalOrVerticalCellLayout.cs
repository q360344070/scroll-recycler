using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

// Used for fields and methods on the CellLayouts because inheriting Unity API classes is messy
[Serializable]
public class HorizontalOrVerticalCellLayout : CellLayout
{
    public LayoutDimensions GetLayoutDimensions(LayoutAxis axis)
    {
        var hovLayoutGroup = (HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup;
        bool isVertical = hovLayoutGroup is VerticalLayoutGroup;
        float layoutPadding = (axis != 0) ? hovLayoutGroup.padding.vertical : hovLayoutGroup.padding.horizontal;
        bool initialAxis = isVertical ^ (axis == LayoutAxis.Vertical);
        var layoutDimensions = new LayoutDimensions() { Min = layoutPadding, Preferred = layoutPadding, Flexible = 0f };

        foreach (CellData cellRecord in CellRecords)
        {
            bool cellNeedsLayoutData = 
                (!cellRecord.LayoutData.DimensionsHorizontalSet && axis == LayoutAxis.Horizontal)
                || (!cellRecord.LayoutData.DimensionsVerticalSet && axis == LayoutAxis.Vertical);

            //if (cellNeedsLayoutData)
            //{
                cellRecord.LayoutData.SetDimensions(PrecalculateCellLayoutData(cellRecord, axis), axis);
            //}

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

    public void SetCellsDimensions(LayoutAxis axis)
    {
        var hovLayoutGroup = (HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup;
        float rectSizeOnCurrAxis = ((RectTransform)hovLayoutGroup.transform).rect.size[(int)axis];
        bool initialAxis = (hovLayoutGroup is VerticalLayoutGroup) ^ (axis == LayoutAxis.Vertical);
        float layoutPadding = (axis != 0) ? hovLayoutGroup.padding.vertical : hovLayoutGroup.padding.horizontal;
        float layoutSizeWithoutPadding = rectSizeOnCurrAxis - layoutPadding;
        bool layoutChildForceExpand = 
            (axis != 0) ? hovLayoutGroup.childForceExpandHeight : hovLayoutGroup.childForceExpandWidth;

        if (initialAxis)
        {
            foreach (CellData cellRecord in CellRecords)
            {
                float minSize = RecyclerUtil.GetMinSize(cellRecord.LayoutData, (int)axis);
                float preferredSize = RecyclerUtil.GetPreferredSize(cellRecord.LayoutData, (int)axis);
                float flexibleSize = RecyclerUtil.GetFlexibleSize(cellRecord.LayoutData, (int)axis);

                if (layoutChildForceExpand)
                {
                    flexibleSize = Mathf.Max(flexibleSize, 1f);
                }

                float childSize = Mathf.Clamp(
                    layoutSizeWithoutPadding, 
                    minSize,
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
            float layoutTotalMinSize = RecyclerUtil.GetTotalMinSize(hovLayoutGroup, (int)axis);
            float layoutTotalPreferredSize = RecyclerUtil.GetTotalPreferredSize(hovLayoutGroup, (int)axis);
            float layoutTotalFlexibleSize = RecyclerUtil.GetTotalFlexibleSize(hovLayoutGroup, (int)axis);

            if (layoutTotalFlexibleSize == 0f && layoutTotalPreferredSize < rectSizeOnCurrAxis)
            {
                childPos = RecyclerUtil.GetStartOffset(hovLayoutGroup, (int)axis, layoutPadding);
            }

            float minToPreferredRatio = 0f;

            if (layoutTotalMinSize != layoutTotalPreferredSize)
            {
                float rectSizeAfterMinSize = rectSizeOnCurrAxis - layoutTotalMinSize;
                float deltaBetweenMinAndPreferred = layoutTotalPreferredSize - layoutTotalMinSize;

                minToPreferredRatio = Mathf.Clamp01(rectSizeAfterMinSize / deltaBetweenMinAndPreferred);
            }

            float remainingSizeRatio = 0f;
            if (rectSizeOnCurrAxis > layoutTotalPreferredSize && layoutTotalFlexibleSize > 0f)
            {
                remainingSizeRatio = (rectSizeOnCurrAxis - layoutTotalPreferredSize) / layoutTotalFlexibleSize;
            }

            foreach (CellData cellRecord in CellRecords)
            {
                float minSize = RecyclerUtil.GetMinSize(cellRecord.LayoutData, (int)axis);
                float preferredSize = RecyclerUtil.GetPreferredSize(cellRecord.LayoutData, (int)axis);
                float flexibleSize = RecyclerUtil.GetFlexibleSize(cellRecord.LayoutData, (int)axis);

                if (layoutChildForceExpand)
                {
                    flexibleSize = Mathf.Max(flexibleSize, 1f);
                }

                float childSize = Mathf.Lerp(minSize, preferredSize, minToPreferredRatio);
                childSize += flexibleSize * remainingSizeRatio;

                cellRecord.RectTransformData.SetInsetAndSizeFromParentEdge(
                    (axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left,
                    childPos,
                    childSize);

                childPos += childSize + hovLayoutGroup.spacing;
            }
        }
    }

    // NOTE: This is an experimental method, DO NOT invoke in more places than already invoked
    public void PlaceCell(RectTransform cellRtx, LayoutAxis axis)
    {
        var hovLayoutGroup = (HorizontalOrVerticalLayoutGroup)ICellLayout.LayoutGroup;
        float rectSizeOnCurrAxis = ((RectTransform)hovLayoutGroup.transform).rect.size[(int)axis];
        bool initialAxis = (hovLayoutGroup is VerticalLayoutGroup) ^ (axis == LayoutAxis.Vertical);
        float layoutPadding = (axis != 0) ? hovLayoutGroup.padding.vertical : hovLayoutGroup.padding.horizontal;
        float layoutSizeWithoutPadding = rectSizeOnCurrAxis - layoutPadding;
        bool layoutChildForceExpand = 
            (axis != 0) ? hovLayoutGroup.childForceExpandHeight : hovLayoutGroup.childForceExpandWidth;

        if (initialAxis)
        {
            {
                float minSize = LayoutUtility.GetMinSize(cellRtx, (int)axis);
                float preferredSize = LayoutUtility.GetPreferredSize(cellRtx, (int)axis);
                float flexibleSize = LayoutUtility.GetFlexibleSize(cellRtx, (int)axis);

                if (layoutChildForceExpand)
                {
                    flexibleSize = Mathf.Max(flexibleSize, 1f);
                }

                float childSize = Mathf.Clamp(
                    layoutSizeWithoutPadding, 
                    minSize,
                    (flexibleSize <= 0f) ? preferredSize : rectSizeOnCurrAxis);
                float startOffset = RecyclerUtil.GetStartOffset(hovLayoutGroup, (int)axis, childSize);

                cellRtx.SetInsetAndSizeFromParentEdge(
                    (axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left,
                    startOffset,
                    childSize);
            }
        }
        else
        {
            float childPos = ((axis != 0) ? hovLayoutGroup.padding.top : hovLayoutGroup.padding.left);
            float layoutTotalMinSize = RecyclerUtil.GetTotalMinSize(hovLayoutGroup, (int)axis);
            float layoutTotalPreferredSize = RecyclerUtil.GetTotalPreferredSize(hovLayoutGroup, (int)axis);
            float layoutTotalFlexibleSize = RecyclerUtil.GetTotalFlexibleSize(hovLayoutGroup, (int)axis);

            if (layoutTotalFlexibleSize == 0f && layoutTotalPreferredSize < rectSizeOnCurrAxis)
            {
                childPos = RecyclerUtil.GetStartOffset(hovLayoutGroup, (int)axis, layoutPadding);
            }

            float minToPreferredRatio = 0f;

            if (layoutTotalMinSize != layoutTotalPreferredSize)
            {
                float rectSizeAfterMinSize = rectSizeOnCurrAxis - layoutTotalMinSize;
                float deltaBetweenMinAndPreferred = layoutTotalPreferredSize - layoutTotalMinSize;

                minToPreferredRatio = Mathf.Clamp01(rectSizeAfterMinSize / deltaBetweenMinAndPreferred);
            }

            float remainingSizeRatio = 0f;
            if (rectSizeOnCurrAxis > layoutTotalPreferredSize && layoutTotalFlexibleSize > 0f)
            {
                remainingSizeRatio = (rectSizeOnCurrAxis - layoutTotalPreferredSize) / layoutTotalFlexibleSize;
            }

            {
                float minSize = LayoutUtility.GetMinSize(cellRtx, (int)axis);
                float preferredSize = LayoutUtility.GetPreferredSize(cellRtx, (int)axis);
                float flexibleSize = LayoutUtility.GetFlexibleSize(cellRtx, (int)axis);

                if (layoutChildForceExpand)
                {
                    flexibleSize = Mathf.Max(flexibleSize, 1f);
                }

                float childSize = Mathf.Lerp(minSize, preferredSize, minToPreferredRatio);
                childSize += flexibleSize * remainingSizeRatio;

                cellRtx.SetInsetAndSizeFromParentEdge(
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

        LayoutAxis initialAxis = 
            (ICellLayout.LayoutGroup is VerticalCellLayout) ? LayoutAxis.Horizontal : LayoutAxis.Vertical;
        LayoutAxis secondaryAxis =
            (initialAxis == LayoutAxis.Horizontal) ? LayoutAxis.Vertical : LayoutAxis.Horizontal;

        var iLayout = CellPool.CellProxy.GetComponent<ILayoutElement>();
        Action<string> dbgPrint = (string msg) =>
        {
            Debug.Log(string.Format("{0} proxyRtx.rect.size = {1} preferredSize = {2} minSize = {3} flexibleSize = {4}", 
                msg, 
                proxyRtx.rect.size, 
                new Vector2(iLayout.preferredWidth, iLayout.preferredHeight),
                new Vector2(iLayout.minWidth, iLayout.minHeight),
                new Vector2(iLayout.flexibleWidth, iLayout.flexibleHeight)));
        };

        PlaceCell(proxyRtx, initialAxis);
        dbgPrint("Initial Axis: ");
        PlaceCell(proxyRtx, secondaryAxis);
        dbgPrint("Secondary Axis: ");

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
        bool layoutChildForceExpand = (axis != 0) 
            ? hovLayoutGroup.childForceExpandHeight 
            : hovLayoutGroup.childForceExpandWidth;
        float layoutPadding = (axis != 0) 
            ? ICellLayout.LayoutGroup.padding.vertical 
            : ICellLayout.LayoutGroup.padding.horizontal;

        if (ICellLayout.CellLayout != null && ICellLayout.CellLayout.CellPrefab)
        {
            float rectSizeOnCurrAxis = rectTransform.rect.size[(int)axis];
            bool initialAxis = isVertical ^ axis == LayoutAxis.Vertical;

            if (initialAxis)
            {
                for (int i = 0; i < ICellLayout.CellLayout.CellRecords.Count; i++)
                {
                    CellData currRecord = ICellLayout.CellLayout.CellRecords[i];

                    layoutDims.Min = RecyclerUtil.GetMinSize(cellRtx, (int)axis);
                    layoutDims.Preferred = RecyclerUtil.GetPreferredSize(cellRtx, (int)axis);
                    layoutDims.Flexible = RecyclerUtil.GetFlexibleSize(cellRtx, (int)axis);

                    if (layoutChildForceExpand)
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
                float childPos = layoutPadding;

                float totalLayoutMinSize = RecyclerUtil.GetTotalMinSize(ICellLayout.LayoutGroup, (int)axis);
                float totalLayoutPreferredSize = RecyclerUtil.GetTotalPreferredSize(ICellLayout.LayoutGroup, (int)axis);
                float totalLayoutFlexibleSize = RecyclerUtil.GetTotalFlexibleSize(ICellLayout.LayoutGroup, (int)axis);
                bool layoutRectLargeEnoughForPreferred =  totalLayoutPreferredSize < rectSizeOnCurrAxis;

                if ((totalLayoutFlexibleSize == 0f) && layoutRectLargeEnoughForPreferred)
                {
                    float requiredSpaceWithoutPadding = totalLayoutPreferredSize - layoutPadding;

                    childPos = RecyclerUtil.GetStartOffset(
                        ICellLayout.LayoutGroup, 
                        (int)axis, 
                        requiredSpaceWithoutPadding);
                }

                float flexibleScalar = 0f;

                if (!(totalLayoutMinSize == totalLayoutPreferredSize))
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

                    if (layoutChildForceExpand)
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
