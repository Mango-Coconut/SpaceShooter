using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueCore
{
    DialogueAsset asset;
    Dictionary<string, DialogueNode> nodeMap;
    DialogueNode current;

    public event Action<DialogueNode> OnNodeChanged;
    public event Action<DialogueCommand> OnCommand;
    public event Action OnEnded;

    public void Start(DialogueAsset dialogueAsset)
    {
        asset = dialogueAsset;
        BuildMap();
        Goto(asset.startNodeId);
    }

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

    void Goto(string nodeId)
    {
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

        RaiseNodeChanged();
    }

    void RaiseNodeChanged()
    {
        OnNodeChanged?.Invoke(current);
    }
    void RaiseEnded()
    {
        OnEnded?.Invoke();
    }

    public void Next() // 선택지 없는 노드에서 “다음”
    {
        if (current == null)
        {
            return;
        }
        if (current.HasChoices)
        {
            return; // 선택지는 SelectChoice로만
        }
        if (current.isEnd)
        {
            // 끝
            Goto(null);
            return;
        }

        Goto(current.nextNodeId);
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
        if (choice.command != DialogueCommand.None && OnCommand != null)
        {
            OnCommand.Invoke(choice.command);
        }

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