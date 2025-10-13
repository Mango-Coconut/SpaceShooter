using System;
using UnityEngine;

public class Chest : Container, IInteractable
{
    //미리 들어있는 상자 내용물
    [SerializeField] ItemData[] chestitems;
    public static event Action<Chest> OnChestOpened;
    public event Action OnChestChanged;

    [SerializeField] Sprite chestSprite;

    void OnEnable() => Changed += Forward;
    void OnDisable() => Changed -= Forward;
    void Forward() => OnChestChanged?.Invoke();

    void Start()
    {
        foreach (ItemData item in chestitems)
        {
            TryAddItem(item);
        }
    }
    public void Interact(PlayerController player)
    {
        //PanelManager가 받음
        OnChestOpened?.Invoke(this);
    }

    public bool IsAvailable() => true;
    public void OnFocus() { }
    public void OnUnfocus() { }

    public Sprite GetIcon() => chestSprite;
    public (string inputKeyText, string behaviorText) GetPrompt() => ("F", "열기");
}