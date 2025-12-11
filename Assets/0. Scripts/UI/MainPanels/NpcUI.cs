using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcUI : MonoBehaviour
{
    DialogueCore boundDialogue;
    [SerializeField] ChoicesPanel choicesPanel;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Button nextButton;
    [SerializeField] RewardSlotPanel rewardSlotPanel;
    public RewardSlotPanel RewardSlotPanel => rewardSlotPanel;
    SlotEventBridge slotEventBridge = new SlotEventBridge();
    public SlotEventBridge SlotEventBridge => slotEventBridge;

    void OnEnable()
    {
        slotEventBridge.Subscribe(rewardSlotPanel.Forwarder);
    }

    void OnDisable()
    {
        slotEventBridge.UnSubscribe(rewardSlotPanel.Forwarder);
    }

    public void Bind(DialogueCore dialogue)
    {
        if (dialogue == null) return;
        UnbindDialogue();

        boundDialogue = dialogue;
        boundDialogue.OnNodeChanged += HandleNodeChanged;
    }

    public void UnbindDialogue()
    {
        if (boundDialogue == null) return;

        boundDialogue.OnNodeChanged -= HandleNodeChanged;
        boundDialogue = null;
    }

    void HandleNodeChanged(DialogueNode node)
    {
        if (node == null)
        {
            //ended 에서 처리
            return;
        }

        dialogueText.text = node.text;

        if (node.HasChoices)
        {
            nextButton.gameObject.SetActive(false);
            choicesPanel.gameObject.SetActive(true);
            choicesPanel.Set(node.choices, OnClickChoice);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
            choicesPanel.gameObject.SetActive(false);
        }

        if (node.showQuestRewards)
        {
            rewardSlotPanel.gameObject.SetActive(true);
            rewardSlotPanel.ShowRewards(boundDialogue.asset.questData.reward);
        }
        else
        {
            rewardSlotPanel.gameObject.SetActive(false);
        }
    }

    void OnClickChoice(int index)
    {
        if (boundDialogue != null)
        {
            boundDialogue.SelectChoice(index);
        }
    }

    public void OnClickNext()
    {
        if (boundDialogue != null)
        {
            boundDialogue.Next();
        }
    }

    public void Close()
    {
        choicesPanel.gameObject.SetActive(false);
        UnbindDialogue();
        gameObject.SetActive(false);
    }
}