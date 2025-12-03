using System;

[Serializable]
public class ObjectiveProgress
{
    public QuestObjective data;
    public int currentCount;
    public bool isCompleted;

    public ObjectiveProgress(QuestObjective data)
    {
        this.data = data;
        this.currentCount = 0;
        this.isCompleted = false;
    }

    public bool Matches(QuestObjectiveType type, string targetId)
    {
        if (data == null) return false;
        if (data.type != type) return false;

        // 필요시 대소문자 비교 조정
        if (!string.IsNullOrEmpty(data.targetId) && data.targetId != targetId)
            return false;

        return true;
    }

    public void AddProgress(int amount)
    {
        currentCount += amount;
        if (currentCount >= data.requiredCount)
        {
            currentCount = data.requiredCount;
            isCompleted = true;
        }
    }
}
