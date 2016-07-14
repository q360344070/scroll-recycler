using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class ChatItem : MonoBehaviour, IRecyclerCell
{
    public Text Message;

    public void OnCellInstantiate()
    {
    }

    public void OnCellShow(CellRecord cellRecord)
    {
        var cr = (ChatRecord)cellRecord;
        Message.text = cr.Message;
    }
}

public class ChatRecord : CellRecord
{
    public string Message;
}
