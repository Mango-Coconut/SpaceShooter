using System;
using UnityEngine;

[CreateAssetMenu(menuName="Game/Chest/ChestInteractionChannel")]
public class ChestInteractionChannel : ScriptableObject
{
    public event Action<Chest> OnOpen;
    public event Action<Chest> OnClose;
    public void RaiseOpen(Chest c){ if(OnOpen!=null) OnOpen?.Invoke(c); }
    public void RaiseClose(Chest c){ if(OnClose!=null) OnClose.Invoke(c); }
}