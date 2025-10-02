using System;
using UnityEngine;

public class Chest : Container, IInteractable
{
    public event Action OnChestChanged;

    [SerializeField] Sprite chestSprite;

    void OnEnable()  => Changed += Forward;
    void OnDisable() => Changed -= Forward;
    void Forward()   => OnChestChanged?.Invoke();

    public void Interact(PlayerController player)
    {
        Debug.Log("상자 열기");
        // 상자 UI 열고/닫는 건 별도 PanelManager에서 처리
    }

    public bool IsAvailable() => true;
    public void OnFocus() { }
    public void OnUnfocus() { }

    public Sprite GetIcon() => chestSprite;
    public (string inputKeyText, string behaviorText) GetPrompt() => ("F", "열기");
}