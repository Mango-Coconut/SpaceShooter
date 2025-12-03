public enum QuestState
{
    Locked,            // 받을 수 없음
    CanAccept,          // 시작 가능 상태
    Active,            // 진행 중 (목표 달성 전)
    ReadyToTurnIn,     // 목표는 다 했고, NPC에게 보고하러 가야 함

    Completed          // 보상까지 다 받음
}