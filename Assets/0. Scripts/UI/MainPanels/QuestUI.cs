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
        QuestSlot existingSlot = questSlots.Find(slot => slot.Data == quest.data);

        if (existingSlot != null)
        {
            existingSlot.Set(quest);
        }
        else
        {
            QuestSlot newSlot = Instantiate(questSlotPrefab, QuestSlotPanel.transform);
            newSlot.Set(quest);
            questSlots.Add(newSlot);
        }
    }
}
