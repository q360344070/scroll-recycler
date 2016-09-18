using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class ProfilerUI : EditorWindow
{
    string Filename;

    [MenuItem("User/ProfilerUI")]
    static void Launch()
    {
        var window = EditorWindow.GetWindow<ProfilerUI>("Profiler UI");
        window.Show();
    }

    public void OnGUI()
    {
        Filename = EditorGUILayout.TextField("Item Name", Filename);
        string fullPath = Application.dataPath + "/" + Filename;

        if (GUILayout.Button("Load Profiled Data"))
        {
            if (File.Exists(Filename))
            {
                Profiler.AddFramesFromFile(Filename);
            }
            else
            {
                Debug.Log("Could not find profile data at path: " + fullPath);
            }
        }

        // #donotcommit
        if (GUILayout.Button("Print Filename Path"))
        {
            Debug.Log(Application.dataPath + "/" + Filename);
        }
    }
}
