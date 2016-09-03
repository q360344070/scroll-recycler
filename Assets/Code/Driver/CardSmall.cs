using UnityEngine;
using System.Collections;
using System;

public class CardRecord : CellData
{
    public string unitInstance;
}

public class CardSmall : MonoBehaviour, IRecyclableCell
{
    public void OnCellInstantiate()
    {
    }

    public void OnCellShow(CellData cellRecord)
    {
        //var cardRecord = (CardRecord)cellRecord;
        //var libraryPage = LibraryPage.inst;

        //// Set sprite
        //libraryPage.UpdateUnitUIElement(cardRecord.unitInstance, this);
    }
}
