using UnityEngine;
using System.Collections;

public class ChatView : MonoBehaviour
{
    public ScrollRecyclerArbitrary ScrollRecycler;
    public GameObject ChatSectionPrefab;

    void Start()
    {
        ScrollRecycler
            .InstantiateCellLayout(ChatSectionPrefab)
            .GetCellLayout().AddCells(
                new ChatCellData() { Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit." });
    }
}
