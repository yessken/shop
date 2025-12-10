namespace Shop.Services
{
    using Shop.Data;

    public class ProductGroupingService
    {
        private readonly decimal _maxGroupPrice = 200m;

        public Task<List<ProductGroup>> GroupProductsAsync(List<Product> products)
        {
            var groups = new List<ProductGroup>();
            int groupNumber = 1;

            var remainingProducts = products.Select(p => new Product
            {
                Name = p.Name,
                UnitName = p.UnitName,
                Price = p.Price,
                Quantity = p.Quantity
            }).ToList();

            while (remainingProducts.Any())
            {
                var group = new ProductGroup { GroupName = $"Группа {groupNumber}" };
                decimal groupSum = 0;

                foreach (var product in remainingProducts.ToList())
                {
                    int maxQuantity = (int)Math.Floor((_maxGroupPrice - groupSum) / product.Price);
                    if (maxQuantity > 0)
                    {
                        int quantityToAdd = Math.Min(maxQuantity, product.Quantity);
                        group.Items.Add(new ProductInGroup
                        {
                            Name = product.Name,
                            UnitName = product.UnitName,
                            Price = product.Price,
                            Quantity = quantityToAdd
                        });

                        groupSum += product.Price * quantityToAdd;
                        product.Quantity -= quantityToAdd;
                        if (product.Quantity == 0)
                            remainingProducts.Remove(product);
                    }
                }

                groups.Add(group);
                groupNumber++;
            }

            return Task.FromResult(groups);
        }
    }
}