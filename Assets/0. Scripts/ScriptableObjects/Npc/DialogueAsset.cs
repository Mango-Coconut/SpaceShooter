using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Dialogue/DialogueAsset")]
public class DialogueAsset : ScriptableObject
{
    public string startNodeId;
    public List<DialogueNode> nodes;
}