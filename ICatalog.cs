namespace AspNetCoreFirstApp
{
    public interface ICatalog<T>
    {
        public string GetItemByIndex(int id);
        public string GetItems();
        public void DeleteItem(int id);
        public void AddItem(T item);
        public void UpdateItem(int id, T item);
        public void Clear();
    }
}