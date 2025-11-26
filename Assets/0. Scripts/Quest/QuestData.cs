using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string questId;            // 저장/로딩용 고유 ID
    public string questName;          // UI 제목
    [TextArea]
    public string summary;            // 짧은 설명 (목록용)
    [TextArea(3, 6)]
    public string description;        // 상세 설명 (상세창용)

    public Sprite icon;
    public QuestCategory category;

    [Header("추천 정보 / 플래그")]
    public int recommendedLevel;
    public bool isRepeatable;

    [Header("선행 조건")]
    public List<string> requiredQuestIds;   // 이 퀘스트들 완료해야 시작 가능
    public int requiredPlayerLevel;

    [Header("목표 목록")]
    public List<QuestObjective> objectives;

    [Header("보상")]
    public QuestReward reward;
}
