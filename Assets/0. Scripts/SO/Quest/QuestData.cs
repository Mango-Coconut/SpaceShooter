using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    public string id;
    public string title;
    [TextArea] public string summary;
    [TextArea(3, 6)] public string description;
    public Sprite icon;

    public bool isRepeatable;

    [Header("목표 목록")]
    public List<QuestObjective> objectives;

    [Header("보상")]
    public QuestReward reward;

    [Header("이 퀘스트 전용 대화 에셋")]
    public DialogueAsset questDialogue;

    [Header("퀘스트 상태별 시작 노드 ID")]
    public string nodeLocked;
    public string nodeCanAccept;
    public string nodeInProgress;
    public string nodeReadyToTurnIn;
    public string nodeCompleted;

    public string GetNodeIdByState(QuestState state)
    {
        switch (state)
        {
            case QuestState.Locked:         return nodeLocked;
            case QuestState.CanAccept:      return nodeCanAccept;
            case QuestState.Active:         return nodeInProgress;
            case QuestState.ReadyToTurnIn:  return nodeReadyToTurnIn;
            case QuestState.Completed:      return nodeCompleted;
            default:                        return nodeCanAccept;
        }
    }
}
