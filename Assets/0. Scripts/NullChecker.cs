using UnityEngine;
using System.Diagnostics;

public static class NullChecker
{
#if UNITY_EDITOR
    /// <summary>
    /// null 상태 추적 (빌드 시 자동 제거)
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void NullCheck(Object context, string varName = null)
    {
        if (context == null)
        {
            UnityEngine.Debug.LogError("❌ [NullChecker] Context is null — cannot trace object.");
            return;
        }

        GameObject go = (context as Component)?.gameObject ?? context as GameObject;
        string objName = context.name;
        string sceneName = go != null ? go.scene.name : "(No Scene)";
        string path = go != null ? GetFullPath(go) : "(No Path)";
        bool active = go != null && go.activeInHierarchy;

        string variable = string.IsNullOrEmpty(varName) ? "" : $" → {varName}";

        UnityEngine.Debug.LogError(
            $"❌ [NullChecker] Null Reference{variable} detected in '{objName}'" +
            $"\n  • Scene: {sceneName}" +
            $"\n  • ActiveInHierarchy: {active}" +
            $"\n  • Object Path: {path}",
            context
        );
    }

    private static string GetFullPath(GameObject go)
    {
        if (go == null) return "(no object)";
        string path = go.name;
        Transform t = go.transform;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
#else
    // ✅ 빌드 시 완전 제거
    [Conditional("UNITY_EDITOR")]
    public static void NullCheck(Object context, string varName = null) { }
#endif
}