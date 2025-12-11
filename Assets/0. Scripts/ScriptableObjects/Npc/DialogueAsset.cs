using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Dialogue/DialogueAsset")]
public class DialogueAsset : ScriptableObject
{
    public string startNodeId;
    [Header("퀘스트 전용 대화일 경우 설정")]
    public QuestData questData;
    public List<DialogueNode> nodes;
}