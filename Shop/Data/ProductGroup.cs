public class ProductGroup
{
    public string GroupName { get; set; } = null!;
    public List<ProductInGroup> Items { get; set; } = new();
}