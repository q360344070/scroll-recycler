using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System.Linq;

public class GridCellLayout : GridLayoutGroup, ICellLayout
{
    [ReadOnly] public CellLayout CellLayoutData;

    bool NeedsUnityLayout = true;

#if UNITY_EDITOR
    protected override void Reset()
    {

    }
#endif

    protected override void Awake()
    {
        base.Awake();
    }

    public void CalcAlongAxisRecycler(int axis, List<RectTransformData> childRects)
    {
        Debug.Log("GridLayoutRecycler.CalcAlongAxisRecycler()");// #debug
        if (axis == 0)
        {

            int childCount = childRects.Count;

            int cols1;
            int cols2;
            if (m_Constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                cols2 = (cols1 = this.m_ConstraintCount);
            }
            else if (this.m_Constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                cols2 = (cols1 = Mathf.CeilToInt((float)childCount / (float)this.m_ConstraintCount - 0.001f));
            }
            else
            {
                cols1 = 1;
                cols2 = Mathf.CeilToInt(Mathf.Sqrt((float)childCount));
            }

            float minWidth = (float)padding.horizontal + (cellSize.x + spacing.x) * (float)cols1 - spacing.x;
            float preferredWidth = (float)padding.horizontal + (cellSize.x + spacing.x) * (float)cols2 - spacing.x;

            SetLayoutInputForAxis(minWidth, preferredWidth, -1f, 0);
        }
        else
        {
            int childCount = childRects.Count;

            int rows;
            if (m_Constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                rows = Mathf.CeilToInt((float)childCount / (float)m_ConstraintCount - 0.001f);
            }
            else if (m_Constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                rows = m_ConstraintCount;
            }
            else
            {
                float x = rectTransform.rect.size.x;
                int columns = Mathf.Max(1, Mathf.FloorToInt((x - (float)padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                rows = Mathf.CeilToInt((float)childCount / (float)columns);
            }
            float height = (float)padding.vertical + (cellSize.y + spacing.y) * (float)rows - spacing.y;
            SetLayoutInputForAxis(height, height, -1f, 1);
        }
    }


    private void SetCellsAlongAxis(int axis, List<RectTransformData> childRects)
    {
        int childCount = childRects.Count;

        if (axis == 0) // Horizontal
        {
            // Adjust the dimensions of child rects when setting along horizontal axis
            foreach (RectTransformData cardRecord in childRects)
            {
                cardRecord.anchorMin = Vector2.up;
                cardRecord.anchorMax = Vector2.up;
                cardRecord.sizeDelta = cellSize;
            }
        }
        else // axis == 1, Vertical
        {
            float x = rectTransform.rect.size.x;
            float y = rectTransform.rect.size.y;
            int num;
            int num2;
            if (m_Constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            {
                num = constraintCount;
                num2 = Mathf.CeilToInt((float)childCount / (float)num - 0.001f);
            }
            else if (this.m_Constraint == GridLayoutGroup.Constraint.FixedRowCount)
            {
                num2 = this.m_ConstraintCount;
                num = Mathf.CeilToInt((float)childCount / (float)num2 - 0.001f);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0f)
                {
                    num = 2147483647;
                }
                else
                {
                    num = Mathf.Max(1, 
                        Mathf.FloorToInt((x - (float)padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                }
                if (cellSize.y + spacing.y <= 0f)
                {
                    num2 = 2147483647;
                }
                else
                {
                    num2 = Mathf.Max(1, 
                        Mathf.FloorToInt((y - (float)padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
                }
            }
            int num3 = ((int)startCorner % (int)GridLayoutGroup.Corner.LowerLeft);
            int num4 = ((int)startCorner / (int)GridLayoutGroup.Corner.LowerLeft);
            int num5;
            int num6;
            int num7;
            if (this.startAxis == GridLayoutGroup.Axis.Horizontal)
            {
                num5 = num;
                num6 = Mathf.Clamp(num, 1, childCount);
                num7 = Mathf.Clamp(num2, 1, Mathf.CeilToInt((float)childCount / (float)num5));
            }
            else
            {
                num5 = num2;
                num7 = Mathf.Clamp(num2, 1, childCount);
                num6 = Mathf.Clamp(num, 1, Mathf.CeilToInt((float)childCount / (float)num5));
            }

            Vector2 vector = new Vector2((float)num6 * cellSize.x + (float)(num6 - 1) * spacing.x, 
                (float)num7 * cellSize.y + (float)(num7 - 1) * spacing.y);
            Vector2 pos = new Vector2(GetStartOffset(0, vector.x), GetStartOffset(1, vector.y));
            for (int j = 0; j < childCount; j++)
            {
                int spacingCountHoriz;
                int spacingCountVert;
                if (this.startAxis == GridLayoutGroup.Axis.Horizontal)
                {
                    spacingCountHoriz = j % num5;
                    spacingCountVert = j / num5;
                }
                else
                {
                    spacingCountHoriz = j / num5;
                    spacingCountVert = j % num5;
                }
                if (num3 == 1)
                {
                    spacingCountHoriz = num6 - 1 - spacingCountHoriz;
                }
                if (num4 == 1)
                {
                    spacingCountVert = num7 - 1 - spacingCountVert;
                }
                LayoutUtil.SetChildAlongAxis(childRects[j], 0, 
                    pos.x + (cellSize[0] + spacing[0]) * (float)spacingCountHoriz, cellSize[0]);
                LayoutUtil.SetChildAlongAxis(childRects[j], 1, 
                    pos.y + (cellSize[1] + spacing[1]) * (float)spacingCountVert, cellSize[1]);
            }
        }
    }

    public CellLayout GetCellLayout()
    {
        return CellLayoutData;
    }

    public LayoutGroup GetLayoutGroup()
    {
        throw new NotImplementedException();
    }

    // =========== Automatic Layout system functions (Unity) ===========
    // This uses records instead of rect transforms to determine size only, add support for a branch if you
    // want to use existing instantiated rects
    public override void CalculateLayoutInputHorizontal()
    {
        if (NeedsUnityLayout)
        {
            ManualCalculateLayoutInputHorizontal();
        }
    }

    public override void CalculateLayoutInputVertical()
    {
        if (NeedsUnityLayout)
        {
            ManualCalculateLayoutInputVertical();
        }
    }

    public override void SetLayoutHorizontal()
    {
    }

    public override void SetLayoutVertical()
    {
        if (NeedsUnityLayout)
        {
            NeedsUnityLayout = false;
        }
    }

    // =========== Manual Layout functions ===========
    public void ManualCalculateLayoutInputHorizontal()
    {
        CalcAlongAxisRecycler(0, CellLayoutData.AllCellsRectTransformData);
    }

    public void ManualCalculateLayoutInputVertical()
    {
        CalcAlongAxisRecycler(1, CellLayoutData.AllCellsRectTransformData);
    }

    public void ManualSetLayoutHorizontal()
    {
        SetCellsAlongAxis(0, CellLayoutData.AllCellsRectTransformData);
    }

    public void ManualSetLayoutVertical()
    {
        SetCellsAlongAxis(1, CellLayoutData.AllCellsRectTransformData);
    }

    public void ManualLayoutBuild()
    {
        ManualCalculateLayoutInputHorizontal();
        ManualSetLayoutHorizontal();
        ManualCalculateLayoutInputVertical();
        ManualSetLayoutVertical();
    }

    // =========== Proxy Layout functions ===========
    public void ProxyLayoutBuild()
    {
        CalcAlongAxisRecycler(0, CellLayoutData.AllCellsRectTransformData);
        CalcAlongAxisRecycler(1, CellLayoutData.AllCellsRectTransformData);
        SetCellsAlongAxis(0, CellLayoutData.AllCellsRectTransformData);
        SetCellsAlongAxis(1, CellLayoutData.AllCellsRectTransformData);
    }
}
