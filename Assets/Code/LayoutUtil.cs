using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
}
