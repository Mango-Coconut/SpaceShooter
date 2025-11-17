using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    [SerializeField] Sprite ladderSprite;
    bool isUsing = false;

    public void Interact(PlayerController player)
    {
        player.UseLadder();
    }

    public bool IsAvailable()
    {
        return !isUsing;
    }

    public void OnFocus()
    {
        
    }

    public void OnUnfocus()
    {
        
    }
        public Sprite GetIcon() => ladderSprite;

    public (string inputKeyText, string behaviorText) GetPrompt() => ("F", "올라타기");

}
