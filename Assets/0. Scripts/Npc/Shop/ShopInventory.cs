using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInventory : InventoryMono
{

    [Header("Initial Items (for editor setup)")]
    [SerializeField] StoredItem[] shopitems;


    // Start is called before the first frame update
    void Start()
    {
        // 초기 아이템 등록
         if (shopitems == null) return;

        for (int i = 0; i < shopitems.Length; i++)
            {
            StoredItem item = shopitems[i];
            if (item.itemData == null)
            {
                Log.Error($"ShopInventory 초기 아이템 누락: index={i}");
                continue;
            }

            TryAddItem(item);
        }
    }
}
