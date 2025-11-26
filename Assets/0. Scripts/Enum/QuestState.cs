public enum QuestState
{
    Inactive,          // 아직 받지도 않음
    Active,            // 진행 중 (목표 달성 전)
    ReadyToTurnIn,     // 목표는 다 했고, NPC에게 보고하러 가야 함
    Completed          // 보상까지 다 받음
}