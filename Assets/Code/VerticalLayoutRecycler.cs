using Core.Diagnostics;
using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(RecyclerLayout))]
    public class VerticalLayoutRecycler : HorizontalOrVerticalLayoutGroup, IRecyclerLayout
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

        public override void CalculateLayoutInputHorizontal() {}

        public override void SetLayoutHorizontal() {}

        public override void CalculateLayoutInputVertical() {}

        public override void SetLayoutVertical() {}

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

            // NOTE: Doing this additional ForceRebuild for ContentSizeFitter is expensive, find alternate way
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)RecyclerLayout.ScrollRecycler.ScrollRect.content);
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

        public RecyclerLayout GetRecyclerLayout()
        {
            return RecyclerLayout;
        }

        // *************************************************************************************************************
        // Recycler calculations
        // *************************************************************************************************************
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
    }
}
