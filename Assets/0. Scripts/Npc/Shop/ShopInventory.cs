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
        if (shopitems != null)
        {
            foreach (var item in shopitems)
            {
                if (item.itemData == null)
                {
                    Log.Error($"shop에 아이템 지정하기 실패");
                    break;
                }
                TryAddItem(item);
            }
        }
    }
}
