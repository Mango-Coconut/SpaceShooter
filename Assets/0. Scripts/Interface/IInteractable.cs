using UnityEngine;

public interface IInteractable
{
    bool IsAvailable();
    void OnFocus();
    void OnUnfocus();
    void Interact(PlayerController player);
    public bool CanInteract();

    (string inputKeyText, string behaviorText) GetPrompt();
    Sprite GetIcon();
}