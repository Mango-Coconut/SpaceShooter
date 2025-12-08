using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField] GameEventHub hub;
    [SerializeField] GameObject QuestSlotPanel;
    List<QuestSlot> questSlots = new List<QuestSlot>();
    [SerializeField] QuestSlot questSlotPrefab;

    void Awake()
    {
        QuestSlotPanel.SetActive(false);
    }
    void OnEnable()
    {
        hub.quest.OnQuestStateChanged += HandleQuestStateChange;
    }

    void OnDisable()
    {
        hub.quest.OnQuestStateChanged -= HandleQuestStateChange;
    }

    void Update()
    {
        if(hasActiveOrReady) return;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if(questSlots.Count != 0) QuestSlotPanel.SetActive(true);
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            QuestSlotPanel.SetActive(false);
        }
    }

    bool hasActiveOrReady;
    void HandleQuestStateChange(QuestInstance quest)
    {
        QuestSlot existingSlot = questSlots.Find(slot => slot.QuestInstance.data == quest.data);

        if (quest.state == QuestState.Completed && quest.data.isRepeatable)
        {
            if (existingSlot != null)
            {
                questSlots.Remove(existingSlot);
                Destroy(existingSlot.gameObject);
            }
        }
        else
        {
            // 나머지 상태는 갱신/생성
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

        hasActiveOrReady =
            questSlots.Any(slot =>
                slot.QuestInstance.state == QuestState.Active ||
                slot.QuestInstance.state == QuestState.ReadyToTurnIn);

        // 패널 활성/비활성 토글
        QuestSlotPanel.SetActive(hasActiveOrReady);
    }
}
