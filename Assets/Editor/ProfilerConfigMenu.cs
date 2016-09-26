using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class ProfilerUI : EditorWindow
{
    string LoadFileName;
    string WriteFileName;

    [MenuItem("User/ProfilerUI")]
    static void Launch()
    {
        var window = EditorWindow.GetWindow<ProfilerUI>("Profiler UI");
        window.Show();
    }

    public void OnGUI()
    {
        //string pathToAssetsFolder = UnityEngine.Application.dataPath;
        //pathToAssetsFolder = pathToAssetsFolder.Replace("/", "\\");


        //LoadFileName = EditorGUILayout.TextField("File Name", LoadFileName);
        //string fullPath = Application.dataPath + "/" + LoadFileName;

        //if (GUILayout.Button("Load Profiled Data"))
        //{
        //    if (File.Exists(LoadFileName))
        //    {
        //        Profiler.AddFramesFromFile(LoadFileName);
        //    }
        //    else
        //    {
        //        Debug.Log("Could not find profile data at path: " + fullPath);
        //    }
        //}

        //WriteFileName = EditorGUILayout.TextField("File Name", WriteFileName);
        //if (GUILayout.Button("Save Profiler Data"))
        //{
        //    FileUtil.CopyFileOrDirectory(pathToAssetsFolder + "\\profilerLog.txt", pathToAssetsFolder + "\\" + WriteFileName);
        //    FileUtil.CopyFileOrDirectory(pathToAssetsFolder + "\\profilerLog.txt.data", pathToAssetsFolder + "\\" + WriteFileName);
        //}
    }
}
