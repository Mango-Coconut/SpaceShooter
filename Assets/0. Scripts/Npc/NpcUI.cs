using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcUI : MonoBehaviour
{
    [SerializeField] ChoicesPanel choicesPanel;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] Button nextButton;

    DialogueCore boundDialogue;

    void Awake()
    {
        //DialogueCore의 
    }

    public void Bind(DialogueCore dialogue)
    {
        if (dialogue == null) return;
        Unbind();

        boundDialogue = dialogue;
        boundDialogue.OnNodeChanged += HandleNodeChanged;
    }

    public void Unbind()
    {
        if (boundDialogue == null) return;

        boundDialogue.OnNodeChanged -= HandleNodeChanged;
        boundDialogue = null;
    }

    void HandleNodeChanged(DialogueNode node)
    {
        if (node == null)
        {
            Close();
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

    void Close()
    {
        choicesPanel.gameObject.SetActive(false);
        Unbind();
        gameObject.SetActive(false);
    }
}