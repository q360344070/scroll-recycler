using UnityEngine;
using System.Collections;
using System;

public class CardRecord : CellRecord
{
    public string unitInstance;
}

public class CardSmall : MonoBehaviour, IRecyclerCell
{
    public void OnCellInstantiate()
    {
    }

    public void OnCellShow(CellRecord cellRecord)
    {
        //var cardRecord = (CardRecord)cellRecord;
        //var libraryPage = LibraryPage.inst;

        //// Set sprite
        //libraryPage.UpdateUnitUIElement(cardRecord.unitInstance, this);
    }
}
