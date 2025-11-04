using System;
using System.Collections.Generic;

public class NpcCore
{
    public enum Phase { Idle, Menu, Talk }

    public NpcDefinition Def { get; }
    public Phase CurrentPhase { get; private set; } = Phase.Idle;

    int lineIndex = -1;

    // UI/외부로 내보내는 이벤트
    public event Action<List<string>> OnShowMenu;                 // 메뉴 항목 라벨
    public event Action<string, bool> OnShowLine;                 // (텍스트, 다음줄존재여부)
    public event Action OnRequestOpenShop;                        // 상점 열어달라
    public event Action OnEnd;                                    // 세션 종료(패널 정리/게이트 Pop 트리거)

    public NpcCore(NpcDefinition def)
    {
        Def = def;
    }

    public void StartSession()
    {
        CurrentPhase = Phase.Menu;
        EmitMenu();
    }

    public void ChooseMenu(NpcMenuOption option)
    {
        if (option == NpcMenuOption.Talk)
        {
            EnterTalk();
            return;
        }
        if (option == NpcMenuOption.Shop)
        {
            if (Def != null && Def.hasShop)
            {
                if (OnRequestOpenShop != null) OnRequestOpenShop();
            }
            else
            {
                // 상점이 없으면 메뉴로 유지
                EmitMenu();
            }
            return;
        }
        // Leave
        EndSession();
    }

    public void NextLine()
    {
        if (CurrentPhase != Phase.Talk) return;

        lineIndex++;
        if (Def == null || Def.dialogueLines == null || lineIndex >= Def.dialogueLines.Count)
        {
            // 대화 끝 → 메뉴로 복귀
            CurrentPhase = Phase.Menu;
            EmitMenu();
            return;
        }

        string text = Def.dialogueLines[lineIndex];
        bool hasNext = (lineIndex < Def.dialogueLines.Count - 1);
        if (OnShowLine != null) OnShowLine(text, hasNext);
    }

    void EnterTalk()
    {
        CurrentPhase = Phase.Talk;
        lineIndex = -1;
        NextLine();
    }

    void EmitMenu()
    {
        CurrentPhase = Phase.Menu;

        List<string> options = new List<string>();
        options.Add("대화하기");
        if (Def != null && Def.hasShop) { options.Add("상점"); }
        options.Add("떠나기");

        if (OnShowMenu != null) OnShowMenu(options);
    }

    void EndSession()
    {
        CurrentPhase = Phase.Idle;
        if (OnEnd != null) OnEnd();
    }
}