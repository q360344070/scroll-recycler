using UnityEngine;
using UnityEditor;

/*
[CustomEditor(typeof(Transform))]
class WorldPosViewer : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        Transform tx = (Transform)target;
        EditorGUILayout.Vector3Field("World Position", tx.position);
        EditorGUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}

[CustomEditor(typeof(RectTransform))]
class RectTansformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        var tx = (Transform)target;
        EditorGUILayout.Vector3Field("World Position", tx.position);
        EditorGUILayout.EndHorizontal();

        DrawDefaultInspector();
    }
}
*/

public static class TransformMenu
{
    [MenuItem("Blake/Print World Position")]
    public static void PrintGlobalPosition()
    {
        if (Selection.activeGameObject != null)
        {
            Debug.Log(Selection.activeGameObject.name + " is at " + Selection.activeGameObject.transform.position);
        }
    }
}
