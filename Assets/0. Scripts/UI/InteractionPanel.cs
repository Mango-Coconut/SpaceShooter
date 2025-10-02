using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPanel : MonoBehaviour
{

    [SerializeField] TMP_Text inputKeyText;
    [SerializeField] Image itemIcon;
    [SerializeField] TMP_Text behaviorText;

    public void OnTargetChange(IInteractable target)
    {
        if (target == null) return; // 안전망

        var (key, text) = target.GetPrompt();   // 지금은 튜플 유지
        var spr = target.GetIcon();

        inputKeyText.text = key ?? "";
        behaviorText.text = text ?? "";

        if (spr != null)
        {
            itemIcon.enabled = true;
            itemIcon.sprite = spr;
        }
        else
        {
            itemIcon.enabled = false;
            itemIcon.sprite = null;
        }
    }
}
