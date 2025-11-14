using System.Collections.Generic;
using UnityEngine;

public class ResourcesItemProvider : IItemProvider
{
    // 필요시 폴더 경로를 바꿔서 부분 로드 가능 (예: "Items")
    readonly string loadPath;
    Dictionary<string, ItemData> map;
    bool initialized;

    public ResourcesItemProvider() : this(string.Empty) { }

    public ResourcesItemProvider(string resourcesLoadPath)
    {
        loadPath = resourcesLoadPath; // "" 면 Resources 전체에서 ItemData 검색
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        ItemData[] all = Resources.LoadAll<ItemData>(loadPath);
        map = new Dictionary<string, ItemData>(all.Length);

        for (int i = 0; i < all.Length; i++)
        {
            ItemData item = all[i];
            if (item == null)
            {
                continue;
            }
            if (string.IsNullOrWhiteSpace(item.id))
            {
                Debug.LogWarning("ResourcesItemProvider: Item '" + item.name + "' has empty id. Skipped.");
                continue;
            }
            if (map.ContainsKey(item.id))
            {
                Debug.LogError("ResourcesItemProvider: Duplicate id '" + item.id + "' (" + item.name + ")");
                continue;
            }
            map.Add(item.id, item);
        }

        initialized = true;
    }

    public bool TryGet(string id, out ItemData item)
    {
        if (!initialized)
        {
            Initialize();
        }
        if (string.IsNullOrWhiteSpace(id))
        {
            item = null;
            return false;
        }
        return map.TryGetValue(id, out item);
    }

    public IEnumerable<string> GetAllIds()
    {
        if (!initialized)
        {
            Initialize();
        }
        return map.Keys;
    }
}