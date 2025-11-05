using System;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;

public class NpcMono : MonoBehaviour, IInteractable
{
    [SerializeField] InteractionHub hub;

    [SerializeField] NpcDefinition definition;
    [SerializeField] Sprite icon;

    public NpcCore Core { get; private set; }

    Animator animator;

    PlayerController user;

    // PanelManager가 받아서 NpcUI 띄우기
    public event Action<NpcMono> OnNpcEnter;
    public event Action<NpcMono> OnNpcExit;



    void Awake()
    {

        Core = new NpcCore(definition);
        Core.OnShowMenu += HandleShowMenu;
        Core.OnShowLine += HandleShowLine;
        Core.OnRequestOpenShop += HandleRequestOpenShop;
        Core.OnEnd += HandleEnd;

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
    void OnDestroy()
    {
        if (Core != null)
        {
            Core.OnShowMenu -= HandleShowMenu;
            Core.OnShowLine -= HandleShowLine;
            Core.OnRequestOpenShop -= HandleRequestOpenShop;
            Core.OnEnd -= HandleEnd;
        }
    }
    bool isEnter = false;
    public void Interact(PlayerController pc)
    {
        //추후 네트워크 환경 등에서 추가
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
        if (gate != null) { gate.PushInteract(); }

        if (hub != null && hub.npc != null)
        {
            hub.npc.RaiseEnter(this);
        }

        Core.StartSession();
    }

    public void Exit()
    {
        isEnter = false;

        if (hub != null && hub.npc != null)
        {
            hub.npc.RaiseExit(this);
        }

        if (gate != null) { gate.PopInteract(); }
        user = null;
    }

    // ===== Core → UI 연결 =====
    void HandleShowMenu(System.Collections.Generic.List<string> options)
    {
        if (npcUI != null) { npcUI.ShowMenu(options); }
    }

    void HandleShowLine(string text, bool hasNext)
    {
        if (npcUI != null) { npcUI.ShowLine(text, hasNext); }
    }

    void HandleRequestOpenShop()
    {
        if (panelManager != null)
        {
            //panelManager.OpenShop();     // 네 PanelManager에 있는 간단 OpenShop() 사용
        }
    }

    void HandleEnd()
    {
        Exit();
    }

    public bool IsAvailable()
    {
        return true;
    }

    public void OnFocus()
    {
        animator.SetTrigger("Scanned");
    }

    public void OnUnfocus()
    {

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