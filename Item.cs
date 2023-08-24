namespace AspNetCoreFirstApp
{
    public class Item
    {
        public string Name { get; set; }
        public decimal Cost { get; set; }
        public Item(string name, decimal cost)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (cost <= 0)
                throw new ArgumentNullException(nameof(cost));
            Name = name;
            Cost = cost;
        }
        public override string ToString()
        {
            return $"{Name} - {(DateTime.Now.DayOfWeek == DayOfWeek.Monday ? Cost - Cost * 0.3m : Cost)}";
        }
    }
}
