using System;
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
        BuildQuestCacheFromResources();
    }

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        UnSubcribe();
    }

    #region Event Subscribe
    void Subscribe()
    {
        UnSubcribe();
        if (hub != null)
        {
            if (hub.enemy != null)
            {
                hub.enemy.OnEnemyKilled += HandleEnemyKilled;
            }

            if (hub.item != null)
            {
                hub.item.OnItemObtained += HandleItemCollected;
                hub.item.OnItemUsed += HandleItemUsed;
            }
        }
    }

    void UnSubcribe()
    {
        if (hub != null)
        {
            if (hub.enemy != null)
            {
                hub.enemy.OnEnemyKilled -= HandleEnemyKilled;
            }

            if (hub.item != null)
            {
                hub.item.OnItemObtained -= HandleItemCollected;
                hub.item.OnItemUsed -= HandleItemUsed;
            }
        }
    }

    #endregion

    #region 상태 판정
    public QuestState GetQuestState(QuestData quest)
    {
        // 1. 이미 완료했는지
        if (completedQuests.Contains(quest) && !quest.isRepeatable)
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
    #endregion

    #region 퀘스트 시작
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
            Log.Info(string.Format("퀘스트 [{0}] 시작 실패: 조건 미충족(Locked).", quest.title));
            return false;
        }

        if (state == QuestState.Active)
        {
            Log.Info(string.Format("퀘스트 [{0}] 시작 실패: 이미 진행 중.", quest.title));
            return false;
        }

        if (state == QuestState.ReadyToTurnIn)
        {
            Log.Info(string.Format("퀘스트 [{0}] 시작 실패: 이미 목표 달성, 보고 대기 상태.", quest.title));
            return false;
        }

        if (state == QuestState.Completed)
        {
            Log.Info(string.Format("퀘스트 [{0}] 시작 실패: 이미 완료된 퀘스트.", quest.title));
            return false;
        }

        // 수락 하기
        QuestInstance instance = new QuestInstance(quest);

        activeQuests.Add(quest.id, instance); // 자신에게 등록
        giver.EnrollQuest(quest); // 해당 NPC에게 등록
        hub.quest.RaiseQuestStateChanged(instance); // UI 등 기타 요소에게 알려줌

        // 목표가 없는 퀘스트라면 바로 ReadyToTurnIn 상태로 만들 수도 있음
        if (instance.AreAllObjectivesDone())
        {
            OnObjectiveProgressChanged(instance);
        }

        // TODO: UI 갱신, 토스트, 퀘스트 수락 연출 등 필요하면 여기서
        return true;
    }
    #endregion

    #region 퀘스트 진행 중

    // 퀘스트 완료 조건을 충족시키는 전역 허브 이벤트 받기
    void HandleEnemyKilled(string enemyId, int amount)
    {
        Progress(QuestObjectiveType.KillMonster, enemyId, amount);
    }

    void HandleItemCollected(string itemId, int amount)
    {
        //Progress(QuestObjectiveType.CollectItem, itemId, amount);
    }

    void HandleItemUsed(string itemId, int amount)
    {
        Progress(QuestObjectiveType.UseItem, itemId, amount);
    }

    void Progress(QuestObjectiveType type, string targetId, int amount)
    {
        if (activeQuests == null || activeQuests.Count == 0)
            return;

        foreach (KeyValuePair<string, QuestInstance> pair in activeQuests)
        {
            QuestInstance inst = pair.Value;
            if (inst == null)
                continue;

            inst.TryProgress(type, targetId, amount);

            // 목표 달성 여부 체크
            OnObjectiveProgressChanged(inst);
        }
    }

    #endregion

    #region  퀘스트 완료
    public bool TryCompleteQuest(QuestData quest, NpcMono npc, PlayerController player)
    {
        if (quest == null)
        {
            Log.Warn("QuestManager: quest is null in HandleQuestCompleteRequested.");
            return false;
        }

        // 1. 완료 요청 받은 퀘스트의 인스턴스 확인
        QuestInstance instance;
        if (!activeQuests.TryGetValue(quest.id, out instance))
        {
            Log.Warn($"QuestManager: Quest not found in activeQuests ({quest.title})");
            return false;
        }

        // 2. 해당 퀘스트가 완료 가능한 상태인지
        if (instance.state != QuestState.ReadyToTurnIn)
        {
            Log.Warn($"QuestManager: {quest.title} is not ready to turn in. Current state: {instance.state}");
            return false;
        }

        // 3. 보상 지급
        if (quest.reward != null)
        {
            // 골드 지급
            if (quest.reward.coin > 0)
            {
                InventoryManager.Instance.TryAddCoin(player.inventory.Core, quest.reward.coin);
            }

            // 아이템 지급
            if (quest.reward.items != null && quest.reward.items.Count > 0)
            {
                InventoryManager inv = InventoryManager.Instance;
                for (int i = 0; i < quest.reward.items.Count; i++)
                {
                    QuestItemReward itemReward = quest.reward.items[i];
                    bool added = inv.TryAddItem(inv.PlayerInventoryMono.Core, itemReward.itemData, itemReward.amount);
                    // 아이템 추가 실패 시 중단 및 false 반환
                    if (!added)
                    {
                        Log.Warn($"인벤토리 비우고 다시 오렴(혹은 다른 버그)");
                        return false;
                    }
                }
            }
        }

        // 4. 퀘스트 상태를 완료로 전환
        instance.state = QuestState.Completed;
        // 반복 불가 퀘스트만 '완료 목록'에 넣어서 영구 완료 처리
        if (!quest.isRepeatable)
        {
            completedQuests.Add(quest);
        }
        // 진행 중 목록에서는 항상 제거 (반복 가능/불가능 상관 없이)
        activeQuests.Remove(quest.id);


        // 5. 이벤트 알림 (UI / 사운드 등)
        if (hub != null && hub.quest != null)
        {
            hub.quest.RaiseQuestStateChanged(instance);
        }

        Log.Info($"[Quest] Completed: {quest.title} (NPC: {npc?.name ?? "null"})");
        return true;
    }

    #endregion

    void OnObjectiveProgressChanged(QuestInstance instance)
    {
        // 1. 모든 목표가 완료되었는지 검사
        bool allDone = instance.objectives.All(o => o.isCompleted);

        // 2. 아직 진행 중이었다면 상태 전환
        if (allDone && instance.state == QuestState.Active)
        {
            instance.state = QuestState.ReadyToTurnIn;
            hub.quest.RaiseQuestStateChanged(instance);
        }
    }

    bool CheckPrerequisites(QuestData quest)
    {
        if (quest == null)
            return false;
        //추후에 추가

        // 1. 레벨 조건 (플레이어 스탯 등)

        // 2. 선행 퀘스트 조건

        // 3. 기타 조건 (ex. 특정 아이템, 스토리 플래그 등) 등등

        return true;
    }


    #region Save&Load
    public QuestManagerSaveData SaveData()
    {
        QuestManagerSaveData data = new QuestManagerSaveData();
        data.activeQuests = new List<QuestInstanceSaveData>();
        data.completedQuestIds = new List<string>();

        // 1) 진행 중인 퀘스트들
        foreach (KeyValuePair<string, QuestInstance> kv in activeQuests)
        {
            QuestInstance instance = kv.Value;
            if (instance == null || instance.data == null) continue;

            QuestInstanceSaveData s = new QuestInstanceSaveData();
            s.questId = instance.data.id;
            s.state = instance.state;

            // 목표들 진행도 저장 (필요하면)
            if (instance.objectives != null)
            {
                s.objectives = new List<QuestObjectiveSaveData>();
                for (int i = 0; i < instance.objectives.Count; i++)
                {
                    ObjectiveProgress instObj = instance.objectives[i];

                    QuestObjectiveSaveData os = new QuestObjectiveSaveData();
                    os.index = i;
                    os.currentCount = instObj.currentCount;  // 네 구조에 맞게 필드 이름 맞춰줘
                    os.isCompleted = instObj.isCompleted;

                    s.objectives.Add(os);
                }
            }

            data.activeQuests.Add(s);
        }

        // 2) 완료된 퀘스트들: id만 저장
        for (int i = 0; i < completedQuests.Count; i++)
        {
            QuestData q = completedQuests[i];
            if (q == null) continue;
            data.completedQuestIds.Add(q.id);
        }

        return data;
    }


    public void LoadData(QuestManagerSaveData data)
    {
        activeQuests.Clear();
        completedQuests.Clear();

        if (data == null)
        {
            Log.Warn("QuestManager.LoadData: data is null");
            return;
        }

        // -----------------------------
        // 1) 완료된 퀘스트 복원
        // -----------------------------
        if (data.completedQuestIds != null)
        {
            for (int i = 0; i < data.completedQuestIds.Count; i++)
            {
                string id = data.completedQuestIds[i];
                QuestData q = GetQuestById(id);
                if (q == null)
                {
                    Log.Warn($"QuestManager.LoadData: 완료된 퀘스트 {id} 를 찾을 수 없음");
                    continue;
                }

                completedQuests.Add(q);
            }
        }

        // -----------------------------
        // 2) 진행 중 퀘스트 복원
        // -----------------------------
        if (data.activeQuests != null)
        {
            for (int i = 0; i < data.activeQuests.Count; i++)
            {
                QuestInstanceSaveData s = data.activeQuests[i];
                if (string.IsNullOrEmpty(s.questId))
                    continue;

                QuestData qd = GetQuestById(s.questId);
                if (qd == null)
                {
                    Log.Warn($"QuestManager.LoadData: QuestData '{s.questId}' 를 찾을 수 없음");
                    continue;
                }

                // 새로운 인스턴스 생성
                QuestInstance instance = new QuestInstance(qd);
                instance.state = s.state;

                // 목표 진행도 복원
                if (s.objectives != null && instance.objectives != null)
                {
                    int count = Mathf.Min(s.objectives.Count, instance.objectives.Count);
                    for (int j = 0; j < count; j++)
                    {
                        QuestObjectiveSaveData savedObj = s.objectives[j];
                        ObjectiveProgress obj = instance.objectives[j];

                        obj.currentCount = savedObj.currentCount;
                        obj.isCompleted = savedObj.isCompleted;
                    }
                }

                activeQuests[qd.id] = instance;
            }
        }

        // -----------------------------
        // 3) 로드 후 UI 갱신 이벤트 발송
        // -----------------------------
        if (hub != null && hub.quest != null)
        {
            foreach (var kv in activeQuests)
            {
                hub.quest.RaiseQuestStateChanged(kv.Value);
            }
        }

        Log.Info($"QuestManager.LoadData 완료: active={activeQuests.Count}, completed={completedQuests.Count}");
    }

    List<QuestData> questDatabase; // 모든 퀘스트의 종류
    Dictionary<string, QuestData> questById;
    void BuildQuestCacheFromResources()
    {
        questById = new Dictionary<string, QuestData>();

        QuestData[] allQuests = Resources.LoadAll<QuestData>("Quests");
        if (allQuests == null || allQuests.Length == 0)
        {
            Log.Warn("QuestManager: Resources/Quests 에서 QuestData를 찾지 못했습니다.");
            return;
        }

        for (int i = 0; i < allQuests.Length; i++)
        {
            QuestData data = allQuests[i];
            if (data == null || string.IsNullOrEmpty(data.id))
            {
                continue;
            }

            if (!questById.ContainsKey(data.id))
            {
                questById.Add(data.id, data);
            }
            else
            {
                Log.Warn($"QuestManager: 중복된 Quest id 발견 ({data.id})");
            }
        }

        Log.Info($"QuestManager: 퀘스트 캐싱 완료. 개수 = {questById.Count}");
    }

    // -------------------------------
    // 2) id로 QuestData 가져오기
    // -------------------------------
    public QuestData GetQuestById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        if (questById == null || questById.Count == 0)
        {
            // 혹시 Awake 전에 호출되었거나, 에디터에서 리셋된 경우 대비
            BuildQuestCacheFromResources();
        }

        QuestData data;
        if (questById.TryGetValue(id, out data))
        {
            return data;
        }

        Log.Warn($"QuestManager.GetQuestById: 해당 id의 퀘스트를 찾을 수 없습니다. ({id})");
        return null;
    }
    #endregion
}
