using System.Collections.Generic;
using System.Data;
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
        // TODO Addressables濡?蹂寃쏀븯湲?
        // ItemDatabase.Initialize(new ResourcesItemProvider("Items"));
    }


    #region ?몄씠釉?
    public void SaveNow()
    {
        SaveData save = new SaveData();

        // 1) ?뚮젅?댁뼱 ?몃깽?좊━
        save.inventory = playerInventory.Core.SaveData();
        // 2) ?λ퉬 李?
        save.equipped = equipInventory.Core.SaveData();
        // 3) ?붾뱶 ?쒕엻
        save.world = worldInventory.Core.SaveData();

        // 4) ?곸옄??
        Chest[] allChests = FindObjectsOfType<Chest>();
        save.chests = new List<ChestData>();
        for (int i = 0; i < allChests.Length; i++)
        {
            Chest chest = allChests[i];
            if (chest == null) continue;
            // 4) ?곸옄
            save.chests.Add(chest.SaveData());
        }

        // ?꾩뿉???쎌뼱??紐⑤뱺 ?뺣낫?????
        string json = JsonUtility.ToJson(save, true);
        string path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
        System.IO.File.WriteAllText(path, json);

        Log.Info($"Saved to {path}");
    }
    #endregion


    public void LoadNow()
    {
        if (playerInventory == null || equipInventory == null || worldInventory == null)
        {
            Log.Error("SaveManager: references are not assigned");
            return;
        }

        string path = Path.Combine(Application.persistentDataPath, "save.json");
        if (!File.Exists(path))
        {
            Log.Warn("No save file at: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        SaveData save = JsonUtility.FromJson<SaveData>(json);

        // 1) ?뚮젅?댁뼱 ?몃깽?좊━
        if (save.inventory != null)
        {
            playerInventory.Core.LoadData(save.inventory);
        }

        // 2) ?λ퉬李?
        if (save.equipped != null)
        {
            equipInventory.Core.LoadData(save.equipped);
        }

        // 3) 월드 드랍: 로드시 기존 드랍 정리 후 데이터 기반으로 재생성
        worldInventory.ClearAllDrops();
        if (save.world != null)
        {
            worldInventory.Core.LoadData(save.world);
        }

        // 4) 泥댁뒪??
        LoadChests(save);
    }
    //Chest瑜?李얠븘??媛?Chest??InventoryCore??LoadData ?붿껌
    void LoadChests(SaveData save)
    {
        
        if (save == null || save.chests == null || save.chests.Count == 0)
        {
            return;
        }

        Chest[] sceneChests = FindObjectsOfType<Chest>();
        if (sceneChests == null || sceneChests.Length == 0)
        {
            Debug.LogWarning("LoadChestsIfPresent: no Chest in scene");
            return;
        }

        Dictionary<string, Chest> byId = new Dictionary<string, Chest>();
        for (int i = 0; i < sceneChests.Length; i++)
        {
            Chest c = sceneChests[i];
            if (c == null) { continue; }
            if (string.IsNullOrWhiteSpace(c.InstanceId))
            {
                Debug.LogWarning("Chest without InstanceId: " + c.name);
                continue;
            }
            if (!byId.ContainsKey(c.InstanceId))
            {
                byId.Add(c.InstanceId, c);
            }
        }

        for (int i = 0; i < save.chests.Count; i++)
        {
            ChestData cd = save.chests[i];
            if (cd == null || string.IsNullOrWhiteSpace(cd.instanceId)) { continue; }

            Chest target;
            if (!byId.TryGetValue(cd.instanceId, out target))
            {
                Debug.LogWarning("Saved chest not found in scene: " + cd.instanceId);
                continue;
            }
            if (cd.inventory == null) { continue; }

            target.Core.LoadData(cd.inventory); // [~] ?꾩엫
        }
    }
}
