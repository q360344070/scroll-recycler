using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LayoutData : ILayoutElement
{
    public float flexibleHeight { get { return _FlexibleHeight; } set { _FlexibleHeight = value; } }
    public float flexibleWidth { get { return _FlexibleWidth; } set { _FlexibleWidth = value; } }
    public int layoutPriority { get { return _LayoutPriority; } set { _LayoutPriority = value; } }
    public float minHeight { get { return _MinHeight; } set { _MinHeight = value; } }
    public float minWidth { get { return _MinWidth; } set { _MinWidth = value; } }
    public float preferredHeight { get { return _PreferredHeight; } set { _PreferredHeight = value; } }
    public float preferredWidth { get { return _PreferredWidth; } set { _PreferredWidth = value; } }

    [SerializeField] private float _MinWidth;
    [SerializeField] private float _MinHeight;
    [SerializeField] private float _PreferredWidth;
    [SerializeField] private float _PreferredHeight;
    [SerializeField] private float _FlexibleWidth;
    [SerializeField] private float _FlexibleHeight;
    [SerializeField] private int _LayoutPriority;

    public LayoutData(
        LayoutDimensions layoutWidth = default(LayoutDimensions), 
        LayoutDimensions layoutHeight = default(LayoutDimensions))
    {
        SetWidth(layoutWidth);
        SetHeight(layoutHeight);
    }

    public void SetWidth(LayoutDimensions widthDims)
    {
        minWidth = widthDims.Min;
        preferredWidth = widthDims.Preferred;
        flexibleWidth = widthDims.Flexible;
    }

    public void SetHeight(LayoutDimensions heightDims)
    {
        minHeight = heightDims.Min;
        preferredHeight = heightDims.Preferred;
        flexibleHeight = heightDims.Flexible;
    }

    public void CalculateLayoutInputHorizontal()
    {
    }

    public void CalculateLayoutInputVertical()
    {
    }

    public void CopyFromRectTransform(RectTransform rtx)
    {
        minWidth = LayoutUtility.GetMinWidth(rtx);
        minHeight = LayoutUtility.GetMinHeight(rtx);
        preferredWidth = LayoutUtility.GetPreferredWidth(rtx);
        preferredHeight = LayoutUtility.GetPreferredHeight(rtx);
        flexibleWidth = LayoutUtility.GetFlexibleWidth(rtx);
        flexibleHeight = LayoutUtility.GetFlexibleHeight(rtx);
    }

    public void CopyFromLayoutElement(ILayoutElement le)
    {
        minWidth = le.minWidth;
        minHeight = le.minHeight;
        preferredWidth = le.preferredWidth;
        preferredHeight = le.preferredHeight;
        flexibleWidth = le.flexibleWidth;
        flexibleHeight = le.flexibleHeight;
    }
}

public struct LayoutDimensions
{
    public float Min;
    public float Preferred;
    public float Flexible;
}

public struct LayoutInput
{
    public LayoutDimensions Height;
    public LayoutDimensions Width;
}
