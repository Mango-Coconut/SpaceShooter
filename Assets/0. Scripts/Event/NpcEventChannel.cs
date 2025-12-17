using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventChannel/NpcEventChannel")]
public class NpcEventChannel : ScriptableObject
{
    public event Action<NpcMono> OnEnter;
    public event Action<NpcMono> OnExit;
    public event Action<ShopInventory> OpenShop;
    public void RaiseEnter(NpcMono npc) => OnEnter?.Invoke(npc);
    public void RaiseExit(NpcMono npc) => OnExit?.Invoke(npc);
    public void RaiseOpenShop(ShopInventory shopInventory) => OpenShop?.Invoke(shopInventory);
}
