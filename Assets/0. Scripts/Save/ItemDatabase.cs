using System.Collections.Generic;
using UnityEngine;

public static class ItemDatabase
{
    static IItemProvider provider;
    static bool initialized;

    // 앱 시작 시 한 번만 (GameManager.Awake 또는 SaveManager.Awake 등에서)
    public static void Initialize(IItemProvider itemProvider)
    {
        if (initialized)
        {
            return;
        }
        provider = itemProvider != null ? itemProvider : new ResourcesItemProvider(); // 기본값
        provider.Initialize();
        initialized = true;
    }

    public static ItemData Get(string id)
    {
        if (!initialized)
        {
            Initialize(new ResourcesItemProvider());
        }
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning("ItemDatabase.Get: id is null or empty");
            return null;
        }
        ItemData item;
        if (provider.TryGet(id, out item))
        {
            return item;
        }
        Debug.LogError("ItemDatabase.Get: not found id=" + id);
        return null;
    }

    public static bool TryGet(string id, out ItemData item)
    {
        if (!initialized)
        {
            Initialize(new ResourcesItemProvider());
        }
        return provider.TryGet(id, out item);
    }

    public static IEnumerable<string> GetAllIds()
    {
        if (!initialized)
        {
            Initialize(new ResourcesItemProvider());
        }
        return provider.GetAllIds();
    }
}