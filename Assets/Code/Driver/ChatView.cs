using UnityEngine;
using System.Collections;

public class ChatView : MonoBehaviour
{
    public ScrollRecycler ScrollRecycler;
    public GameObject ChatSectionPrefab;

    void Start()
    {
        LayoutRecycler lr = ScrollRecycler
            .InstantiateRecyclerLayout(ChatSectionPrefab, ScrollRecycler.ScrollRect.content);

        lr.AddRecord(CreateRecord("Lorem ipsum dolor sit amet, consectetur adipiscing elit."));
    }

    public ChatRecord CreateRecord(string message)
    {
        return new ChatRecord()
        {
            Message = message
        };
    }
}
