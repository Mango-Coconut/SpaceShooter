using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInventory : MonoBehaviour, IItemSource, IItemSink
{
    [SerializeField] List<DroppedItem> worldItems = new List<DroppedItem>();

    // Start is called before the first frame update
    void Start()
    {
        // 씬 내 DroppedItem 전부 탐색
        DroppedItem[] droppedItems = FindObjectsOfType<DroppedItem>();

        // worldItems 리스트 초기화
        worldItems.Clear();

        // 각 DroppedItem에서 ItemData 추출해서 리스트에 추가
        foreach (DroppedItem dropped in droppedItems)
        {
            if (dropped != null && dropped.item.itemData != null)
            {
                dropped.SetWorldInventory(this);
                worldItems.Add(dropped);
            }
        }

        Debug.Log($"WorldInventory 초기화: {worldItems.Count}개 아이템 등록됨");
    }

    public void NotifyPickedUp(DroppedItem di)
    {
        if (di == null) return;
        worldItems.Remove(di);
    }

    public bool CanRemoveItem(StoredItem item)
    {
        throw new System.NotImplementedException();
    }

    public bool TryRemoveItem(StoredItem item)
    {
        throw new System.NotImplementedException();
    }

    public bool CanAddItem(StoredItem item)
    {
        throw new System.NotImplementedException();
    }

    public bool TryAddItem(StoredItem item)
    {
        throw new System.NotImplementedException();
    }
}
