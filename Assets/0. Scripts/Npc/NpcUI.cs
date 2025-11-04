using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcUI : MonoBehaviour
{
    [Header("Menu")]
    [SerializeField] GameObject menuRoot;
    [SerializeField] Button talkButton;
    [SerializeField] Button shopButton;   // hasShop=false면 숨김
    [SerializeField] Button leaveButton;

    [Header("Dialogue")]
    [SerializeField] GameObject dialogueRoot;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Button nextButton;
    [SerializeField] Button closeButton;

    NpcMono owner;

    public void Bind(NpcMono mono)
    {
        owner = mono;

        // 메뉴 버튼
        if (talkButton != null)
        {
            talkButton.onClick.RemoveAllListeners();
            talkButton.onClick.AddListener(OnClickTalk);
        }
        if (shopButton != null)
        {
            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(OnClickShop);
        }
        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveAllListeners();
            leaveButton.onClick.AddListener(OnClickLeave);
        }

        // 대화 버튼
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnClickNext);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnClickClose);
        }
    }

    public void Unbind()
    {
        owner = null;
    }

    public void OpenMenu()
    {
        if (menuRoot != null) { menuRoot.setActiveTrue(); }
        if (dialogueRoot != null) { dialogueRoot.setActiveFalse(); }
    }

    public void ShowMenu(List<string> options)
    {
        // 옵션은 "대화하기", ["상점"], "떠나기" 순서로 온다.
        if (menuRoot != null) { menuRoot.setActiveTrue(); }
        if (dialogueRoot != null) { dialogueRoot.setActiveFalse(); }

        if (talkButton != null)
        {
            TextMeshProUGUI label = talkButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) { label.text = options.Count > 0 ? options[0] : "대화하기"; }
            talkButton.gameObject.setActiveTrue();
        }

        if (shopButton != null)
        {
            bool hasShop = options.Count == 3; // 가운데가 상점
            shopButton.gameObject.SetActive(hasShop);
            if (hasShop)
            {
                TextMeshProUGUI label = shopButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) { label.text = options[1]; }
            }
        }

        if (leaveButton != null)
        {
            TextMeshProUGUI label = leaveButton.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                string text = options.Count == 3 ? options[2] : options[1];
                label.text = text;
            }
            leaveButton.gameObject.setActiveTrue();
        }
    }

    public void ShowLine(string text, bool hasNext)
    {
        if (menuRoot != null) { menuRoot.setActiveFalse(); }
        if (dialogueRoot != null) { dialogueRoot.setActiveTrue(); }

        if (dialogueText != null) { dialogueText.text = text != null ? text : string.Empty; }

        if (nextButton != null) { nextButton.gameObject.SetActive(hasNext); }
        if (closeButton != null) { closeButton.gameObject.SetActive(!hasNext); }
    }

    public void CloseAll()
    {
        if (menuRoot != null) { menuRoot.setActiveFalse(); }
        if (dialogueRoot != null) { dialogueRoot.setActiveFalse(); }
    }

    // ===== Button Handlers =====
    void OnClickTalk()
    {
        if (owner != null) { owner.Core.ChooseMenu(NpcMenuOption.Talk); }
    }

    void OnClickShop()
    {
        if (owner != null) { owner.Core.ChooseMenu(NpcMenuOption.Shop); }
    }

    void OnClickLeave()
    {
        if (owner != null) { owner.Core.ChooseMenu(NpcMenuOption.Leave); }
    }

    void OnClickNext()
    {
        if (owner != null) { owner.Core.NextLine(); }
    }

    void OnClickClose()
    {
        // 마지막 줄에서 닫기 → 메뉴로 복귀(코어가 알아서 처리함)
        if (owner != null) { owner.Core.NextLine(); }
    }
}

// 편의 확장 (네임 충돌 없게 internal static)
static class GameObjectExtensions_Min
{
    public static void setActiveTrue(this GameObject go)
    {
        if (go != null) { go.SetActive(true); }
    }
    public static void setActiveFalse(this GameObject go)
    {
        if (go != null) { go.SetActive(false); }
    }
}