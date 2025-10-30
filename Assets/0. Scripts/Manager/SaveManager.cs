using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] InventoryMono playerInventory;
    [SerializeField] EquipInventoryMono equipInventory;
    [SerializeField] WorldInventoryMono worldInventory;
    void Awake()
    {
        ItemDatabase.Initialize(new ResourcesItemProvider("Items"));
        // TODO Addressables로 변경하기
        // ItemDatabase.Initialize(new ResourcesItemProvider("Items"));
    }


    #region 세이브
    public void SaveNow()
    {
        SaveData save = new SaveData();

        // 플레이어 인벤, 장비, 월드드랍
        save.inventory = playerInventory.Core.SaveData();     // 1
        save.equipped = equipInventory.Core.SaveData();      // 2
        save.world = worldInventory.Core.SaveData();      // 3

        // 상자들
        Chest[] allChests = FindObjectsOfType<Chest>();
        save.chests = new List<ChestData>();
        for (int i = 0; i < allChests.Length; i++)
        {
            Chest chest = allChests[i];
            if (chest == null) continue;
            save.chests.Add(chest.SaveData());               // 4
        }

        string json = JsonUtility.ToJson(save, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"Saved to {path}");
    }
    #endregion


    [Header("World Drop Instantiate")]
    [SerializeField] DroppedItem droppedItemPrefab;       // 드랍 프리팹 :contentReference[oaicite:3]{index=3}

    public void LoadNow()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("No save file at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        SaveData save = JsonUtility.FromJson<SaveData>(json); // SaveData: inventory/equipped/world (+chests 예정) :contentReference[oaicite:4]{index=4}

        // 1) 플레이어 인벤토리
        if (save.inventory != null)
        {
            int capacity = save.inventory.capacity;
            List<StoredItem> items = new List<StoredItem>();
            if (save.inventory.slots != null)
            {
                for (int i = 0; i < save.inventory.slots.Count; i++)
                {
                    StoredItemData sd = save.inventory.slots[i];
                    StoredItem s = StoredItem.RestoreFromData(sd); // ★ 핵심
                    if (s != null)
                    {
                        items.Add(s);
                    }
                }
            }
            playerInventory.Core.RestoreExact(capacity, items);
        }

        // 2) 장비
        if (save.equipped != null)
        {
            equipInventory.Core.RestoreExact(save.equipped.equippedSlots); // 내부에서 RestoreFromData 사용
        }

        // 3) 월드 드랍 (기존 것 정리 후 재생성 권장)
        if (save.world != null && save.world.drops != null)
        {
            // (기존 드랍 정리)
            ClearAllWorldDrops();

            for (int i = 0; i < save.world.drops.Count; i++)
            {
                WorldDropEntry entry = save.world.drops[i];

                StoredItem s = StoredItem.RestoreFromData(entry.storedItem); // ★ 핵심
                if (s == null)
                {
                    continue;
                }

                Vector3 pos = new Vector3(entry.position.x, entry.position.y, entry.position.z);
                Quaternion rot = Quaternion.Euler(entry.rotationEuler.x, entry.rotationEuler.y, entry.rotationEuler.z);

                DroppedItem di = Instantiate(droppedItemPrefab, pos, rot);
                di.Bind(s);
                di.SetWorldInventory(worldInventory);
                worldInventory.Core.RegisterExistingDrop(di);
            }
        }

        // 4) 상자(Chest) – SaveData에 chests를 추가했다면 사용
        // LoadChestsIfPresent(save);
    }

    void ClearAllWorldDrops()
    {
        // 선택적: 현재 씬의 DroppedItem을 찾아 정리해주고 Core에서도 Unregister
        DroppedItem[] all = FindObjectsOfType<DroppedItem>();
        for (int i = 0; i < all.Length; i++)
        {
            DroppedItem di = all[i];
            if (di == null) continue;
            worldInventory.Core.UnregisterDrop(di);      // 목록 제거 + OnChanged :contentReference[oaicite:13]{index=13}
            Destroy(di.gameObject);
        }
    }

    // -------- 재구성 유틸 --------

    StoredItem RebuildStoredItem(StoredItemData d)
    {
        if (d == null) return null;
        ItemData refData = ItemDatabase.Get(d.itemDataId); // id -> ScriptableObject (직접 구현)
        if (refData == null)
        {
            Debug.LogError("ItemData not found for id: " + d.itemDataId);
            return null;
        }

        StoredItem s = new StoredItem(refData, d.count);   // 생성자 기본 로직 :contentReference[oaicite:14]{index=14}
        s.instanceId = d.instanceId;                       // 세이브된 인스턴스 유지가 핵심
        return s;
    }

    void RebuildInventory(PlayerInventoryData src, out int capacity, out List<StoredItem> items)
    {
        capacity = src.capacity;                           // :contentReference[oaicite:15]{index=15}
        items = new List<StoredItem>();

        if (src.slots == null) return;
        for (int i = 0; i < src.slots.Count; i++)
        {
            StoredItemData sd = src.slots[i];
            StoredItem s = RebuildStoredItem(sd);
            if (s != null) items.Add(s);
        }
    }
}