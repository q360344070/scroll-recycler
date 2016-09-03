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
        }

        public CellLayout GetCellLayout()
        {
            return CellLayout;
        }

        public LayoutGroup GetLayoutGroup()
        {
            return this;
        }

        void CalcAlongAxisRecycler(int axis, bool isVertical)
        {
            float totalMin = 0.0f;
            float totalPreferred = 0.0f;
            float totalFlexible = 0.0f;

            LayoutUtil.CalcAlongAxisRecycler(this, this, axis, isVertical, ref totalMin, ref totalPreferred,
                ref totalFlexible);
            SetLayoutInputForAxis(totalMin, totalPreferred, totalFlexible, axis);
        }

        void SetChildrenAlongAxisRecycler(int axis, bool isVertical)
        {
            LayoutUtil.SetChildrenAlongAxisRecycler(this, this, axis, isVertical);
        }

        public void ProxyLayoutBuild()
        {
            throw new NotImplementedException();
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
            CalcAlongAxisRecycler(0, true);
        }

        void ManualSetLayoutHorizontal()
        {
            SetChildrenAlongAxisRecycler(0, true);
        }

        void ManualCalculateLayoutInputVertical()
        {
            CalcAlongAxisRecycler(1, true);
        }

        void ManualSetLayoutVertical()
        {
            SetChildrenAlongAxisRecycler(1, true);
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
