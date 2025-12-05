using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField] GameEventHub hub;
    [SerializeField] GameObject QuestSlotPanel;
    List<QuestSlot> questSlots = new List<QuestSlot>();
    [SerializeField] QuestSlot questSlotPrefab;

    void OnEnable()
    {
        hub.quest.OnQuestStateChanged += HandleQuestStateChange;
    }

    void OnDisable()
    {
        hub.quest.OnQuestStateChanged -= HandleQuestStateChange;
    }



    void HandleQuestStateChange(QuestInstance quest)
    {
        if(quest.state == QuestState.Active)
        {
            QuestSlot newQuest = Instantiate(questSlotPrefab, QuestSlotPanel.transform);
            questSlots.Add(newQuest);
            newQuest.Set(quest);
        }
        foreach(QuestSlot slot in questSlots)
        {
            if(slot.Data == quest.data)
            {
                slot.Set(quest);
            }
        }
    }
}
