using UnityEngine;

[CreateAssetMenu(menuName="Game/GameEventHub")]
public class GameEventHub : ScriptableObject
{
    public NpcInteractionChannel npc;
    public ChestInteractionChannel chest;
    public QuestEventChannel quest;
    // 필요시 더 추가
}