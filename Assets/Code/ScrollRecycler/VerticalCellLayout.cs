using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class VerticalCellLayout : VerticalLayoutGroup, ICellLayout
    {
        [ReadOnly] public LayoutInput LayoutInput;

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
            LayoutInput.Width = CellLayout.GetLayoutDimensions(LayoutAxis.Horizontal);
            SetLayoutInputForAxis(
                LayoutInput.Width.Min, 
                LayoutInput.Width.Preferred, 
                LayoutInput.Width.Flexible, 
                (int)LayoutAxis.Horizontal);
        }

        void ManualSetLayoutHorizontal()
        {
            CellLayout.SetCellsDimensions(LayoutAxis.Horizontal);
        }

        void ManualCalculateLayoutInputVertical()
        {
            LayoutInput.Height = CellLayout.GetLayoutDimensions(LayoutAxis.Vertical);
            SetLayoutInputForAxis(
                LayoutInput.Height.Min, 
                LayoutInput.Height.Preferred,
                LayoutInput.Height.Flexible, 
                (int)LayoutAxis.Vertical);
        }

        void ManualSetLayoutVertical()
        {
            CellLayout.SetCellsDimensions(LayoutAxis.Vertical);
        }
    }
}
