using System.Collections.Concurrent;

namespace AspNetCoreFirstApp
{
    public class ThreadSafeCatalog : ICatalog<Item>
    {
        private readonly ThreadSafeCollection<Item> items = new();

        public void AddItem(Item item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public void DeleteItem(int index)
        {
            items.RemoveAt(index);
        }

        public string GetItemByIndex(int index)
        {
            return items[index].ToString();
        }

        public string GetItems()
        {
            return items.ToString();
        }

        public void UpdateItem(int index, Item item)
        {
            items[index] = item;
        }
    }
}
