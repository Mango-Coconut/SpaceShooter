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
        Unbind();

        boundDialogue = dialogue;
        if (boundDialogue == null)
        {
            return;
        }

        boundDialogue.OnNodeChanged += HandleNodeChanged;

        // 현재 노드 바로 반영하고 싶으면:
        // HandleNodeChanged(boundDialogue.CurrentNode); 이런 식으로 프로퍼티 하나 두면 됨.
    }

    public void Unbind()
    {
        if (boundDialogue != null)
        {
            boundDialogue.OnNodeChanged -= HandleNodeChanged;
            boundDialogue = null;
        }
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
        gameObject.SetActive(false);
    }
}