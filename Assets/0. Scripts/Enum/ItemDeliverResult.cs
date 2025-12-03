public enum ItemDeliverResult
{
    None,            // 옮길 대상 자체가 없어서 아무것도 안 함
    Delivered,       // 성공적으로 다른 창으로 옮김
    FailedHasTarget  // 대상은 있었지만, 공간 부족/조건 불일치 등으로 실패
}