using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventChannel/PlayerEventChannel")]
public class PlayerEventChannel : ScriptableObject
{
    public event Action<PlayerActionGate> test;
    public event Action<PlayerController> OnExit;
    public void RaiseEnter(NpcMono npc){ if(OnEnter!=null) OnEnter?.Invoke(npc); }
    public void RaiseExit(NpcMono npc){ if(OnExit!=null) OnExit?.Invoke(npc); }
}