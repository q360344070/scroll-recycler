using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LayoutInputCalculator : MonoBehaviour, ILayoutElement
{
    public bool IsVertical;

    public float flexibleHeight { get { return _FlexibleHeight; } set { _FlexibleHeight = value; } }
    public float flexibleWidth { get { return _FlexibleWidth; } set { _FlexibleWidth = value; } }
    public int layoutPriority { get { return _LayoutPriority; } set { _LayoutPriority = value; } }
    public float minHeight { get { return _MinHeight; } set { _MinHeight = value; } }
    public float minWidth { get { return _MinWidth; } set { _MinWidth = value; } }
    public float preferredHeight { get { return _PreferredHeight; } set { _PreferredHeight = value; } }
    public float preferredWidth { get { return _PreferredWidth; } set { _PreferredWidth = value; } }

    private float _FlexibleHeight;
    private float _FlexibleWidth;
    private int _LayoutPriority;
    private float _MinHeight;
    private float _MinWidth;
    private float _PreferredHeight;
    private float _PreferredWidth;

    public void CalculateLayoutInputHorizontal()
    {
        CalcAlongAxis(0);
    }

    public void CalculateLayoutInputVertical()
    {
        CalcAlongAxis(1);
    }

    protected void CalcAlongAxis(int axis)
    {
        float totalMinSize = 0.0f;
        float totalPreferredSize = 0.0f;
        float totalFlexibleSize = 0f;
        bool flag = IsVertical ^ axis == 1;

        List<RectTransform> rectChildren = transform.Cast<Transform>().Select((t) => (RectTransform)t).ToList();

        for (int i = 0; i < rectChildren.Count; i++)
        {
            RectTransform rect = rectChildren[i];

            float minSize = LayoutUtility.GetMinSize(rect, axis);
            float preferredSize = LayoutUtility.GetPreferredSize(rect, axis);
            float flexibleSize = LayoutUtility.GetFlexibleSize(rect, axis);
            if (flag)
            {
                totalMinSize = Mathf.Max(minSize, totalMinSize);
                totalPreferredSize = Mathf.Max(preferredSize, totalPreferredSize);
                totalFlexibleSize = Mathf.Max(flexibleSize, totalFlexibleSize);
            }
            else
            {
                totalMinSize += minSize;
                totalPreferredSize += preferredSize;
                totalFlexibleSize += flexibleSize;
            }
        }
        totalPreferredSize = Mathf.Max(totalMinSize, totalPreferredSize);

        if (axis == 0)
        {
            _MinWidth = totalMinSize;
            _PreferredWidth = totalPreferredSize;
            _FlexibleWidth = totalFlexibleSize;
        }
        else
        {
            _MinHeight = totalMinSize;
            _PreferredHeight = totalPreferredSize;
            _FlexibleHeight = totalFlexibleSize;
        }
    }

}
