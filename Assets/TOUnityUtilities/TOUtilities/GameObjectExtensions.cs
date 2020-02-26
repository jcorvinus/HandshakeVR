using UnityEngine;
using System.Collections;

public static class GameObjectExtensions
{
    public static T GetComponentInChildren<T>(this Component g, GetComponentSafety safety) where T : Component
    {
        var c = g.GetComponentInChildren<T>();
#if !UNITY_EDITOR
        Debug.LogWarning("Safe GetComponent being used. Slowness may ensue");
#endif
        if ((safety & GetComponentSafety.NoNullExpected) == GetComponentSafety.NoNullExpected)
        {
            if (c == null) Debug.LogError("No Component of type " + (typeof(T)).ToString() + " on Game Object " + g.gameObject.name);
        }
        if ((safety & GetComponentSafety.SingleResultExpected) == GetComponentSafety.SingleResultExpected)
        {
            if (g.GetComponents<T>().Length > 1) Debug.LogError("More than one component of type " + (typeof(T)).ToString() + " on Game Object " + g.gameObject.name);
        }
        return c;
    }
    public static T GetComponent<T>(this Component g, GetComponentSafety safety) where T : Component
    {
        var c = g.GetComponent<T>();
#if !UNITY_EDITOR
        Debug.LogWarning("Safe GetComponent being used. Slowness may ensue");
#endif
        if ((safety & GetComponentSafety.NoNullExpected) == GetComponentSafety.NoNullExpected)
        {
            if (c == null) Debug.LogError("No Component of type " + (typeof(T)).ToString() + " on Game Object " + g.gameObject.name);
        }
        if ((safety & GetComponentSafety.SingleResultExpected) == GetComponentSafety.SingleResultExpected)
        {
            if (g.GetComponents<T>().Length > 1) Debug.LogError("More than one component of type " + (typeof(T)).ToString() + " on Game Object " + g.gameObject.name);
        }
        return c;
    }
    public enum GetComponentSafety
    {
        None = 0,
        NoNullExpected = 1,
        SingleResultExpected = 2
    }
}