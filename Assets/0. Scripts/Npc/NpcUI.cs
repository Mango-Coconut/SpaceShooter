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

    NpcCore core;

    public void Bind(NpcCore npc)
    {
        core = npc;

        talkButton.onClick.RemoveAllListeners();
        talkButton.onClick.AddListener(OnClickTalk);
        shopButton.onClick.RemoveAllListeners();
        shopButton.onClick.AddListener(OnClickShop);
        leaveButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnClickLeave);

        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnClickNext);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(OnClickClose);

        OpenMenu();
    }

    public void Unbind()
    {
        core = null;
        menuRoot.SetActive(false);
        dialogueRoot.SetActive(false);
    }

    public void ShowMenu(List<string> options)
    {
        menuRoot.SetActive(true);
        dialogueRoot.SetActive(false);

        SetLabel(talkButton, options[0]);

        bool hasShop = options.Count == 3;
        shopButton.gameObject.SetActive(hasShop);
        if (hasShop) SetLabel(shopButton, options[1]);

        string leaveText = hasShop ? options[2] : options[1];
        SetLabel(leaveButton, leaveText);
    }

    public void ShowLine(string text, bool hasNext)
    {
        menuRoot.SetActive(false);
        dialogueRoot.SetActive(true);

        dialogueText.text = text != null ? text : string.Empty;
        nextButton.gameObject.SetActive(hasNext);
        closeButton.gameObject.SetActive(!hasNext);
    }

    public void OpenMenu()
    {
        menuRoot.SetActive(true);
        dialogueRoot.SetActive(false);
    }

    void OnClickTalk() { if (core != null) core.ChooseMenu(NpcMenuOption.Talk); }
    void OnClickShop() { if (core != null) core.ChooseMenu(NpcMenuOption.Shop); }
    void OnClickLeave() { if (core != null) core.ChooseMenu(NpcMenuOption.Leave); }
    void OnClickNext() { if (core != null) core.NextLine(); }
    void OnClickClose() { if (core != null) core.NextLine(); }

    void SetLabel(Button btn, string text)
    {
        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = text;
    }
}