using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class HorizontalCellLayout : HorizontalLayoutGroup, ICellLayout
    {
        [ReadOnly] public CellLayout CellLayoutData;

        protected override void Awake()
        {
            base.Awake();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            //base.CalculateLayoutInputHorizontal(); // Does rect tracking, not necessary, keep comments -bt
            CalcAlongAxisRecycler(0, false);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxisRecycler(1, false);
        }

        public override void SetLayoutHorizontal()
        {
            CalcAlongAxisRecycler(0, false);
            SetChildrenAlongAxisRecycler(0, false);
        }

        public override void SetLayoutVertical()
        {
            CalcAlongAxisRecycler(1, false);
            SetChildrenAlongAxisRecycler(1, false);
        }

        // =========== Recycler calculations ===========
        void CalcAlongAxisRecycler(int axis, bool isVertical)
        {
            float totalWidth = 0.0f;
            float totalHeight = 0.0f;
            float totalFlexible = 0.0f;

            LayoutUtil.CalcAlongAxisRecycler(this, this, axis, isVertical, ref totalWidth, ref totalHeight,
                ref totalFlexible);
            SetLayoutInputForAxis(totalWidth, totalHeight, totalFlexible, axis);
        }

        void SetChildrenAlongAxisRecycler(int axis, bool isVertical)
        {
            LayoutUtil.SetChildrenAlongAxisRecycler(this, this, axis, isVertical);
        }

        public void ManualLayoutBuild()
        {
        }

        public void ProxyLayoutBuild()
        {
            throw new NotImplementedException();
        }

        public CellLayout GetCellLayout()
        {
            return CellLayoutData;
        }

        public LayoutGroup GetLayoutGroup()
        {
            throw new NotImplementedException();
        }
    }
}