using Core.Diagnostics;
using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(RecyclerLayout))]
    public class HorizontalLayoutRecycler : HorizontalOrVerticalLayoutGroup, IRecyclerLayout
    {
        [ReadOnly] public RecyclerLayout RecyclerLayout;

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

        public RecyclerLayout GetRecyclerLayout()
        {
            return RecyclerLayout;
        }

        // *************************************************************************************************************
        // Recycler calculations
        // *************************************************************************************************************
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
    }
}
