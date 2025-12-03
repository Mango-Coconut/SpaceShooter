using UnityEngine;

[CreateAssetMenu(menuName="Game/GameEventHub")]
public class GameEventHub : ScriptableObject
{
    public NpcInteractionChannel npc;
    public ChestInteractionChannel chest;
    public QuestEventChannel quest;
    public EnemyEventChannel enemy;
    public ItemEventChannel item;

}