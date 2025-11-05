using UnityEngine;

[CreateAssetMenu(menuName="Game/InteractionHub")]
public class InteractionHub : ScriptableObject
{
    public NpcInteractionChannel npc;
    public ChestInteractionChannel chest;
    // 필요시 더 추가
}