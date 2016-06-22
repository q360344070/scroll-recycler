using UnityEngine;
using System.Collections;

public static class Misc
{
    //==================================================================================================================
    // Rect transformation functions

    public class CornerInfo
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    public static CornerInfo GetCornerInfo(Vector3[] corners)
    {
        var ci = new CornerInfo();

        float leftMostX = float.MaxValue;
        float rightMostX = float.MinValue;
        float topMostY = float.MinValue;
        float bottomMostY = float.MaxValue;

        foreach (Vector3 corner in corners)
        {
            if (corner.x < leftMostX)
            {
                leftMostX = corner.x;
            }
            if (corner.x > rightMostX)
            {
                rightMostX = corner.x;
            }
            if (corner.y < bottomMostY)
            {
                bottomMostY = corner.y;
            }
            if (corner.y > topMostY)
            {
                topMostY = corner.y;
            }
        }

        ci.topLeft = new Vector2(leftMostX, topMostY);
        ci.topRight = new Vector2(rightMostX, topMostY);
        ci.bottomLeft = new Vector2(leftMostX, bottomMostY);
        ci.bottomRight = new Vector2(rightMostX, bottomMostY);

        return ci;
    }

    public static Vector2 GetSizeFromCorners(CornerInfo cornerInfo)
    {
        float leftMostX = cornerInfo.topLeft.x;
        float rightMostX = cornerInfo.bottomRight.x;
        float topMostY = cornerInfo.topLeft.y;
        float bottomMostY = cornerInfo.bottomRight.y;

        return new Vector2(
            Mathf.Abs(rightMostX - leftMostX),
            Mathf.Abs(topMostY - bottomMostY));
    }

    public static Vector3 GetAnchorWorldSpaceMidpoint(Vector3[] corners, Vector2 anchorMin, Vector2 anchorMax)
    {
        return GetCornersMidpoint(GetAnchorsWorldspaceCorners(corners, anchorMin, anchorMax));
    }

    public static Vector3 GetCornersMidpoint(CornerInfo corners)
    {
        return new Vector3(
            corners.topLeft.x + (Mathf.Abs(corners.bottomRight.x - corners.topLeft.x) / 2.0f),
            corners.bottomRight.y + (Mathf.Abs(corners.topLeft.y - corners.bottomRight.y) / 2.0f),
            0.0f);
    }

    public static CornerInfo GetAnchorsWorldspaceCorners(Vector3[] rectCorners, Vector2 anchorMin, Vector2 anchorMax)
    {
        CornerInfo cornerInfo = GetCornerInfo(rectCorners);
        Vector2 rectSize = GetSizeFromCorners(cornerInfo);
        var ci = new CornerInfo();

        float leftMostX = cornerInfo.bottomLeft.x;
        float bottomMostY = cornerInfo.bottomLeft.y;

        ci.topLeft = new Vector2(
            leftMostX + (anchorMin.x * rectSize.x),
            bottomMostY + (anchorMax.y * rectSize.y));
        ci.bottomLeft = new Vector2(
            leftMostX + (anchorMin.x * rectSize.x),
            bottomMostY + (anchorMin.y * rectSize.y));
        ci.topRight = new Vector2(
            leftMostX + (anchorMax.x * rectSize.x),
            bottomMostY + (anchorMax.y * rectSize.y));
        ci.bottomRight = new Vector2(
            leftMostX + (anchorMax.x * rectSize.x),
            bottomMostY + (anchorMin.y * rectSize.y));

        return ci;
    }

    public static Vector3 AnchoredPositionToWorldPosition(RectTransform parent, Vector2 anchoredPosition, Vector2 pivot,
     Vector2 anchorMin, Vector2 anchorMax)
    {
        Vector3[] parentWorldCorners = new Vector3[4];
        parent.GetWorldCorners(parentWorldCorners);

        Vector3 centeredWorldPos = new Vector3(anchoredPosition.x, anchoredPosition.y, 0.0f)
            + Misc.GetAnchorWorldSpaceMidpoint(parentWorldCorners, anchorMin, anchorMax);

        return centeredWorldPos;
    }

    // Rect transformation functions
    //==================================================================================================================
}
