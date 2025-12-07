using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventChannel/NpcEventChannel")]
public class NpcEventChannel : ScriptableObject
{
    public event Action<NpcMono> OnEnter;
    public event Action<NpcMono> OnExit;
    public void RaiseEnter(NpcMono npc){ if(OnEnter!=null) OnEnter?.Invoke(npc); }
    public void RaiseExit(NpcMono npc){ if(OnExit!=null) OnExit?.Invoke(npc); }
}