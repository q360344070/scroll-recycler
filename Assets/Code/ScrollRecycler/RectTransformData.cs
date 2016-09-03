using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class RectTransformData
{
    //==================================================================================================================
    // Observee properties
    public Transform parent
    {
        get
        {
            return (_observee == null) ? _parent : _observee.parent;
        }
        set
        {
            if (_observee == null) { _parent = value; }
            else { _observee.parent = value; }
        }
    }
    public Quaternion localRotation
    {
        get
        {
            return (_observee == null) ? _localRotation : _observee.localRotation;
        }
        set
        {
            if (_observee == null) { _localRotation = value; }
            else { _observee.localRotation = value; }
        }
    }
    public Vector3 position
    {
        get
        {
            return (_observee == null) ? _position : _observee.position;
        }
        set
        {
            if (_observee == null) { _position = value; }
            else { _observee.position = value; }
        }
    }
    public Vector3 localPosition
    {
        get
        {
            return (_observee == null) ? _localPosition : _observee.localPosition;
        }
        set
        {
            if (_observee == null) { _localPosition = value; }
            else { _observee.localPosition = value; }
        }
    }
    public Vector3 localScale
    {
        get
        {
            return (_observee == null) ? _localScale : _observee.localScale;
        }
        set
        {
            if (_observee == null) { _localScale = value; }
            else { _observee.localScale = value; }
        }
    }
    public Vector2 anchorMin
    {
        get
        {
            return (_observee == null) ? _anchorMin : _observee.anchorMin;
        }
        set
        {
            if (_observee == null) { _anchorMin = value; }
            else { _observee.anchorMin = value; }
        }
    }
    public Vector2 anchorMax
    {
        get
        {
            return (_observee == null) ? _anchorMax : _observee.anchorMax;
        }
        set
        {
            if (_observee == null) { _anchorMax = value; }
            else { _observee.anchorMax = value; }
        }
    }
    public Vector2 anchoredPosition
    {
        get
        {
            return (_observee == null) ? _anchoredPosition : _observee.anchoredPosition;
        }
        set
        {
            if (_observee == null) { _anchoredPosition = value; }
            else { _observee.anchoredPosition = value; }
        }
    }
    public Vector2 sizeDelta
    {
        get
        {
            return (_observee == null) ? _sizeDelta : _observee.sizeDelta;
        }
        set
        {
            if (_observee == null) { _sizeDelta = value; }
            else { _observee.sizeDelta = value; }
        }
    }
    public Vector2 pivot
    {
        get
        {
            return (_observee == null) ? _pivot : _observee.pivot;
        }
        set
        {
            if (_observee == null) { _pivot = value; }
            else { _observee.pivot = value; }
        }
    }

    public Rect rect
    {
        get
        {
            if (_observee == null)
            {
                Vector2 rectSize = Misc.GetRectSize((RectTransform)parent, sizeDelta, anchorMin, anchorMax);
                Vector2 rectPos = new Vector2(pivot.x * rectSize.x, pivot.y * rectSize.y);
                return new Rect(rectPos, rectSize);
            }
            else
            {
                return _observee.rect;
            }
        }
    }

    // Observee properties
    //==================================================================================================================

    public LayoutDimensions LayoutDimensions;

    // Non RTX fields
    Transform _parent;
    Quaternion _localRotation = Quaternion.identity;
    Vector3 _position = Vector3.zero;
    Vector3 _localPosition = Vector3.zero;
    Vector3 _localScale = Vector3.one;
    Vector2 _anchorMin = new Vector2(0.5f, 0.5f);
    Vector2 _anchorMax = new Vector2(0.5f, 0.5f);
    Vector2 _anchoredPosition = Vector2.zero;
    Vector2 _sizeDelta = Vector2.zero;
    Vector2 _pivot = new Vector2(0.5f, 0.5f);

    RectTransform _observee;

    public RectTransformData()
    {
        _observee = null;
    }

    public RectTransformData(RectTransform rtx)
    {
        _observee = rtx;
    }
    
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
        float value = (float)((!topOrRightAligned) ? 0 : 1);
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

        // position - NOTE: Might be able to defer this until axis == 1
        temp = this.position;
        temp[axis] = Misc.AnchoredPositionToWorldPosition((RectTransform)parent, anchoredPosition,
            pivot, anchorMin, anchorMax)[axis];
        this.position = temp;

        // localPosition - NOTE: Might be able to defer this until axis == 1
        temp = this.localPosition;
        temp[axis] = parent.InverseTransformPoint(this.position)[axis];
        this.localPosition = temp;
    }
}
