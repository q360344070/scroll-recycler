using UnityEngine;
using System.Collections;

// Test commit

public class ChatView : MonoBehaviour
{
    public ScrollRecycler ScrollRecycler;
    public GameObject ChatSectionPrefab;

    void Start()
    {
        ScrollRecycler
            .InstantiateCellLayout(ChatSectionPrefab)
            .GetCellLayout().AddCells(
                new ChatCellData() { Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit." });
    }
}
