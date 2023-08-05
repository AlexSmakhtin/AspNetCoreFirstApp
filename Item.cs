namespace AspNetCoreFirstApp
{
    public class Item
    {
        public string Name { get; set; }
        public decimal Cost { get; set; }
        public Item(string name, decimal cost)
        {
            if (string.IsNullOrEmpty(name))
                Name = string.Empty;
            if (cost <= 0)
                cost = 0;
            Name = name;
            Cost = cost;
        }
        public override string ToString()
        {
            return $"{Name} - {Cost}";
        }
    }
}
