using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    public bool TryAddItem(IItemSink c, ItemData data, int amount = 1)
    {
        return c.TryAddItem(new StoredItem(data, amount));
    }
    public bool TryAddItem(IItemSink c, StoredItem item)
    {
        return c.TryAddItem(item);
    }
    public bool TryRemoveItem(IItemSource c, ItemData data, int amount = 1)
    {
        return c.TryRemoveItem(new StoredItem(data, amount));
    }
    public bool TryRemoveItem(IItemSource c, StoredItem item)
    {
        return c.TryRemoveItem(item);
    }
    // 일반 전달 (Source -> Sink)
    public static bool TryDeliver(IItemSource from, IItemSink to, StoredItem item)
    {
        if (from == null || to == null || item == null) return false;
        if (!from.CanRemoveItem(item)) return false;
        if (!to.CanAddItem(item)) return false;

        // 사전 검증으로 충분히 보장된 상태라면 이 아래는 거의 실패하지 않음
        from.TryRemoveItem(item);
        to.TryAddItem(item);
        return true;
    }

    public static bool TryDeliverSwap(IItemSource from, ISwapSink to, StoredItem item, IItemSink originSink)
{
    if (from == null || to == null || originSink == null || item == null) return false;

    // 1) 사전검증: 꺼낼 수 있는가?
    if (!from.CanRemoveItem(item)) return false;

    // 2) 사전검증: 장비창이 이번 아이템을 받을 수 있는가? 그리고 무엇이 튀어나오는가?
    if (!to.CanAddItemSwap(item, out var willBeSwapped)) return false;

    // 3) 사전검증: 튀어나올 아이템이 있다면 원래 자리(or 지정된 곳)가 받을 수 있는가?
    if (willBeSwapped != null && !originSink.CanAddItem(willBeSwapped)) return false;

    // === 여기까지 오면 실패하지 않도록 보장됨 ===

    // 4) 실제 실행
    from.TryRemoveItem(item);                        // 꺼내기
    to.TryAddItemSwap(item, out var swapped);        // 장비창에 넣고, 기존 장비를 받음 (swapped는 willBeSwapped와 동일해야 함)

    // 5) 기존 장비를 원래 자리로 복귀
    if (swapped != null)
    {
        originSink.TryAddItem(swapped);
    }

    return true;
}

    #region 싱글톤
    private static InventoryManager instance;
    public static InventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[InventoryManager] Instance is null. Make sure a GameObject with InventoryManager exists in the scene.");
#endif
            }
            return instance;
        }
    }

    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[InventoryManager] Duplicate detected. Destroying this component.", this);
#endif
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (dontDestroyOnLoad == true)
        {
            DontDestroyOnLoad(gameObject);
        }

        // TODO: 초기화 필요 시 여기서 수행
        // Init();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion


}