using System;
using System.Collections.Generic;

[Serializable]
public class QuestManagerSaveData
{
    public List<QuestInstanceSaveData> activeQuests;
    public List<string> completedQuestIds;
}