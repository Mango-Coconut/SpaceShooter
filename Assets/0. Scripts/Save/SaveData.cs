using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 2;
    public PlayerInventoryData inventory;
    public EquipData equipped;
    public WorldDropData world;
    public List<ChestData> chests;
    public List<NpcData> npcs;
}
