using System;
using UnityEngine;
using System.Reflection;
using System.Collections;

public static class MonoBehaviourExtensions
{
    // BE CAREFUL USING THIS FUNCTION -bt -jc
    // This function copies all of the references in one component to another, this includes references to
    // other objects. For instance, if the first component has a reference to a text field -> making
    // changes to the text field in the _second component_ will also change the text field in the first component.
    // You have been warned.
    public static T CopyFieldsFrom<T>(this Component com, T other) where T : Component
    {
        Type type = com.GetType();
        if (type != other.GetType()) { return null; } // Make sure only copy fields between same components

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
            | BindingFlags.Default;

        PropertyInfo[] pInfos = type.GetProperties(flags);
        foreach (var currPInfo in pInfos)
        {
            if (currPInfo.CanWrite)
            {
                try { currPInfo.SetValue(com, currPInfo.GetValue(other, null), null); }
                catch { } // In case of NotImplementedException being thrown
            }
        }

        FieldInfo[] fInfos = type.GetFields(flags);
        foreach (var currFInfo in fInfos)
        {
            currFInfo.SetValue(com, currFInfo.GetValue(other));
        }

        return com as T;
    }
}
