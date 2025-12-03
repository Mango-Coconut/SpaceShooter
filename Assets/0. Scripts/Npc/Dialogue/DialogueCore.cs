using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueCore
{
    DialogueAsset asset;
    Dictionary<string, DialogueNode> nodeMap;
    DialogueNode current;

    #region 이벤트 정의
    public event Action<DialogueNode> OnNodeChanged;
    public event Action<DialogueCommand> OnCommand;
    public event Action OnEnded;

    void RaiseNodeChanged() => OnNodeChanged?.Invoke(current);
    void RaiseEnded() => OnEnded?.Invoke();
    void ExecuteCommand(DialogueCommand cmd)
    {
        if (cmd == null) return;
        if (cmd.type == DialogueCommandType.None) return;

        OnCommand?.Invoke(cmd);
    }
    #endregion

    void BuildMap()
    {
        nodeMap = new Dictionary<string, DialogueNode>();

        for (int i = 0; i < asset.nodes.Count; i++)
        {
            DialogueNode node = asset.nodes[i];
            if (!string.IsNullOrEmpty(node.nodeId) && !nodeMap.ContainsKey(node.nodeId))
            {
                nodeMap.Add(node.nodeId, node);
            }
        }
    }

    // 일반 시작
    public void Start(DialogueAsset dialogueAsset)
    {
        asset = dialogueAsset;
        BuildMap();
        Goto(asset.startNodeId);
    }

    // 특정 노드로 대화 시작
    public void Start(DialogueAsset dialogueAsset, string startNodeId = null)
    {
        asset = dialogueAsset;
        BuildMap();

        if (string.IsNullOrEmpty(startNodeId))
        {
            startNodeId = asset.startNodeId;
        }

        Goto(startNodeId);
    }

    public void Next() // 선택지 없는 노드에서 “다음”
    {
        if (current == null) return;
        if (current.HasChoices)  return; // 선택지는 SelectChoice로만
        if (current.isEnd)
        {
            Goto(null);
            return;
        }
        Goto(current.nextNodeId);
    }

    void Goto(string nodeId)
    {   
        // 읽을 노드가 없으면 끝내기
        if (string.IsNullOrEmpty(nodeId))
        {
            current = null;
            RaiseNodeChanged();
            RaiseEnded();
            return;
        }

        DialogueNode next;
        if (!nodeMap.TryGetValue(nodeId, out next))
        {
            Debug.LogWarning("DialogueCore: Node not found : " + nodeId);
            current = null;
        }
        else
        {
            current = next;
        }
        if (current != null)
        {
            ExecuteCommand(current.command);
        }
        RaiseNodeChanged();
    }


    public void SelectChoice(int index)
    {
        if (current == null || !current.HasChoices)
        {
            return;
        }
        if (index < 0 || index >= current.choices.Count)
        {
            return;
        }

        DialogueChoice choice = current.choices[index];

        // 커맨드 먼저 쏘고
        ExecuteCommand(choice.command);

        // 다음 노드로 이동
        if (string.IsNullOrEmpty(choice.nextNodeId))
        {
            Goto(null);
        }
        else
        {
            Goto(choice.nextNodeId);
        }
    }


}