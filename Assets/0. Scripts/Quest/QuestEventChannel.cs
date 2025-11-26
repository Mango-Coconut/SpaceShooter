using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest/QuestEventChannel")]
public class QuestEventChannel : ScriptableObject
{
    // 퀘스트 시작 요청 (어떤 NPC에서 시작했는지도 같이 보냄)
    public event Action<QuestData, NpcMono> OnQuestStartRequested;
    public event Action<QuestData, NpcMono> OnQuestCompleteRequested;

    public void RaiseQuestStartRequested(QuestData quest, NpcMono fromNpc)
    {
        if (OnQuestStartRequested != null)
        {
            OnQuestStartRequested.Invoke(quest, fromNpc);
        }
        else
        {
            Debug.LogWarning(
                string.Format("QuestEventChannel: OnQuestStartRequested has no listeners. quest = {0}",
                    quest != null ? quest.questName : "null")
            );
        }
    }

    public void RaiseQuestCompleteRequested(QuestData quest, NpcMono npc)
    {
        if (OnQuestStartRequested != null)
        {
            OnQuestCompleteRequested.Invoke(quest, npc);
        }
        else
        {
            Debug.LogWarning(
                string.Format("QuestEventChannel: OnQuestCompleteRequested has no listeners. quest = {0}",
                    quest != null ? quest.questName : "null")
            );
        }
    }
}