using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string id;            // 저장/로딩용 고유 ID
    public string title;          // UI 제목
    [TextArea]
    public string summary;            // 짧은 설명 (목록용)
    [TextArea(3, 6)]
    public string description;        // 상세 설명 (상세창용)

    public Sprite icon;

    public bool isRepeatable;

    [Header("목표 목록")]
    public List<QuestObjective> objectives;

    [Header("보상")]
    public QuestReward reward;
}
