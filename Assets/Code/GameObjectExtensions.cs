using UnityEngine;
using System.Collections;

public static class GameObjectExtensions
{
    public static void TryCopyComponent<T>(this GameObject go, Component comp) where T : Component
    {
        if (typeof(T).IsAssignableFrom(comp.GetType()))
        {
            go.AddComponent<T>().CopyFieldsFrom(comp);
        }
    }
}
