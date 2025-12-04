using System;

[Serializable]
public class DialogueCommand
{
    public DialogueCommandType type;

    public QuestData questData;  // ★ StartQuest 전용

    // 나중에 OpenShop 관련해서 shopId 같은 것도 추가 가능
}