using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public static class LayoutUtil
{
    public static void DuplicateLayoutElements(GameObject source, GameObject destination)
    {
        Component[] components = source.GetComponents<Component>();

        // This layout elements are the types which will contribute to the LayoutDimensions of the RectTransform
        foreach (Component mb in components)
        {
            destination.TryCopyComponent<GridLayoutGroup>(mb);
            destination.TryCopyComponent<VerticalLayoutGroup>(mb);
            destination.TryCopyComponent<HorizontalLayoutGroup>(mb);
            destination.TryCopyComponent<ContentSizeFitter>(mb);
            destination.TryCopyComponent<ScrollRect>(mb);
        }
    }

    public static void SetChildAlongAxis(RectTransformDimensions rectDims, int axis, float pos, float size)
    {
        if (rectDims == null)
        {
            return;
        }

        rectDims.SetInsetAndSizeFromParentEdge((axis != 0) ? RectTransform.Edge.Top : RectTransform.Edge.Left, pos, size);
    }

    public static float GetMinSize(RectTransform rect, int axis, CellRecord cellRecord)
    {
        return (axis == 0) ? GetMinWidth(rect) : GetMinHeight(rect);
    }

    public static float GetPreferredSize(RectTransform rect, int axis, CellRecord cellRecord)
    {
        return (axis == 0) ? GetPreferredWidth(rect) : GetPreferredHeight(rect);
    }

    public static float GetFlexibleSize(RectTransform rect, int axis)
    {
        return (axis == 0) ? GetFlexibleWidth(rect) : GetFlexibleHeight(rect);
    }

    public static float GetMinWidth(RectTransform rect)
    {
        return GetLayoutProperty(rect, (ILayoutElement e) => e.minWidth, 0f);
    }

    public static float GetPreferredWidth(RectTransform rect)
    {
        float minWidth = GetLayoutProperty(rect, (ILayoutElement e) => e.minWidth, 0f);
        float preferredWidth = GetLayoutProperty(rect, (ILayoutElement e) => e.preferredWidth, 0f);
        return Mathf.Max(minWidth, preferredWidth);
    }

    public static float GetFlexibleWidth(RectTransform rect)
    {
        return GetLayoutProperty(rect, (ILayoutElement e) => e.flexibleWidth, 0f);
    }

    public static float GetMinHeight(RectTransform rect)
    {
        return GetLayoutProperty(rect, (ILayoutElement e) => e.minHeight, 0f);
    }

    public static float GetPreferredHeight(RectTransform rect)
    {
        float minHeight = GetLayoutProperty(rect, (ILayoutElement e) => e.minHeight, 0f);
        float preferredHeight = GetLayoutProperty(rect, (ILayoutElement e) => e.preferredHeight, 0f);

        return Mathf.Max(minHeight, preferredHeight);
    }

    public static float GetFlexibleHeight(RectTransform rect)
    {
        return GetLayoutProperty(rect, (ILayoutElement e) => e.flexibleHeight, 0f);
    }

    public static float GetLayoutProperty(RectTransform rect, Func<ILayoutElement, float> property, float defaultValue)
    {
        ILayoutElement layoutElement;
        return GetLayoutProperty(rect, property, defaultValue, out layoutElement);
    }

    public static float GetLayoutProperty(RectTransform rect, Func<ILayoutElement, float> property, float defaultValue, out ILayoutElement source)
    {
        source = null;
        if (rect == null)
        {
            return 0f;
        }
        float num = defaultValue;
        int num2 = -2147483648;
        List<Component> list = ListPool<Component>.Get();
        rect.GetComponents(typeof(ILayoutElement), list);

        for (int i = 0; i < list.Count; i++)
        {
            ILayoutElement layoutElement = list[i] as ILayoutElement;

            // Original check only allows behaviors to trigger if isActiveAndEnabled
            //if (!(layoutElement is Behaviour) || ((Behaviour)layoutElement).isActiveAndEnabled)
            if (layoutElement != null)
            {
                int layoutPriority = layoutElement.layoutPriority;
                if (layoutPriority >= num2)
                {
                    float propertyValue = property(layoutElement);
                    if (propertyValue >= 0f)
                    {
                        if (layoutPriority > num2)
                        {
                            num = propertyValue;
                            num2 = layoutPriority;
                            source = layoutElement;
                        }
                        else if (propertyValue > num)
                        {
                            num = propertyValue;
                            source = layoutElement;
                        }
                    }
                }
            }
        }
        ListPool<Component>.Release(list);
        return num;
    }

    public static float GetMinSize(LayoutDimensions dims, int axis)
    {
        return (axis == 0) ? dims.minWidth : dims.minHeight;
    }

    public static float GetPreferredSize(LayoutDimensions dims, int axis)
    {
        return (axis == 0) ? dims.preferredWidth : dims.preferredHeight;
    }

    public static float GetFlexibleSize(LayoutDimensions dims, int axis)
    {
        return (axis == 0) ? dims.flexibleWidth : dims.flexibleHeight;
    }

    //==================================================================================================================
    // LayoutGroup forks
    public static float GetTotalFlexibleSize(LayoutGroup layoutGroup, int axis)
    {
        return (axis == 0) ? layoutGroup.flexibleWidth : layoutGroup.flexibleHeight;
    }

    public static float GetTotalPreferredSize(LayoutGroup layoutGroup, int axis)
    {
        return (axis == 0) ? layoutGroup.preferredWidth : layoutGroup.preferredHeight;
    }

    public static float GetTotalMinSize(LayoutGroup layoutGroup, int axis)
    {
        return (axis == 0) ? layoutGroup.minWidth : layoutGroup.minHeight;
    }

    public static float GetStartOffset(LayoutGroup layoutGroup, int axis, float requiredSpaceWithoutPadding)
    {
        var rectTransform = (RectTransform)layoutGroup.transform;
        float num = requiredSpaceWithoutPadding + (float)((axis != 0)
            ? layoutGroup.padding.vertical
            : layoutGroup.padding.horizontal);
        float num2 = rectTransform.rect.size[axis];
        float num3 = num2 - num;
        float num4;
        if (axis == 0)
        {
            num4 = (float)((int)layoutGroup.childAlignment % (int)TextAnchor.MiddleLeft) * 0.5f;
        }
        else
        {
            num4 = (float)((int)layoutGroup.childAlignment / (int)TextAnchor.MiddleLeft) * 0.5f;
        }
        return (float)((axis != 0)
            ? (int)layoutGroup.padding.top
            : (int)layoutGroup.padding.left) + num3 * num4;
    }
    // LayoutGroup forks
    //==================================================================================================================

    class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, delegate (List<T> l)
        {
            l.Clear();
        });

        public static List<T> Get()
        {
            return s_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            s_ListPool.Release(toRelease);
        }
    }

    class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_Stack = new Stack<T>();

        private readonly UnityAction<T> m_ActionOnGet;

        private readonly UnityAction<T> m_ActionOnRelease;

        public int countAll
        {
            get;
            private set;
        }

        public int countActive
        {
            get
            {
                return countAll - countInactive;
            }
        }

        public int countInactive
        {
            get
            {
                return m_Stack.Count;
            }
        }

        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        public T Get()
        {
            T t;
            if (m_Stack.Count == 0)
            {
                t = ((default(T) == null) ? Activator.CreateInstance<T>() : default(T));
                countAll++;
            }
            else
            {
                t = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
            {
                m_ActionOnGet(t);
            }
            return t;
        }

        public void Release(T element)
        {
            if (m_Stack.Count > 0 && object.ReferenceEquals(m_Stack.Peek(), element))
            {
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }
            if (m_ActionOnRelease != null)
            {
                m_ActionOnRelease(element);
            }
            m_Stack.Push(element);
        }
    }

    //==================================================================================================================
    // HorizontalOrVerticalLayoutGroup calculations
    public static void CalcAlongAxisRecycler(
        HorizontalOrVerticalLayoutGroup layoutGroup,
        IRecyclableLayout iRecyclerLayout, 
        int axis, 
        bool isVertical, 
        ref float totalMin, 
        ref float totalPreferred,
        ref float totalFlexible)
    {
        LayoutRecycler recyclerLayout = iRecyclerLayout.GetLayoutRecycler();

        if (recyclerLayout != null && recyclerLayout.CellPrefab)
        {
            float padding = ((axis != 0) ? layoutGroup.padding.vertical : layoutGroup.padding.horizontal);
            totalMin = padding;
            totalPreferred = padding;
            totalFlexible = 0f;
            bool flag = isVertical ^ (axis == 1);

            for (int i = 0; i < recyclerLayout.CellRectTransformDimensions.Count; i++)
            {
                RectTransformDimensions currRect = recyclerLayout.CellRectTransformDimensions[i];

                if (currRect.LayoutDimensions == null)
                {
                    currRect.LayoutDimensions =
                        CalculateLayoutDimensionsFromCellInstance(recyclerLayout.CellPool.CellLayoutProxy, currRect);
                }

                float minSize = LayoutUtil.GetMinSize(currRect.LayoutDimensions, axis);
                float preferredSize = LayoutUtil.GetPreferredSize(currRect.LayoutDimensions, axis);
                float flexibleSize = LayoutUtil.GetFlexibleSize(currRect.LayoutDimensions, axis);

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

            if (!flag && recyclerLayout.CellRecords.Count > 0)
            {
                totalMin -= layoutGroup.spacing;
                totalPreferred -= layoutGroup.spacing;
            }

            totalPreferred = Mathf.Max(totalMin, totalPreferred);
        }
    }

    public static void SetChildrenAlongAxisRecycler(
        HorizontalOrVerticalLayoutGroup layoutGroup,
        IRecyclableLayout iRecyclerLayout, 
        int axis, 
        bool isVertical)
    {
        LayoutRecycler recyclerLayout = iRecyclerLayout.GetLayoutRecycler();
        var rectTransform = (RectTransform)layoutGroup.transform;

        if (recyclerLayout != null && recyclerLayout.CellPrefab)
        {
            float rectSizeOnCurrAxis = rectTransform.rect.size[axis];
            bool flag = isVertical ^ axis == 1;

            if (flag)
            {
                float value = rectSizeOnCurrAxis - (float)((axis != 0)
                    ? layoutGroup.padding.vertical
                    : layoutGroup.padding.horizontal);

                for (int i = 0; i < recyclerLayout.CellRectTransformDimensions.Count; i++)
                {
                    RectTransformDimensions currRect = recyclerLayout.CellRectTransformDimensions[i];

                    if (currRect.LayoutDimensions == null)
                    {
                        currRect.LayoutDimensions =
                            CalculateLayoutDimensionsFromCellInstance(recyclerLayout.CellPool.CellLayoutProxy, currRect);
                    }

                    float minSize = LayoutUtil.GetMinSize(currRect.LayoutDimensions, axis);
                    float preferredSize = LayoutUtil.GetPreferredSize(currRect.LayoutDimensions, axis);
                    float flexibleSize = LayoutUtil.GetFlexibleSize(currRect.LayoutDimensions, axis);

                    if ((axis != 0) ? layoutGroup.childForceExpandHeight : layoutGroup.childForceExpandWidth)
                    {
                        flexibleSize = Mathf.Max(flexibleSize, 1f);
                    }

                    float childSize = Mathf.Clamp(value, minSize, (flexibleSize <= 0f)
                        ? preferredSize
                        : rectSizeOnCurrAxis);
                    float startOffset = LayoutUtil.GetStartOffset(layoutGroup, axis, childSize);

                    LayoutUtil.SetChildAlongAxis(currRect, axis, startOffset, childSize);
                }
            }
            else
            {
                float childPos = ((axis != 0) ? layoutGroup.padding.top : layoutGroup.padding.left);

                if (GetTotalFlexibleSize(layoutGroup, axis) == 0f
                    && GetTotalPreferredSize(layoutGroup, axis) < rectSizeOnCurrAxis)
                {
                    childPos = GetStartOffset(layoutGroup, axis,
                        GetTotalPreferredSize(layoutGroup, axis) - (float)((axis != 0)
                        ? layoutGroup.padding.vertical
                        : layoutGroup.padding.horizontal));
                }

                float minToPreferredRatio = 0f;

                if (GetTotalMinSize(layoutGroup, axis) != GetTotalPreferredSize(layoutGroup, axis))
                {
                    float rectSizeAfterMinSize =
                        rectSizeOnCurrAxis - GetTotalMinSize(layoutGroup, axis);
                    float deltaBetweenMinAndPreferred =
                        GetTotalPreferredSize(layoutGroup, axis) - GetTotalMinSize(layoutGroup, axis);

                    minToPreferredRatio = Mathf.Clamp01(rectSizeAfterMinSize / deltaBetweenMinAndPreferred);
                }

                float sizeRatio = 0f;

                if (rectSizeOnCurrAxis > GetTotalPreferredSize(layoutGroup, axis)
                    && GetTotalFlexibleSize(layoutGroup, axis) > 0f)
                {
                    sizeRatio = (rectSizeOnCurrAxis - GetTotalPreferredSize(layoutGroup, axis))
                        / GetTotalFlexibleSize(layoutGroup, axis);
                }

                for (int j = 0; j < recyclerLayout.CellRectTransformDimensions.Count; j++)
                {
                    RectTransformDimensions currRect = recyclerLayout.CellRectTransformDimensions[j];

                    if (currRect.LayoutDimensions == null)
                    {
                        currRect.LayoutDimensions =
                            CalculateLayoutDimensionsFromCellInstance(recyclerLayout.CellPool.CellLayoutProxy,
                            currRect);
                    }

                    float minSize = LayoutUtil.GetMinSize(currRect.LayoutDimensions, axis);
                    float preferredSize = LayoutUtil.GetPreferredSize(currRect.LayoutDimensions, axis);
                    float flexibleSize = LayoutUtil.GetFlexibleSize(currRect.LayoutDimensions, axis);

                    if ((axis != 0) ? layoutGroup.childForceExpandHeight : layoutGroup.childForceExpandWidth)
                    {
                        flexibleSize = Mathf.Max(flexibleSize, 1f);
                    }

                    float childSize = Mathf.Lerp(minSize, preferredSize, minToPreferredRatio);
                    childSize += flexibleSize * sizeRatio;

                    // RectTransformDimensions for record set here
                    LayoutUtil.SetChildAlongAxis(currRect, axis, childPos, childSize);
                    childPos += childSize + layoutGroup.spacing;
                }
            }
        }
    }

    static LayoutDimensions CalculateLayoutDimensionsFromCellInstance(GameObject proxyLayoutGO,
        RectTransformDimensions cellRecord)
    {
        var recyclerCell = proxyLayoutGO.GetComponent<IRecyclerCell>();

        recyclerCell.OnCellInstantiate();
        //recyclerCell.OnCellShow(cellRecord);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)proxyLayoutGO.transform.parent);

        var layoutDims = new LayoutDimensions();
        layoutDims.CopyFromRectTransform((RectTransform)proxyLayoutGO.transform); // Recursive summation
        return layoutDims;
    }
    // HorizontalOrVerticalLayoutGroup calculations
    //==================================================================================================================
}
