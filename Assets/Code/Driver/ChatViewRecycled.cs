using UnityEngine;
using System.Collections;

public class ChatViewRecycled : MonoBehaviour
{
    public ScrollRecycler ScrollRecycler;
    public GameObject ChatSectionPrefab;

    void Start()
    {
        ScrollRecycler
            .InstantiateCellLayout(ChatSectionPrefab)
            .AddCells(new ChatCellData() { Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit." });
    }
}
