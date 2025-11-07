using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueAsset")]
public class DialogueAsset : ScriptableObject
{
    public string startNodeId;
    public List<DialogueNode> nodes;
}