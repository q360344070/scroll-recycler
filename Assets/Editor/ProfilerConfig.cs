using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class Startup
{
    static Startup()
    {
        Debug.Log("Up and running");

        // write FPS to "profilerLog.txt"
        // persistentDataPath, because dataPath seems to be readOnly under iOS
        Profiler.logFile = "Assets/profilerLog.txt";
        // write Profiler Data to "profilerLog.txt.data"                                                                                        
        Profiler.enableBinaryLog = true;
        Profiler.enabled = true;
    }
}
