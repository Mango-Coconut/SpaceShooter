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

        giver.EnrollQuest(quest);
        hub.quest.RaiseQuestStateChanged(instance);

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

    // 전역 허브 이벤트 받기
    void HandleEnemyKilled(string enemyId, int amount)
    {
        Progress(QuestObjectiveType.KillMonster, enemyId, amount);
    }

    void HandleItemCollected(string itemId, int amount)
    {
        Progress(QuestObjectiveType.CollectItem, itemId, amount);
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
                Log.Info($"[Quest] Gained Gold: {quest.reward.coin}");
            }

            // 아이템 지급
            if (quest.reward.items != null && quest.reward.items.Count > 0)
            {
                InventoryManager inv = InventoryManager.Instance;
                if (inv != null && inv.PlayerInventoryMono != null)
                {
                    for (int i = 0; i < quest.reward.items.Count; i++)
                    {
                        QuestItemReward itemReward = quest.reward.items[i];
                        bool added = inv.TryAddItem(inv.PlayerInventoryMono.Core, itemReward.itemData, itemReward.amount);
                        Log.Info($"[Quest] Item Reward: {itemReward.itemData.name} x{itemReward.amount} (Result: {added})");

                        // 아이템 추가 실패 시 중단 및 false 반환
                        if (!added)
                        {
                            Log.Warn($"QuestManager: Failed to grant item reward {itemReward.itemData.name}");
                            return false;
                        }
                    }
                }
                else
                {
                    Log.Warn("InventoryManager not ready while granting item rewards.");
                    return false;
                }
            }
        }

        // 4. 퀘스트 상태를 완료로 전환
        instance.state = QuestState.Completed;
        completedQuests.Add(quest);
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
        // 1️⃣ 모든 목표가 완료되었는지 검사
        bool allDone = AreAllObjectivesDone(instance);

        // 2️⃣ 아직 진행 중이었다면 상태 전환
        if (allDone && instance.state == QuestState.Active)
        {
            instance.state = QuestState.ReadyToTurnIn;

            // 플레이어에게 알림 띄우기 등
            hub.quest.RaiseQuestStateChanged(instance);

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
