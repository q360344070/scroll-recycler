using UnityEngine;
using System.Collections;

public class RectTransformDimensions
{
    public Transform parent;
    public Quaternion localRotation = Quaternion.identity;
    public Vector3 position = Vector3.zero;
    public Vector3 localPosition = Vector3.zero;
    public Vector3 localScale = Vector3.one;
    public Vector2 anchorMin = new Vector2(0.5f, 0.5f);
    public Vector2 anchorMax = new Vector2(0.5f, 0.5f);
    public Vector2 anchoredPosition = Vector2.zero;
    public Vector2 sizeDelta = Vector2.zero;
    public Vector2 pivot = new Vector2(0.5f, 0.5f);

    public Rect rect
    {
        get
        {
            throw new System.Exception("Implement RectTransformDimensions.rect!!");
        }
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

        this.anchorMin[axis] = value;
        this.anchorMax[axis] = value;
        this.sizeDelta[axis] = size;

        // NOTE: Might be easily computed to local space coordinates
        this.anchoredPosition[axis] = ((!topOrRightAligned)
            ? (inset + size * this.pivot[axis])
            : (-inset - size * (1f - this.pivot[axis])));


        Vector3[] parentWorldCorners = new Vector3[4];
        ((RectTransform)parent).GetWorldCorners(parentWorldCorners);

        { // Compute the world position using the world corners of the enclosing rect
            Vector3 worldPosition = Misc.AnchoredPositionToWorldPosition((RectTransform)parent, anchoredPosition,
                pivot, new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));

            this.position[axis] = worldPosition[axis];
        }
    }
}
