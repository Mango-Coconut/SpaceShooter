public interface IStorable
{
    public bool TryAddItem(ItemData data, int amount = 1);
    public bool TryAddItem(StoredItem item);
    public bool TryRemoveItem(ItemData data, int amount = 1);
    public bool TryRemoveItem(StoredItem item);
}
