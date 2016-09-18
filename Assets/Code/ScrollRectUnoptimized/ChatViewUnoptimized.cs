using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ChatViewUnoptimized : MonoBehaviour 
{
    public ScrollRect ScrollRect;

    void Start()
    {
        int childCount = 1000;
        for (int i = 0; i < childCount; ++i)
        {
            GameObject chatItem = ResourceCache.Inst.Create("ChatItem");
            chatItem.transform.SetParent(ScrollRect.content);
        }
    }
}
