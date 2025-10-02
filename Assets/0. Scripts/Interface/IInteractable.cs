using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    bool IsAvailable();
    void OnFocus();
    void OnUnfocus();
    void Interact(PlayerController player);

    (string inputKeyText, string behaviorText) GetPrompt();
    Sprite GetIcon();
}
