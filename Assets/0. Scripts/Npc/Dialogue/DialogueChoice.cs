using System;

[Serializable]
public class DialogueChoice
{
    public string text;
    public string nextNodeId;          // 이 선택 후 이동할 노드
    public DialogueCommand command;    // OpenShop 같은 액션
}