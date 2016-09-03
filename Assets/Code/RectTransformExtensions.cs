using UnityEngine;
using System.Collections;

public static class RectTransformExtensions
{
    public static void SetAnchoredPosition3DX(this RectTransform rtx, float xVal)
    {
        rtx.anchoredPosition3D = new Vector3(xVal, rtx.anchoredPosition3D.y,
            rtx.anchoredPosition3D.z);
    }

    public static void SetAnchoredPosition3DY(this RectTransform rtx, float yVal)
    {
        rtx.anchoredPosition3D = new Vector3(rtx.anchoredPosition3D.x, yVal,
            rtx.anchoredPosition3D.z);
    }

    public static void SetAnchoredPosition3DZ(this RectTransform rtx, float zVal)
    {
        rtx.anchoredPosition3D = new Vector3(rtx.anchoredPosition3D.x, rtx.anchoredPosition3D.y,
            zVal);
    }

    public static void CopyFromRectTransform(this RectTransform rtx, RectTransform other)
    {
        // NOTE: We skip setting the parent because expensive in Unity

        rtx.anchoredPosition = other.anchoredPosition;
        rtx.anchorMax = other.anchorMax;
        rtx.anchorMin = other.anchorMin;
        rtx.localRotation = other.localRotation;
        rtx.localScale = other.localScale;
        rtx.sizeDelta = other.sizeDelta;
        rtx.pivot = other.pivot;
    }

    public static void CopyFromRectTransformDimensions(this RectTransform rtx, RectTransformData rtd)
    {
        // NOTE: We skip setting the parent because expensive in Unity

        rtx.anchoredPosition = rtd.anchoredPosition;
        rtx.anchorMax = rtd.anchorMax;
        rtx.anchorMin = rtd.anchorMin;
        rtx.localRotation = rtd.localRotation;
        rtx.localScale = rtd.localScale;
        rtx.pivot = rtd.pivot;
        rtx.sizeDelta = rtd.sizeDelta;
        rtx.SetAnchoredPosition3DZ(0.0f);
    }
}
