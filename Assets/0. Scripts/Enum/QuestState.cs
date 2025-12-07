public enum QuestState
{
    Locked,            // 제한 조건으로 인해 받을 수 없음
    CanAccept,          // 시작 가능 상태
    Active,            // 진행 중 (목표 달성 전)
    ReadyToTurnIn,     // 보고 가능
    Completed          // 완료
}