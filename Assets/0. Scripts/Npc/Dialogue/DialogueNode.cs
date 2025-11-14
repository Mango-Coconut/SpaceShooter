using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueNode
{
    public string nodeId;              // 유니크 키 (ex: "start", "ask_job", "end")
    
    [TextArea]
    public string text;                // 대사 본문

    public bool isEnd;                 // true면 여기서 대화 종료

    public string nextNodeId;          // 선택지가 없을 때 다음으로 갈 노드
    public List<DialogueChoice> choices;  // 선택지가 있으면 여기 채움

    public bool HasChoices
    {
        get { return choices != null && choices.Count > 0; }
    }
}