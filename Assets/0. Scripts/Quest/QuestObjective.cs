using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    [Header("기본 정보")]
    public string objectiveId;          // 저장 시 식별용 (옵션)
    public string description;         // UI에 보여줄 문장

    [Header("타입 / 대상")]
    public QuestObjectiveType type;

    // 타겟 식별용 (몬스터/아이템/NPC/트리거 등)
    // - 몬스터: monsterId
    // - 아이템: itemDataId or ItemData
    // - NPC   : npcId
    // public MonsterID monsterID;
    ItemData itemdata;
    string npcID;

    [Header("수량 / 옵션")]
    public int requiredCount = 1;
    public bool isOptional;            // 선택 목표인지 여부

    // 나중에 '동시에' 달성해야 하는 그룹 같은 거 필요하면 groupId 같은 것도 추가 가능
}