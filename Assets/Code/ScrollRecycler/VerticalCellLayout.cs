using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class VerticalCellLayout : VerticalLayoutGroup, ICellLayout
    {
        [ReadOnly] public LayoutInput LayoutInput;

        public HorizontalOrVerticalCellLayout CellLayout;

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
        }

        public override void SetLayoutHorizontal()
        {
        }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutVertical()
        {
        }

        // =========== Manual Layout functions ===========
        public void ManualLayoutBuild()
        {
            ManualCalculateLayoutInputHorizontal();
            ManualSetLayoutHorizontal();
            ManualCalculateLayoutInputVertical();
            ManualSetLayoutVertical();

            Action<string> dbgPrint = (string msg) => Debug.Log(string.Format("{0}Layout Input: width = {1} height = {2}", msg, LayoutInput.Width, LayoutInput.Height));
            //dbgPrint("First Pass: ");

            // #donotcommit
            //ManualCalculateLayoutInputHorizontal();
            //ManualSetLayoutHorizontal();
            //ManualCalculateLayoutInputVertical();
            //ManualSetLayoutVertical();

            //dbgPrint("Second Pass: ");
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
