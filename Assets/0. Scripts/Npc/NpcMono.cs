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

    public NpcCore npcCore { get; private set; }

    Animator animator;

    PlayerController user;

    void Awake()
    {
        npcCore = new NpcCore(gameObject.name);

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
    
    bool isEnter = false;
    public void Interact(PlayerController pc)
    {
        //추후 네트워크 환경 등에서 널가드 추가
        // if (pc == null) Log.Error("NpcMono : PlayerController is null");
        if (isEnter == false)
        {
            user = pc;
            Enter();
        }
        else
        {
            Exit();
        }
    }
    public void Enter()
    {
        isEnter = true;
        if (user.gate != null) { user.gate.PushUI(); }

        if (hub != null && hub.npc != null)
        {
            hub.npc.RaiseEnter(this);
            npcCore.Initialize(dialogueAsset);
        }

    }

    public void Exit()
    {
        isEnter = false;

        if (hub != null && hub.npc != null)
        {
            hub.npc.RaiseExit(this);
        }

        if (user.gate != null) { user.gate.PopUI(); }
        user = null;
    }

    public bool IsAvailable()
    {
        return npcCore.CanTalk;
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