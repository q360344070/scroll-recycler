using UnityEngine;
using System.Collections;

public class ChatView : MonoBehaviour
{
    public ScrollRecycler ScrollRecycler;
    public GameObject ChatSectionPrefab;

    void Start()
    {
        ICellLayout lr = ScrollRecycler
            .InstantiateCellLayout(ChatSectionPrefab);

        lr.GetCellLayout().AddCell(CreateCellData("Lorem ipsum dolor sit amet, consectetur adipiscing elit."));
    }

    public ChatCellData CreateCellData(string message)
    {
        return new ChatCellData()
        {
            Message = message
        };
    }
}
