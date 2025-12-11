using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventHub/GameEventHub")]
public class GameEventHub : ScriptableObject
{
    public PlayerEventChannel player;
    public NpcEventChannel npc;
    public ChestEventChannel chest;
    public QuestEventChannel quest;
    public EnemyEventChannel enemy;
    public ItemEventChannel item;
}   