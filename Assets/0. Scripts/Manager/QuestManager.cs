using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] GameEventHub hub;

    
    Dictionary<string, QuestInstance> activeQuests;

    void OnEnable()
    {
        if (hub != null && hub.quest != null)
        {
            hub.quest.OnQuestStartRequested += HandleQuestStartRequested;
            hub.quest.OnQuestCompleteRequested += HandleQuestCompleteRequested;
        }
    }

    void OnDisable()
    {
        if (hub != null && hub.quest != null)
        {
            hub.quest.OnQuestStartRequested -= HandleQuestStartRequested;
            hub.quest.OnQuestCompleteRequested -= HandleQuestCompleteRequested;
        }
    }

    void HandleQuestStartRequested(QuestData quest, NpcMono npc)
    {
        if (quest == null)
        {
            Debug.LogWarning("QuestManager: quest is null in HandleQuestStartRequested.");
            return;
        }

        // 1. 선행 퀘스트/레벨 조건 체크
        // 2. 이미 진행/완료 여부 체크
        // 3. 통과하면 실제 QuestInstance 생성 + 등록
        // 4. UI, 토스트, 로그 등 필요시 추가 처리

        Debug.Log(
            string.Format("퀘스트 시작 요청: {0} (from NPC: {1})",
                quest.questName,
                npc != null ? npc.name : "null")
        );
    }
    void HandleQuestCompleteRequested(QuestData quest, NpcMono npc)
    {
        // 여기서:
        // 1) 해당 퀘스트 상태가 ReadyToTurnIn인지 확인
        // 2) 맞으면 Complete + reward 지급
    }



    void OnObjectiveProgressChanged(QuestInstance quest)
    {
        // 1️⃣ 모든 목표가 완료되었는지 검사
        bool allDone = AreAllObjectivesDone(quest);

        // 2️⃣ 아직 진행 중이었다면 상태 전환
        if (allDone && quest.state == QuestState.Active)
        {
            quest.state = QuestState.ReadyToTurnIn;

            // 플레이어에게 알림 띄우기 등
            Debug.Log($"[{quest.data.questName}] 목표 전부 완료! 보고하러 가세요.");

            // UI나 알림 시스템으로 이벤트 브로드캐스트할 수도 있음
            // e.g. hub.quest.RaiseQuestReadyToTurnIn(quest.data);
        }

        // 3️⃣ 일부만 완료된 경우엔 그냥 진행도만 업데이트
    }

    bool AreAllObjectivesDone(QuestInstance quest)
    {
        return quest.objectives.All(o => o.IsCompleted);
    }
}
