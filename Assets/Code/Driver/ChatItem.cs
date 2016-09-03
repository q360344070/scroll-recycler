using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class ChatItem : MonoBehaviour, IRecyclableCell
{
    public Text Message;

    public void OnCellInstantiate()
    {
    }

    public void OnCellShow(CellData cellRecord)
    {
        var cr = (ChatCellData)cellRecord;
        Message.text = cr.Message;
    }
}

public class ChatCellData : CellData
{
    public string Message;
}
