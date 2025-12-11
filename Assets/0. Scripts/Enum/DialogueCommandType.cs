// 대화시 실행할 커맨드
public enum DialogueCommandType
{
    None,
    OpenShop,
    EnterNewDialogue, // 현재는 퀘스트 전용 대화 진입 전용
    StartQuest,
    CompleteQuest,
}