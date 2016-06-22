using UnityEngine;
using System.Collections;

public static class Vector3Extensions
{
    public static float GetMaxX(Vector3[] vectors)
    {
        float val = float.MinValue;
        foreach (Vector3 v in vectors)
            val = Mathf.Max(val, v.x);

        return val;
    }

    public static float GetMinX(Vector3[] vectors)
    {
        float val = float.MaxValue;
        foreach (Vector3 v in vectors)
            val = Mathf.Min(val, v.x);

        return val;
    }

    public static float GetMaxY(Vector3[] vectors)
    {
        float val = float.MinValue;
        foreach (Vector3 v in vectors)
            val = Mathf.Max(val, v.y);

        return val;
    }

    public static float GetMinY(Vector3[] vectors)
    {
        float val = float.MaxValue;
        foreach (Vector3 v in vectors)
            val = Mathf.Min(val, v.y);

        return val;
    }
}
