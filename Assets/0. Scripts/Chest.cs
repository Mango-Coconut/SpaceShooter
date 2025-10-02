using System;
using UnityEngine;

public class Chest : Container, IInteractable
{
    //상자 UI Popup 이벤트. PanelManager에서 받음
    public static event Action<Chest> OnChestOpened;
    public event Action OnChestChanged;

    [SerializeField] Sprite chestSprite;

    void OnEnable()  => Changed += Forward;
    void OnDisable() => Changed -= Forward;
    void Forward()   => OnChestChanged?.Invoke();

    public void Interact(PlayerController player)
    {
        OnChestOpened?.Invoke(this);
    }

    public bool IsAvailable() => true;
    public void OnFocus() { }
    public void OnUnfocus() { }

    public Sprite GetIcon() => chestSprite;
    public (string inputKeyText, string behaviorText) GetPrompt() => ("F", "열기");
}