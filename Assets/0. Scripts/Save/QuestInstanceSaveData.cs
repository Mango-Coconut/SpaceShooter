
using System;
using System.Collections.Generic;

[Serializable]
public class QuestInstanceSaveData
{
    public string questId;
    public QuestState state;

    public List<QuestObjectiveSaveData> objectives;
}