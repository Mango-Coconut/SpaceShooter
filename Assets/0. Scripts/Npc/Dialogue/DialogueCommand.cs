using System;

[Serializable]
public class DialogueCommand
{
    public DialogueCommandType type;
    public QuestData questData;  // StartQuest, VisibleReward 전용
}