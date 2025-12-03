using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/ItemEventChannel")]
public class ItemEventChannel : ScriptableObject
{
    public event Action<string, int> OnItemObtained;
    public event Action<string, int> OnItemUsed;

    public void RaiseItemObtained(string itemId, int amount = 1)
    {
        OnItemObtained?.Invoke(itemId, amount);
    }

    public void RaiseItemUsed(string itemId, int amount = 1)
    {
        OnItemUsed?.Invoke(itemId, amount);
    }
}
