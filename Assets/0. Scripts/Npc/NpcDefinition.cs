using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/NPC/NpcDefinition (Minimal)")]
public class NpcDefinition : ScriptableObject
{
    public string npcName;
    [TextArea] public List<string> dialogueLines = new List<string>(); // 선형 대사
    public bool hasShop = false;                                        // 상점 메뉴 노출 여부
}

public enum NpcMenuOption
{
    Talk,
    Shop,
    Leave
}