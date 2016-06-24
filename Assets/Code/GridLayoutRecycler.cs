using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using Core.Diagnostics;

[RequireComponent(typeof(RecyclerLayout))]
public class GridLayoutRecycler : LayoutGroup, IRecyclerLayout
{
    public string childPrefabName;

    [ReadOnly] public RecyclerLayout RecyclerLayout;

    [SerializeField]
    protected Vector2 m_CellSize = new Vector2(100f, 100f);
    public Vector2 cellSize { get { return m_CellSize; } set { SetProperty<Vector2>(ref m_CellSize, value); } }

    [SerializeField]
    protected Vector2 m_Spacing = Vector2.zero;
    public Vector2 spacing { get { return m_Spacing; } set { SetProperty<Vector2>(ref m_Spacing, value); } }

    public int constraintCount { get { return m_ConstraintCount; } set { SetProperty<int>(ref m_ConstraintCount, Mathf.Max(1, value)); } }

    [SerializeField]
    protected GridLayoutGroup.Corner m_StartCorner;
    public GridLayoutGroup.Corner startCorner { get { return m_StartCorner; } set { SetProperty<GridLayoutGroup.Corner>(ref m_StartCorner, value); } }

    [SerializeField]
    protected GridLayoutGroup.Axis m_StartAxis;
    public GridLayoutGroup.Axis startAxis { get { return m_StartAxis; } set { SetProperty<GridLayoutGroup.Axis>(ref m_StartAxis, value); } }

    [SerializeField]
    protected GridLayoutGroup.Constraint m_Constraint;

    [SerializeField]
    protected int m_ConstraintCount = 2;

    public RecyclerLayout GetRecyclerLayout()
    {
        return RecyclerLayout;
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();
        RecyclerLayout = GetComponent<RecyclerLayout>();
        RecyclerLayout.LayoutGroup = this;
    }
#endif

    protected override void Awake()
    {
        base.Awake();
    }

    // This uses records instead of rect transforms to determine size only, add support for a branch if you
    // want to use existing instantiated rects
    public override void CalculateLayoutInputHorizontal() {}

    public override void CalculateLayoutInputVertical() {}

    // This function is called when layout marks for rebuild
    public override void SetLayoutHorizontal() {}

    // This function is called when layout marks for rebuild
    public override void SetLayoutVertical() {}
    
    public void ManualCalculateLayoutInputHorizontal()
    {
        int childCount = RecyclerLayout.CellRecords.Count;

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

    public void ManualCalculateLayoutInputVertical()
    {
        int childCount = RecyclerLayout.CellRecords.Count;

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

    // This function is called when layout marks for rebuild
    public void ManualSetLayoutHorizontal()
    {
        SetCellsAlongAxis(0);
    }

    // This function is called when layout marks for rebuild
    public void ManualSetLayoutVertical()
    {
        SetCellsAlongAxis(1);
    }

    public void ManualLayoutBuild()
    {
        ManualCalculateLayoutInputHorizontal();
        ManualSetLayoutHorizontal();
        ManualCalculateLayoutInputVertical();
        { // #debug
            LayoutRebuilder.ForceRebuildLayoutImmediate(RecyclerLayout.ScrollRecycler.ScrollRect.content);
        }
        ManualSetLayoutVertical();
    }

    private void SetCellsAlongAxis(int axis)
    {
        int childCount = RecyclerLayout.CellRecords.Count;

        if (axis == 0) // Horizontal
        {
            // Adjust the dimensions of child rects when setting along horizontal axis
            foreach (CellRecord cardRecord in RecyclerLayout.CellRecords)
            {
                cardRecord.RectTransformDimensions.anchorMin = Vector2.up;
                cardRecord.RectTransformDimensions.anchorMax = Vector2.up;
                cardRecord.RectTransformDimensions.sizeDelta = cellSize;
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
                LayoutUtil.SetChildAlongAxis(RecyclerLayout.CellRecords[j].RectTransformDimensions, 0, 
                    pos.x + (cellSize[0] + spacing[0]) * (float)spacingCountHoriz, cellSize[0]);
                LayoutUtil.SetChildAlongAxis(RecyclerLayout.CellRecords[j].RectTransformDimensions, 1, 
                    pos.y + (cellSize[1] + spacing[1]) * (float)spacingCountVert, cellSize[1]);
            }
        }
    }
}
