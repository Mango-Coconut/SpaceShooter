using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [SerializeField] GameEventHub hub;


    Dictionary<string, QuestInstance> activeQuests = new Dictionary<string, QuestInstance>();

    List<QuestData> completedQuests = new List<QuestData>();

    public static QuestManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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

    public QuestState GetQuestState(QuestData quest)
    {
        // 1. 이미 완료했는지
        if (completedQuests.Contains(quest))
            return QuestState.Completed;

        // 2. 현재 진행 중인지
        QuestInstance instance;
        if (activeQuests.TryGetValue(quest.id, out instance))
        {
            if (instance.AreAllObjectivesDone())
                return QuestState.ReadyToTurnIn;
            return QuestState.Active;
        }

        // 3. 진행 중인 인스턴스 없음 → 조건 검사
        if (!CheckPrerequisites(quest))
            return QuestState.Locked;

        // 4. 조건 충족했으면 수락 가능
        return QuestState.CanAccept;
    }

    void HandleQuestStartRequested(QuestData quest, NpcMono npc)
    {
        bool started = TryStartQuest(quest, npc);
        if (started)
        {
            npc.EnrollQuest(quest);
        }
    }
    public bool TryStartQuest(QuestData quest, NpcMono giver)
    {
        if (quest == null)
        {
            Debug.LogWarning("TryStartQuest: quest is null.");
            return false;
        }

        // 현재 상태 확인
        QuestState state = GetQuestState(quest);

        // 수락 가능한 상태가 아니면 거부
        if (state == QuestState.Locked)
        {
            Debug.Log(
                string.Format("퀘스트 [{0}] 시작 실패: 조건 미충족(Locked).", quest.title));
            return false;
        }

        if (state == QuestState.Active)
        {
            Debug.Log(
                string.Format("퀘스트 [{0}] 시작 실패: 이미 진행 중.", quest.title));
            return false;
        }

        if (state == QuestState.ReadyToTurnIn)
        {
            Debug.Log(
                string.Format("퀘스트 [{0}] 시작 실패: 이미 목표 달성, 보고 대기 상태.", quest.title));
            return false;
        }

        if (state == QuestState.Completed)
        {
            Debug.Log(
                string.Format("퀘스트 [{0}] 시작 실패: 이미 완료된 퀘스트.", quest.title));
            return false;
        }

        // 여기까지 왔으면 CanAccept일 가능성이 크다.
        QuestInstance instance = new QuestInstance(quest);
        activeQuests.Add(quest.id, instance);

        Debug.Log(
            string.Format("퀘스트 시작: {0} (giver: {1})",
                quest.title,
                giver != null ? giver.name : "null"));

        // 목표가 없는 퀘스트라면 바로 ReadyToTurnIn 상태로 만들 수도 있음
        if (instance.AreAllObjectivesDone())
        {
            OnObjectiveProgressChanged(instance);
        }

        // TODO: UI 갱신, 토스트, 퀘스트 수락 연출 등 필요하면 여기서
        return true;
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
            Debug.Log($"[{quest.data.title}] 목표 전부 완료! 보고하러 가세요.");

            // UI나 알림 시스템으로 이벤트 브로드캐스트할 수도 있음
            // e.g. hub.quest.RaiseQuestReadyToTurnIn(quest.data);
        }

        // 3️⃣ 일부만 완료된 경우엔 그냥 진행도만 업데이트
    }

    bool AreAllObjectivesDone(QuestInstance quest)
    {
        return quest.objectives.All(o => o.isCompleted);
    }

    bool CheckPrerequisites(QuestData quest)
    {
        if (quest == null)
            return false;

        // 1. 레벨 조건 (플레이어 스탯 등)

        // 2. 선행 퀘스트 조건

        // 3. 기타 조건 (ex. 특정 아이템, 스토리 플래그 등) 등등

        return true;
    }
}
