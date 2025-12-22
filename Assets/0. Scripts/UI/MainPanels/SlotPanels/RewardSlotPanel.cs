using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class RewardSlotPanel : MonoBehaviour
{
    List<StoredItem> tempRewards = new List<StoredItem>();

    public GameObject slotPrefab;

    protected ItemPanelEventAggregator forwarder;
    public ItemPanelEventAggregator Forwarder => forwarder;

    [SerializeField] Transform slotRoot;
    public readonly List<IInteractiveView<StoredItem>> uiSlots = new List<IInteractiveView<StoredItem>>();

    void OnEnable()
    {
    }


    protected void EnsureGetComponent()
    {
        if (forwarder != null) return;
        else forwarder = GetComponent<ItemPanelEventAggregator>();
    }

    // 인벤토리 세팅 시 슬롯UI 재생성
    protected void SetSlot(int targetCount)
    {
        uiSlots.Clear();

        // 1) slotRoot 아래에서만 슬롯들을 수집 (비슬롯 무시)
        List<IInteractiveView<StoredItem>> existingSlots = new List<IInteractiveView<StoredItem>>();
        for (int i = 0; i < slotRoot.childCount; i++)
        {
            Transform child = slotRoot.GetChild(i);
            IInteractiveView<StoredItem> slot = child.GetComponent<IInteractiveView<StoredItem>>();
            if (slot != null)
            {
                existingSlots.Add(slot);
            }
            else
            {
                // slotRoot 아래엔 슬롯만 있어야 함
                Log.Warn($"{name}: Non-slot child found under slotRoot: {child.name}");
            }
        }

        // 2) 부족하면 생성 slotRoot 아래에
        while (existingSlots.Count < targetCount)
        {
            GameObject go = Instantiate(slotPrefab, slotRoot);
            IInteractiveView<StoredItem> slot = go.GetComponent<IInteractiveView<StoredItem>>();

            existingSlots.Add(slot);
        }

        // 3) targetCount만큼 uiSlots에 등록
        for (int i = 0; i < targetCount && i < existingSlots.Count; i++)
        {
            uiSlots.Add(existingSlots[i]);
        }

        // 4) 초과 슬롯 삭제
        for (int i = existingSlots.Count - 1; i >= targetCount; i--)
        {
            GameObject go = existingSlots[i].GO; // IInteractiveView에 GO 프로퍼티가 있다고 가정
            Destroy(go);
        }
    }

    // 외부에서 퀘스트 보상 리스트를 전달받아 표시
    public void ShowRewards(QuestReward rewards)
    {
        SetSlot(rewards.items.Count);

        for (int i = 0; i < rewards.items.Count; i++)
        {
            uiSlots[i].Bind(new StoredItem(rewards.items[i].itemData, rewards.items[i].amount));
        }

        // 남는 슬롯은 비우기
        for (int i = rewards.items.Count; i < uiSlots.Count; i++)
        {
            uiSlots[i].Clear();
        }


        EnsureGetComponent();
        forwarder.RebuildViews(uiSlots);

        Log.Info($"[RewardSlotPanel] childCount={transform.childCount} uiSlots={uiSlots.Count}");
        for (int i = 0; i < uiSlots.Count; i++)
        {
            GameObject go = uiSlots[i].GO;
            Log.Info($"slot[{i}] activeInHierarchy={go.activeInHierarchy} name={go.name}");
        }
    }

}
