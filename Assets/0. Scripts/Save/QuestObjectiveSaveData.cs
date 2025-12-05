using System;

[Serializable]
public class QuestObjectiveSaveData
{
    public int index;          // 몇 번째 목표인지
    public int currentCount;   // 현재 달성 수
    public bool isCompleted;   // 완료 여부
}