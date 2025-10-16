public interface IStorable
{
    public bool TryAddItem(ItemData data, int amount = 1);
    public bool TryRemoveItem(ItemData data, int amount = 1);
}
