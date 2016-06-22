using UnityEngine;
using System.Collections;

public interface IRecyclerCell
{
    // After a cell has been instantiated by the pool, the cell needs to define how it will be initialized
    void OnCellInstantiate();
    // Cells need to define how they are shown by their recyclers
    void OnCellShow(CellRecord cellRecord);
}
