using System;

public class NpcCore
{
    public string NpcName { get; }
    public bool CanTalk { get; private set; } = true;

    public DialogueCore dialogueCore { get; private set; }

    // 커맨드 "해석 결과"를 외부에 요청으로 알림
    public event Action OnOpenShopRequested;
    public event Action<QuestData> OnStartQuestRequested;
    public event Action<QuestData> OnCompleteQuestRequested;
    public event Action OnDialogueEnded;



    public NpcCore(string name)
    {
        NpcName = name;
        dialogueCore = new DialogueCore();
    }

    // Quest 상태 조회는 Core가 직접 싱글톤 호출하지 말고, Mono가 주입해주는 콜백으로 받기(가벼운 DI)
    Func<QuestData, QuestState> getQuestState;
    public void BindQuestStateProvider(Func<QuestData, QuestState> provider)
    {
        getQuestState = provider;
    }

    public void AttachDialogueEvents()
    {
        DetachDialogueEvents();

        dialogueCore.OnCommand += HandleCommand;
        dialogueCore.OnEnded += HandleEnded;
    }

    public void DetachDialogueEvents()
    {
        dialogueCore.OnCommand -= HandleCommand;
        dialogueCore.OnEnded -= HandleEnded;
    }

    public void EnterDialogue(DialogueAsset asset, string startNodeId)
    {
        dialogueCore.Start(asset, startNodeId);
    }

    public void EnterDialogue(DialogueAsset asset)
    {
        EnterDialogue(asset, null);
    }

    void HandleEnded()
    {
        OnDialogueEnded?.Invoke();
    }

    void HandleCommand(DialogueCommand command, DialogueAsset nowAsset)
    {
        if (command == null) return;

        switch (command.type)
        {
            case DialogueCommandType.OpenShop:
                OnOpenShopRequested?.Invoke();
                break;

            case DialogueCommandType.StartQuest:
                if (nowAsset != null && nowAsset.questData != null)
                {
                    OnStartQuestRequested?.Invoke(nowAsset.questData);
                }
                break;

            case DialogueCommandType.CompleteQuest:
                if (nowAsset != null && nowAsset.questData != null)
                {
                    OnCompleteQuestRequested?.Invoke(nowAsset.questData);
                }
                break;

            case DialogueCommandType.EnterNewDialogue:
                EnterNewDialogue(command, nowAsset);
                break;
        }
    }

    void EnterNewDialogue(DialogueCommand command, DialogueAsset nowAsset)
    {
        DialogueAsset newAsset = command.newAsset;
        if (newAsset == null) return;

        string startNodeId = null;

        // 새로 진입할 대화 에셋이 '퀘스트 전용 대화'라면,
        // 해당 퀘스트의 진행도에 따라 다른 대화 시작 분기 적용
        // NpcMono(getQuestState)가 있을 때만 가능
        if (newAsset.questData != null && newAsset.questData.questDialogue != null && getQuestState != null)
        {
            QuestState state = getQuestState(newAsset.questData);
            startNodeId = newAsset.questData.GetNodeIdByState(state);
        }

        dialogueCore.Start(newAsset, startNodeId);
    }
}
