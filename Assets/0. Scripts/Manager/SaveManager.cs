using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    const int CurrentFormatVersion = 1;
    [SerializeField] InventoryMono playerInventory;
    [SerializeField] EquipInventoryMono equipInventory;
    [SerializeField] WorldInventoryMono worldInventory;
    void Awake()
    {
        ItemDatabase.Initialize(new ResourcesItemProvider("Items"));
        // TODO Addressables로 교체하기
        // ItemDatabase.Initialize(new ResourcesItemProvider("Items"));
    }


    #region 저장
    public void SaveNow()
    {
        try
        {
            if (playerInventory == null || playerInventory.Core == null ||
                equipInventory == null || equipInventory.Core == null ||
                worldInventory == null || worldInventory.Core == null)
            {
                Log.Error("SaveManager.SaveNow: references are not assigned");
                return;
            }

            SaveData save = new SaveData();
            save.version = CurrentFormatVersion;

            // 1) 플레이어 인벤토리
            save.inventory = playerInventory.Core.SaveData();
            // 2) 장비 슬롯
            save.equipped = equipInventory.Core.SaveData();
            // 3) 월드 드랍
            save.world = worldInventory.Core.SaveData();

            // 4) 상자
            Chest[] allChests = FindObjectsOfType<Chest>();
            save.chests = new List<ChestData>();
            for (int i = 0; i < allChests.Length; i++)
            {
                Chest chest = allChests[i];
                if (chest == null) continue;
                // 상자 저장
                save.chests.Add(chest.SaveData());
            }

            // 직렬화 후 임시 파일을 이용해 원자적으로 저장
            string json = JsonUtility.ToJson(save, true);
            string path = Path.Combine(Application.persistentDataPath, "save.json");
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            string tempPath = path + ".tmp";
            string backupPath = path + ".bak";

            File.WriteAllText(tempPath, json);
            if (File.Exists(path))
            {
                File.Replace(tempPath, path, backupPath);
            }
            else
            {
                File.Move(tempPath, path);
            }

            Log.Info($"Saved to {path}");
        }
        catch (Exception ex)
        {
            Log.Error("SaveManager.SaveNow failed: " + ex.Message);
        }
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

        SaveData save = null;
        try
        {
            string json = File.ReadAllText(path);
            save = JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception ex)
        {
            Log.Error("SaveManager.LoadNow failed to read/parse: " + ex.Message);
            return;
        }

        // 저장 데이터 검증
        if (save == null)
        {
            Log.Error("SaveManager.LoadNow: parsed save was null");
            return;
        }
        if (save.version != 0 && save.version != CurrentFormatVersion)
        {
            Log.Warn($"Save format version mismatch (file={save.version}, current={CurrentFormatVersion})");
        }

        try
        {
            if (save.inventory != null)
            {
                playerInventory.Core.LoadData(save.inventory);
            }

            // 2) 장비 슬롯
            if (save.equipped != null)
            {
                equipInventory.Core.LoadData(save.equipped);
            }

            // 3) 월드 드랍 (성공적으로 파싱된 경우에만)
            // world initialization handled inside WorldInventoryCore.LoadData after successful parse
            if (save.world != null)
            {
                worldInventory.Core.LoadData(save.world);
            }

            // 4) 상자
            LoadChests(save);
        }
        catch (Exception ex)
        {
            Log.Error("SaveManager.LoadNow apply failed: " + ex.Message);
        }
    }
    // 씬 내 Chest 인벤토리 데이터 로드
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

            target.Core.LoadData(cd.inventory);
        }
    }
}
