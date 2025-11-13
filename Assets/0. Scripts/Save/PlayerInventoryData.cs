using System;
using System.Collections.Generic;

[Serializable]
public class PlayerInventoryData
{
    public int coin;
    public int capacity;
    public List<StoredItemData> slots;
}