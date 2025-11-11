using System;
using System.Runtime.InteropServices;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;

public class NpcMono : MonoBehaviour, IInteractable
{
    // Interact시 발송할 이벤트
    [SerializeField] InteractionHub hub;

    [SerializeField] Sprite icon;
    [SerializeField] DialogueAsset dialogueAsset;

    public NpcCore Core { get; private set; }

    Animator animator;

    PlayerController user;

    void Awake()
    {
        Core = new NpcCore(gameObject.name);

        // 자식의 Animator 찾아오기
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            Animator found;
            if (child.TryGetComponent<Animator>(out found))
            {
                animator = found;
                break;
            }
        }

        if (animator == null)
        {
            Log.Warn($"{name}: Animator not found among direct children.");
        }
    }
    
    #region 이벤트
    void OnEnable()
    {
        Core.dialogueCore.OnCommand -= HandleCommand;
        Core.dialogueCore.OnEnded -= HandleDialogueEnded;
        Core.dialogueCore.OnCommand += HandleCommand;
        Core.dialogueCore.OnEnded += HandleDialogueEnded;
    }

    void OnDisable()
    {
        Core.dialogueCore.OnCommand -= HandleCommand;
        Core.dialogueCore.OnEnded -= HandleDialogueEnded;
    }



    void HandleDialogueEnded()
    {
        Exit();
    }
    void HandleCommand(DialogueCommand command)
    {
        switch (command)
        {
            case DialogueCommand.None:
                break;
                
            case DialogueCommand.OpenShop:
                break;
        }
    }
    
    #endregion
    bool isEnter = false;
    public void Interact(PlayerController pc)
    {
        //추후 네트워크 환경 등에서 널가드 추가
        // if (pc == null) Log.Error("NpcMono : PlayerController is null");
        if (isEnter == false)
        {
            Enter(pc);
        }
        else
        {
            Exit();
        }
    }
    public void Enter(PlayerController pc)
    {
        if (isEnter == true) return;
        if (pc == null || pc.gate == null) return;
        if (hub == null || hub.npc == null) return;

        isEnter = true;

        user = pc;
        user.gate.PushUI();

        hub.npc.RaiseEnter(this);
        Core.Initialize(dialogueAsset);

        Log.Info($"enter");
    }
    public void Exit()
    {
        if (isEnter == false) return;
        if (user == null || user.gate == null) return;
        if (hub == null || hub.npc == null) return;

        isEnter = false;

        user.gate.PopUI();
        user = null;

        hub.npc.RaiseExit(this);
        
        Log.Info($"exit");
    }

    public bool IsAvailable()
    {
        return Core.CanTalk;
    }

    public void OnFocus()
    {
        //animator.SetTrigger("Scanned");
    }

    public void OnUnfocus()
    {
        Exit();
    }

    public (string inputKeyText, string behaviorText) GetPrompt()
    {
        return ("F", "대화하기");
    }

    public Sprite GetIcon()
    {
        return icon;
    }
}