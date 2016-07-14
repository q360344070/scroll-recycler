using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(LayoutRecycler))]
    public class VerticalLayoutRecycler : HorizontalOrVerticalLayoutGroup, IRecyclableLayout
    {
        [ReadOnly] public LayoutRecycler RecyclerLayout;

        bool NeedsUnityLayout;

        public LayoutRecycler GetLayoutRecycler()
        {
            return RecyclerLayout;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            RecyclerLayout = GetComponent<LayoutRecycler>();
            RecyclerLayout.LayoutGroup = this;
        }
#endif

        protected override void Awake()
        {
            base.Awake();
        }

        //==================================================================================================================
        // Automatic Layout system functions (Unity)
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

        // Automatic Layout system functions (Unity)
        //===============================================================================================================

        //==============================================================================================================
        // Manual Layout functions

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

        // Manual Layout functions
        //==============================================================================================================

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
    }
}
