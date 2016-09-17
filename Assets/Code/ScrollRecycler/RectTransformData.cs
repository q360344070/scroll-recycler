using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class RectTransformData
{
    public Transform parent
    {
        get
        {
            return _parent;
        }
        set
        {
            _parent = value;
        }
    }

    public Quaternion localRotation
    {
        get
        {
            return _localRotation;
        }
        set
        {
            _localRotation = value;
        }
    }

    public Vector3 position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = value;
        }
    }

    public Vector3 localPosition
    {
        get
        {
            return _localPosition;
        }
        set
        {
            _localPosition = value;
        }
    }

    public Vector3 localScale
    {
        get
        {
            return _localScale;
        }
        set
        {
            _localScale = value;
        }
    }
    public Vector2 anchorMin
    {
        get
        {
            return _anchorMin;
        }
        set
        {
            _anchorMin = value;
        }
    }

    public Vector2 anchorMax
    {
        get
        {
            return _anchorMax;
        }
        set
        {
            _anchorMax = value;
        }
    }

    public Vector2 anchoredPosition
    {
        get
        {
            return _anchoredPosition;
        }
        set
        {
            _anchoredPosition = value;
        }
    }

    public Vector2 sizeDelta
    {
        get
        {
            return _sizeDelta;
        }
        set { _sizeDelta = value; }
    }

    public Vector2 pivot
    {
        get
        {
            return _pivot;
        }
        set
        {
            _pivot = value;
        }
    }

    public Rect rect
    {
        get
        {
            Vector2 rectSize = Misc.GetRectSize((RectTransform)parent, sizeDelta, anchorMin, anchorMax);
            Vector2 rectPos = new Vector2(pivot.x * rectSize.x, pivot.y * rectSize.y);
            return new Rect(rectPos, rectSize);
        }
    }

    // Non RTX fields
    [SerializeField] Transform _parent;
    [SerializeField] Quaternion _localRotation = Quaternion.identity;
    [SerializeField] Vector3 _position = Vector3.zero;
    [SerializeField] Vector3 _localPosition = Vector3.zero;
    [SerializeField] Vector3 _localScale = Vector3.one;
    [SerializeField] Vector2 _anchorMin = new Vector2(0.5f, 0.5f);
    [SerializeField] Vector2 _anchorMax = new Vector2(0.5f, 0.5f);
    [SerializeField] Vector2 _anchoredPosition = Vector2.zero;
    [SerializeField] Vector2 _sizeDelta = Vector2.zero;
    [SerializeField] Vector2 _pivot = new Vector2(0.5f, 0.5f);

    public void CopyFromRectTransform(RectTransform rtx)
    {
        rtx.localRotation = localRotation;
        rtx.localPosition = localPosition;
        rtx.localScale = localScale;
        rtx.anchorMin = anchorMin;
        rtx.anchorMax = anchorMax;
        rtx.anchoredPosition = anchoredPosition;
        rtx.sizeDelta = sizeDelta;
        rtx.pivot = pivot;
    }

    public void SetInsetAndSizeFromParentEdge(RectTransform.Edge edge, float inset, float size)
    {
        int axis = (edge != RectTransform.Edge.Top && edge != RectTransform.Edge.Bottom) ? 0 : 1;
        bool topOrRightAligned = edge == RectTransform.Edge.Top || edge == RectTransform.Edge.Right;
        float value = !topOrRightAligned ? 0 : 1;
        Vector2 temp = default(Vector2); // Used to bypass property assignment

        // anchorMin
        temp = this.anchorMin;
        temp[axis] = value;
        this.anchorMin = temp;

        // anchorMax
        temp = this.anchorMax;
        temp[axis] = value;
        this.anchorMax = temp;

        // sizeDelta
        temp = this.sizeDelta;
        temp[axis] = size;
        this.sizeDelta = temp;

        // anchoredPosition
        temp = this.anchoredPosition;
        temp[axis] = ((!topOrRightAligned)
            ? (inset + size * this.pivot[axis])
            : (-inset - size * (1f - this.pivot[axis])));
        this.anchoredPosition = temp;

        // position
        // NOTE: Might be able to defer this until second axis
        temp = this.position;
        temp[axis] = Misc.AnchoredPositionToWorldPosition((RectTransform)parent, anchoredPosition,
            pivot, anchorMin, anchorMax)[axis];
        this.position = temp;

        // localPosition 
        // NOTE: Might be able to defer this until second axis
        temp = this.localPosition;
        temp[axis] = parent.InverseTransformPoint(this.position)[axis];
        this.localPosition = temp;
    }
}
