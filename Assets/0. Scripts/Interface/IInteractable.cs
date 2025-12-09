using UnityEngine;

public interface IInteractable
{
    bool IsAvailable();
    void OnFocus();
    void OnUnfocus();
    void Interact(PlayerController player);
    void Exit();
    public bool CanInteract();

    (string inputKeyText, string behaviorText) GetPrompt();
    Sprite GetIcon();
}