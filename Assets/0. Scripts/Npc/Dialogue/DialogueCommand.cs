using System;

[Serializable]
public class DialogueCommand
{
    public DialogueCommandType type;

    // 공용 파라미터 (필요한 것만 세팅)
    public string itemId;
    public int itemCount;

    public QuestData questData;  // ★ StartQuest 전용

    // 나중에 OpenShop 관련해서 shopId 같은 것도 추가 가능
}