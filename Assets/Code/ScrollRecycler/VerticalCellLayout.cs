using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class VerticalCellLayout : VerticalLayoutGroup, ICellLayout
    {
        public CellLayout CellLayout;

        bool NeedsUnityLayout;

        protected override void Awake()
        {
            base.Awake();
            CellLayout.ICellLayout = this;
        }

        public CellLayout GetCellLayout()
        {
            return CellLayout;
        }

        public LayoutGroup GetLayoutGroup()
        {
            return this;
        }

        void CalculateCellLayoutInput(int axis, bool isVertical)
        {
            float totalMin = 0.0f;
            float totalPreferred = 0.0f;
            float totalFlexible = 0.0f;

            RecyclerUtil.CalculateCellLayoutInput(
                this,
                this,
                axis,
                isVertical,
                ref totalMin,
                ref totalPreferred,
                ref totalFlexible);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }

        void SetCellsDimensionsAlongCellLayout(int axis, bool isVertical)
        {
            RecyclerUtil.SetAllCellsDimensionsAlongCellLayout(this, this, axis, isVertical);
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
                ManualCalculateLayoutInputVertical();
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            if (NeedsUnityLayout)
            {
                NeedsUnityLayout = false;
            }
        }

        public override void SetLayoutVertical() {}

        // =========== Manual Layout functions ===========
        void ManualCalculateLayoutInputHorizontal()
        {
            CalculateCellLayoutInput(0, true);
        }

        void ManualSetLayoutHorizontal()
        {
            SetCellsDimensionsAlongCellLayout(0, true);
        }

        void ManualCalculateLayoutInputVertical()
        {
            CalculateCellLayoutInput(1, true);
        }

        void ManualSetLayoutVertical()
        {
            SetCellsDimensionsAlongCellLayout(1, true);
        }

        public void ManualLayoutBuild()
        {
            ManualCalculateLayoutInputHorizontal();
            ManualSetLayoutHorizontal();
            ManualCalculateLayoutInputVertical();
            ManualSetLayoutVertical();
        }
    }
}
