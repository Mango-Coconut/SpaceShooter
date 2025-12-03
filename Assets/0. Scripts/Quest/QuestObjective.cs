using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    [Header("기본 정보")]
    public string objectiveId;          // 저장 시 식별용 (옵션)
    public string description;         // UI에 보여줄 문장

    [Header("타입 / 대상")]
    public QuestObjectiveType type;
    public string targetId;

    [Header("수량 / 옵션")]
    public int requiredCount = 1;

    // 나중에 '동시에' 달성해야 하는 그룹 같은 거 필요하면 groupId 같은 것도 추가 가능
}