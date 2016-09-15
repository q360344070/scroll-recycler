using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

public static class RecyclerUtil
{
    public static float GetMinSize(RectTransform rect, int axis)
    {
        return (axis == 0) ? GetMinWidth(rect) : GetMinHeight(rect);
    }

    public static float GetMinWidth(RectTransform rect)
    {
        return GetLayoutProperty(rect, (ILayoutElement e) => e.minWidth, 0f);
    }

    public static float GetMinHeight(RectTransform rect)
    {
        return GetLayoutProperty(rect, (ILayoutElement e) => e.minHeight, 0f);
    }

    public static float GetPreferredSize(RectTransform rect, int axis)
    {
        return (axis == 0) ? GetPreferredWidth(rect) : GetPreferredHeight(rect);
    }

    public static float GetPreferredWidth(RectTransform rect)
    {
        float minWidth = GetLayoutProperty(rect, (ILayoutElement e) => e.minWidth, 0f);
        float preferredWidth = GetLayoutProperty(rect, (ILayoutElement e) => e.preferredWidth, 0f);
        return Mathf.Max(minWidth, preferredWidth);
    }

   public static float GetPreferredHeight(RectTransform rect)
    {
        float minHeight = GetLayoutProperty(rect, (ILayoutElement e) => e.minHeight, 0f);
        float preferredHeight = GetLayoutProperty(rect, (ILayoutElement e) => e.preferredHeight, 0f);

        return Mathf.Max(minHeight, preferredHeight);
    }

    public static float GetFlexibleSize(RectTransform rect, int axis)
    {
        return (axis == 0) ? GetFlexibleWidth(rect) : GetFlexibleHeight(rect);
    }

    public static float GetFlexibleWidth(RectTransform rect)
    {
        return GetLayoutProperty(rect, (ILayoutElement e) => e.flexibleWidth, 0f);
    }

    public static float GetFlexibleHeight(RectTransform rect)
    {
        return GetLayoutProperty(rect, (ILayoutElement e) => e.flexibleHeight, 0f);
    }

    public static float GetLayoutProperty(
        RectTransform rect, 
        Func<ILayoutElement, float> property, 
        float defaultValue)
    {
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

            // NOTE: Original check only allows behaviors to trigger if isActiveAndEnabled
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
                        }
                        else if (propertyValue > num)
                        {
                            num = propertyValue;
                        }
                    }
                }
            }
        }

        ListPool<Component>.Release(list);
        return num;
    }

    public static float GetMinSize(LayoutData dims, int axis)
    {
        return (axis == 0) ? dims.minWidth : dims.minHeight;
    }

    public static float GetPreferredSize(LayoutData dims, int axis)
    {
        return (axis == 0) ? dims.preferredWidth : dims.preferredHeight;
    }

    public static float GetFlexibleSize(LayoutData dims, int axis)
    {
        return (axis == 0) ? dims.flexibleWidth : dims.flexibleHeight;
    }

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

    // =========== LayoutGroup forks ===========
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

    // =========== Class definitions ============
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
}

public enum LayoutAxis
{
    Horizontal = 0,
    Vertical = 1
}
