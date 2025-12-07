#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class ItemDataValidator
{   
    // ItemData.id (ScriptableObject) 중복 여부 확인
    static ItemDataValidator()
    {
        var items = Resources.LoadAll<ItemData>("");
        var ids = new HashSet<string>();
        foreach (var i in items)
        {
            if (!ids.Add(i.id))
                Debug.LogError($"Duplicate ItemData ID: {i.id} ({i.name})");
        }
    }
}
#endif