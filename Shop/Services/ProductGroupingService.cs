namespace Shop.Services
{
    public class ProductGroupingService
    {
        private readonly decimal _groupLimit;

        public ProductGroupingService(decimal groupLimit = 200)
        {
            _groupLimit = groupLimit;
        }

        public Task<List<ProductGroup>> GroupProductsAsync(List<Product> products)
        {
            const decimal maxGroupPrice = 200m;
            var groups = new List<ProductGroup>
    {
        new ProductGroup { GroupName = "Группа 1" },
        new ProductGroup { GroupName = "Группа 2" },
        new ProductGroup { GroupName = "Группа 3" },
        new ProductGroup { GroupName = "Группа 4" }
    };

            // Сортируем товары по цене за единицу по убыванию
            var sortedProducts = products.OrderByDescending(p => p.Price).ToList();

            foreach (var product in sortedProducts)
            {
                int remainingQty = product.Quantity;

                foreach (var group in groups)
                {
                    if (remainingQty == 0) break;

                    decimal currentGroupTotal = group.Items.Sum(i => i.Price * i.Quantity);
                    int maxQtyThatFits = (int)((maxGroupPrice - currentGroupTotal) / product.Price);

                    int qtyToAdd = Math.Min(remainingQty, maxQtyThatFits);
                    if (qtyToAdd > 0)
                    {
                        group.Items.Add(new ProductInGroup
                        {
                            Name = product.Name,
                            UnitName = product.UnitName,
                            Price = product.Price,
                            Quantity = qtyToAdd
                        });
                        remainingQty -= qtyToAdd;
                    }
                }

                // Если остались единицы, создаем новую группу
                while (remainingQty > 0)
                {
                    var newGroup = new ProductGroup
                    {
                        GroupName = $"Группа {groups.Count + 1}"
                    };
                    int qtyToAdd = Math.Min(remainingQty, (int)(maxGroupPrice / product.Price));
                    newGroup.Items.Add(new ProductInGroup
                    {
                        Name = product.Name,
                        UnitName = product.UnitName,
                        Price = product.Price,
                        Quantity = qtyToAdd
                    });
                    remainingQty -= qtyToAdd;
                    groups.Add(newGroup);
                }
            }

            return Task.FromResult(groups);
        }

    }
}