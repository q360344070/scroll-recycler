using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public static class ScrollRectExtensions
{
    public static void ScrollToChild(this ScrollRect sr, Vector3 childWorldPos, Vector2 childSize, Vector2 childPivot)
    {
        if (sr && sr.content && sr.content.transform.parent)
        {
            var contentRtx = ((RectTransform)sr.content.transform);

            if (contentRtx)
            {
                contentRtx.anchoredPosition = CalculateAnchoredPosition(sr, contentRtx, childWorldPos, childSize, childPivot);
            }
        }
    }

    private static Vector2 CalculateAnchoredPosition(ScrollRect sr, RectTransform contentRtx, Vector3 childWorldPos, Vector2 childSize, Vector2 childPivot)
    {
        if ((sr.horizontal && sr.vertical) || (!sr.horizontal && !sr.vertical))
        {
            // #debug - UNCOMMENT IN SHADOW
            //MfLog.Error(LC.Trace, "Tried to scroll to a child on a ScrollRect that can scroll both horizontal and "
            //    + "vertical!");
            return contentRtx.anchoredPosition;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)sr.transform); // Can remove?

        if (sr.horizontal)
        {
            // This will not work when invoked during an Awake()
            float xDist = sr.ChildPosInContentRect(childWorldPos).x;
            float targetXPos = xDist
                + (childSize.x * childPivot.x) // Add the influence of a pivot on child if one present
                - (contentRtx.rect.width * contentRtx.pivot.x) // Subtract the influence of the content's pivot and the anchor of the parent rects
                - (((RectTransform)contentRtx.parent.transform).rect.width * contentRtx.anchorMin.x);

            return sr.GetContentAnchoredPositionBounded(new Vector2(targetXPos, sr.content.anchoredPosition.y));
        }
        else
        {
            float yDist = sr.ChildPosInContentRect(childWorldPos).y;

            float targetYPos = yDist
                - (childSize.y * (1.0f - childPivot.y)) // Subtract the influence of a pivot on child if one present
                + (contentRtx.rect.height * (1.0f - contentRtx.pivot.y)) // Add the influence of the content's pivot and the anchor of the parent rects
                + (((RectTransform)contentRtx.parent.transform).rect.height * (1.0f - contentRtx.anchorMin.y));

            return sr.GetContentAnchoredPositionBounded(new Vector2(sr.content.anchoredPosition.x, targetYPos));
        }
    }

    public static Vector2 GetContentAnchoredPositionBounded(this ScrollRect sr, Vector2 targetAnchoredPos)
    {
        if (sr && sr.content)
        {
            var contentRtx = sr.content.transform as RectTransform;
            var srRtx = sr.transform as RectTransform;
            Bounds contentBounds = sr.GetContentBoundsInAnotherSpace((RectTransform)sr.transform);
            Bounds viewBounds = new Bounds(srRtx.rect.center, srRtx.rect.size);
            bool allowHorizontal = (sr.movementType == ScrollRect.MovementType.Unrestricted) || contentBounds.size.x > viewBounds.size.x;
            bool allowVertical = (sr.movementType == ScrollRect.MovementType.Unrestricted) || contentBounds.size.y > viewBounds.size.y;

            Vector2 offset = sr.CalculateOffsetImpl(targetAnchoredPos - contentRtx.anchoredPosition);
            targetAnchoredPos += offset;

            if (!sr.horizontal || !allowHorizontal)
                targetAnchoredPos.x = contentRtx.anchoredPosition.x;
            if (!sr.vertical || !allowVertical)
                targetAnchoredPos.y = contentRtx.anchoredPosition.y;

            return targetAnchoredPos;
        }

        return Vector2.zero;
    }

    public static Vector2 ChildPosInContentRect(this ScrollRect sr, Vector3 childWorldPos)
    {
        if (sr)
        {
            return sr.transform.InverseTransformPoint(sr.content.transform.position)
                - sr.transform.InverseTransformPoint(childWorldPos);

            // Corrected form?
            //return sr.content.transform.position - child.transform.position;
        }
        else
        {
            return Vector2.zero;
        }
    }

    public static Vector2 GetClosestContentAnchoredPositionBounded(this ScrollRect sr, Vector2 targetAnchoredPosA, Vector2 targetAnchoredPosB)
    {
        if (sr && sr.content)
        {
            var contentRtx = sr.content.transform as RectTransform;
            var srRtx = sr.transform as RectTransform;
            Bounds contentBounds = sr.GetContentBoundsInAnotherSpace((RectTransform)sr.transform);
            Bounds viewBounds = new Bounds(srRtx.rect.center, srRtx.rect.size);
            bool allowHorizontal = (sr.movementType == ScrollRect.MovementType.Unrestricted) || contentBounds.size.x > viewBounds.size.x;
            bool allowVertical = (sr.movementType == ScrollRect.MovementType.Unrestricted) || contentBounds.size.y > viewBounds.size.y;

            Vector2 offsetA = sr.CalculateOffsetImpl(targetAnchoredPosA - contentRtx.anchoredPosition);
            targetAnchoredPosA += offsetA;

            Vector2 offsetB = sr.CalculateOffsetImpl(targetAnchoredPosB - contentRtx.anchoredPosition);
            targetAnchoredPosB += offsetB;


            if (!sr.horizontal || !allowHorizontal)
            {
                targetAnchoredPosA.x = contentRtx.anchoredPosition.x;
                targetAnchoredPosB.x = contentRtx.anchoredPosition.x;
            }
            if (!sr.vertical || !allowVertical)
            {
                targetAnchoredPosA.y = contentRtx.anchoredPosition.y;
                targetAnchoredPosB.y = contentRtx.anchoredPosition.y;
            }

            // we want to get the closest position along A<->B
            float t = 0f;
            if (!Mathf.Approximately(targetAnchoredPosA.x, targetAnchoredPosB.x))
                t = Mathf.InverseLerp(targetAnchoredPosA.x, targetAnchoredPosB.x, contentRtx.anchoredPosition.x);
            else
                t = Mathf.InverseLerp(targetAnchoredPosA.y, targetAnchoredPosB.y, contentRtx.anchoredPosition.y);

            return Vector2.Lerp(targetAnchoredPosA, targetAnchoredPosB, t);
        }

        return Vector2.zero;
    }

    static Bounds GetContentBoundsInAnotherSpace(this ScrollRect sr, RectTransform newSpace)
    {
        if (sr && sr.content && newSpace)
        {
            var contentBoundsMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var contentBoundsMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var rtx = newSpace.transform as RectTransform;

            var contentCorners = new Vector3[4];

            Matrix4x4 toSRLocal = rtx.worldToLocalMatrix;
            sr.content.GetWorldCorners(contentCorners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toSRLocal.MultiplyPoint3x4(contentCorners[j]);
                contentBoundsMin = Vector3.Min(v, contentBoundsMin);
                contentBoundsMax = Vector3.Max(v, contentBoundsMax);
            }

            var bounds = new Bounds(contentBoundsMin, Vector3.zero);
            bounds.Encapsulate(contentBoundsMax);
            return bounds;
        }

        return default(Bounds);
    }

    static Vector3 CalculateOffsetImpl(this ScrollRect sr, Vector2 delta)
    {
        if (sr)
        {
            Vector3 offset = Vector3.zero;
            if (sr.movementType == ScrollRect.MovementType.Unrestricted)
            {
                return offset;
            }

            var srRtx = sr.transform as RectTransform;
            Bounds srBounds = new Bounds(srRtx.rect.center, srRtx.rect.size);
            Bounds contentBounds = sr.GetContentBoundsInAnotherSpace((RectTransform)sr.transform);
            bool allowHorizontal = (sr.movementType == ScrollRect.MovementType.Unrestricted) || contentBounds.size.x > srBounds.size.x;
            bool allowVertical = (sr.movementType == ScrollRect.MovementType.Unrestricted) || contentBounds.size.y > srBounds.size.y;

            Vector2 contentBoundsTranslatedMin = contentBounds.min;
            Vector2 contentBoundsTranslatedMax = contentBounds.max;

            if (sr.horizontal)
            {
                contentBoundsTranslatedMin.x += delta.x;
                contentBoundsTranslatedMax.x += delta.x;
                if (!allowHorizontal || contentBoundsTranslatedMin.x > srBounds.min.x)
                    offset.x = srBounds.min.x - contentBoundsTranslatedMin.x;
                else if (contentBoundsTranslatedMax.x < srBounds.max.x)
                    offset.x = srBounds.max.x - contentBoundsTranslatedMax.x;
            }

            if (sr.vertical)
            {
                contentBoundsTranslatedMin.y += delta.y;
                contentBoundsTranslatedMax.y += delta.y;
                if (!allowVertical || contentBoundsTranslatedMax.y < srBounds.max.y)
                    offset.y = srBounds.max.y - contentBoundsTranslatedMax.y;
                else if (contentBoundsTranslatedMin.y > srBounds.min.y)
                    offset.y = srBounds.min.y - contentBoundsTranslatedMin.y;
            }

            return offset;
        }

        return Vector3.zero;
    }
}
