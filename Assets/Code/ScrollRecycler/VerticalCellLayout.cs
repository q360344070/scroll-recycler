using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class VerticalCellLayout : VerticalLayoutGroup, ICellLayout
    {
        public HorizontalOrVerticalCellLayout CellLayout;

        bool NeedsUnityLayout;

        CellLayout ICellLayout.CellLayout { get { return CellLayout; } }
        LayoutGroup ICellLayout.LayoutGroup { get { return this; } }

        protected override void Awake()
        {
            base.Awake();
            CellLayout.ICellLayout = this;
        }

        // ============ Automatic Layout system functions (Unity) ============
        public override void CalculateLayoutInputHorizontal()
        {
            if (NeedsUnityLayout)
            {
                ManualCalculateLayoutInputHorizontal();
            }
        }

        public override void SetLayoutHorizontal()
        {
            if (NeedsUnityLayout)
            {
                ManualSetLayoutHorizontal();
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            if (NeedsUnityLayout)
            {
                ManualCalculateLayoutInputVertical();
            }
        }

        public override void SetLayoutVertical()
        {
            if (NeedsUnityLayout)
            {
                ManualSetLayoutVertical();
            }
        }

        // =========== Manual Layout functions ===========
        public void ManualLayoutBuild()
        {
            ManualCalculateLayoutInputHorizontal();
            ManualSetLayoutHorizontal();
            ManualCalculateLayoutInputVertical();
            ManualSetLayoutVertical();
        }

        void ManualCalculateLayoutInputHorizontal()
        {
            CalculateCellLayoutInput(0, true);
        }

        void ManualSetLayoutHorizontal()
        {
            CellLayout.SetAllCellsDimensions(LayoutAxis.Horizontal);
        }

        void ManualCalculateLayoutInputVertical()
        {
            CalculateCellLayoutInput(LayoutAxis.Vertical, true);
        }

        void ManualSetLayoutVertical()
        {
            CellLayout.SetAllCellsDimensions(LayoutAxis.Vertical);
        }

        void CalculateCellLayoutInput(LayoutAxis axis, bool isVertical)
        {
            LayoutDimensions layoutDims = CellLayout.SetLayoutInput(axis);
            SetLayoutInputForAxis(layoutDims.Min, layoutDims.Preferred, layoutDims.Flexible, (int)axis);
        }
    }
}
