public interface ISwapSink : IItemSink
{
    // 이 아이템을 넣을 때, 자리가 차 있으면 기존 아이템을 swapped로 돌려준다.
    bool CanAddItemSwap(StoredItem item, out StoredItem willBeSwapped);
    bool TryAddItemSwap(StoredItem item, out StoredItem swapped);
}