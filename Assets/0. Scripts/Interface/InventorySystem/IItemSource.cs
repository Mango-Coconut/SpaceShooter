public interface IItemSource
{
    bool CanRemoveItem(StoredItem item);
    bool TryRemoveItem(StoredItem item);
}
