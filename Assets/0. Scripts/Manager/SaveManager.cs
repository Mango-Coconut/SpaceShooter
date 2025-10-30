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
        // TODO Addressables로 변경하기
        // ItemDatabase.Initialize(new ResourcesItemProvider("Items"));
    }


    #region 세이브
    public void SaveNow()
    {
        SaveData save = new SaveData();

        // 1) 플레이어 인벤토리
        save.inventory = playerInventory.Core.SaveData();
        // 2) 장비 창
        save.equipped = equipInventory.Core.SaveData();
        // 3) 월드 드랍
        save.world = worldInventory.Core.SaveData();

        // 4) 상자들
        Chest[] allChests = FindObjectsOfType<Chest>();
        save.chests = new List<ChestData>();
        for (int i = 0; i < allChests.Length; i++)
        {
            Chest chest = allChests[i];
            if (chest == null) continue;
            // 4) 상자
            save.chests.Add(chest.SaveData());
        }

        // 위에서 읽어온 모든 정보들 저장
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

        // 1) 플레이어 인벤토리
        if (save.inventory != null)
        {
            playerInventory.Core.LoadData(save.inventory);
        }

        // 2) 장비창
        if (save.equipped != null)
        {
            equipInventory.Core.LoadData(save.equipped);
        }

        // 3) 월드 드랍
        if (save.world != null && save.world.drops != null)
        {
            worldInventory.Core.LoadData(save.world);
        }

        // 4) 체스트
        LoadChests(save);
    }
    //Chest를 찾아서 각 Chest의 InventoryCore에 LoadData 요청
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

            target.Core.LoadData(cd.inventory); // [~] 위임
        }
    }
}