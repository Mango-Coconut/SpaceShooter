using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class RewardSlotPanel : SlotPanel
{
    // 인벤토리에 연결되지 않은 임시 보상 표시용 리스트
    List<StoredItem> tempRewards = new List<StoredItem>();

    // 인벤토리 이벤트 구독 금지
    protected override void OnPanelEnabled() { }
    protected override void OnPanelDisabled() { }

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

        coinPanel.SetCoin(rewards.coin);
        
    }

    public void ClearRewards()
    {
        tempRewards.Clear();
        RefreshFrom(tempRewards);
    }

    // 인벤토리 대신 임시 리스트를 그리는 함수
    public void RefreshFrom(IEnumerable<StoredItem> items)
    {
        if (items == null)
        {
            for (int i = 0; i < uiSlots.Count; i++)
                uiSlots[i].Clear();
            return;
        }

        int uiIndex = 0;
        foreach (StoredItem item in items)
        {
            if (uiIndex < uiSlots.Count)
                uiSlots[uiIndex].Bind(item);
            uiIndex++;
        }

        for (int i = uiIndex; i < uiSlots.Count; i++)
            uiSlots[i].Clear();
    }
}
