using System;

[Serializable]
public class DialogueCommand
{
    public DialogueCommandType type;
    public QuestData questData;  // StartQuest 전용
}