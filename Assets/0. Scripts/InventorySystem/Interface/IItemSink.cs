public interface IItemSink
{
    bool CanAddItem(StoredItem item);
    bool TryAddItem(StoredItem item);
}