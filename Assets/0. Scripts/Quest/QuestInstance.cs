using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class QuestInstance
{
    public QuestData data;                 // 어떤 퀘스트인지 (정적 정보)
    public QuestState state = QuestState.Active;

    [SerializeField]
    public List<ObjectiveProgress> objectives;  // 각 목표 진행상태

    public QuestInstance(QuestData questData)
    {
        data = questData;
        state = QuestState.Active;

        // QuestData.objectives를 기반으로 초기화
        objectives = new List<ObjectiveProgress>();
        for (int i = 0; i < data.objectives.Count; i++)
        {
            QuestObjective obj = data.objectives[i];
            objectives.Add(new ObjectiveProgress(obj));
        }
    }

    /// <summary>
    /// 모든 목표가 완료되었는지 검사
    /// </summary>
    public bool AreAllObjectivesDone()
    {
        return objectives.All(o => o.isCompleted);
    }

    /// <summary>
    /// 특정 타입의 목표를 업데이트 (몬스터 처치 등)
    /// </summary>
    public void TryProgress(QuestObjectiveType type, string targetId, int amount = 1)
    {
        for (int i = 0; i < objectives.Count; i++)
        {
            ObjectiveProgress progress = objectives[i];
            if (progress.Matches(type, targetId) && !progress.isCompleted)
            {
                progress.AddProgress(amount);
            }
        }
    }
}
