using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChatViewForked : MonoBehaviour 
{
    public ScrollRectForked ScrollRect;

    int CellCount = 1000;

    void Start()
    {
        for (int i = 0; i < CellCount; ++i)
        {
            ResourceCache.Inst.Create("ChatItem").transform.SetParent(ScrollRect.content, false);
        }
    }
}
