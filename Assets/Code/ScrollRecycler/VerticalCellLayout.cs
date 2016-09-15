using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class VerticalCellLayout : VerticalLayoutGroup, ICellLayout
    {
        public CellLayout CellLayout;

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
            CellLayout.SetAllCellsDimensions(this, this, (int)LayoutAxis.Horizontal, true);
        }

        void ManualCalculateLayoutInputVertical()
        {
            CalculateCellLayoutInput(1, true);
        }

        void ManualSetLayoutVertical()
        {
            CellLayout.SetAllCellsDimensions(this, this, (int)LayoutAxis.Vertical, true);
        }

        void CalculateCellLayoutInput(int axis, bool isVertical)
        {
            float totalMin = 0.0f;
            float totalPreferred = 0.0f;
            float totalFlexible = 0.0f;

            CellLayout.SetLayoutInput(
                this,
                this,
                axis,
                isVertical,
                ref totalMin,
                ref totalPreferred,
                ref totalFlexible);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }
    }
}
