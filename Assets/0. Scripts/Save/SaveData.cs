using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public PlayerInventoryData inventory;
    public EquipData equipped;
    public WorldDropData world;
    public List<ChestData> chests;
}