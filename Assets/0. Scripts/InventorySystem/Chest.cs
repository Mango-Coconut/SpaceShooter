using System;
using UnityEngine;

public class Chest : InventoryMono, IInteractable
{
    //미리 들어있는 상자 내용물
    [SerializeField] ItemData[] chestitems;
    public static event Action<Chest> OnChestOpened;

    [SerializeField] Sprite chestSprite;


    void Start()
    {
        foreach (ItemData item in chestitems)
        {
            TryAddItem(item);
        }
    }
    public void Interact(PlayerController pc)
    {
        //PanelManager가 받음
        //InventoryManager도 받음
        OnChestOpened?.Invoke(this);
    }

    public bool IsAvailable() => true;
    public void OnFocus() { }
    public void OnUnfocus() { }

    public Sprite GetIcon() => chestSprite;
    public (string inputKeyText, string behaviorText) GetPrompt() => ("F", "열기");
}