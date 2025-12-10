using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Services;

var builder = WebApplication.CreateBuilder(args);

// DbContext InMemory для теста
builder.Services.AddDbContext<ShopDbContext>(opt => opt.UseInMemoryDatabase("ShopDb"));

// Регистрируем сервис группировки
builder.Services.AddScoped<ProductGroupingService>();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Endpoint загрузки Excel
app.MapPost("/upload", async (IFormFile file, ShopDbContext db) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("Файл не выбран");

    if (!file.FileName.EndsWith(".xlsx"))
        return Results.BadRequest("Неправильный формат файла");

    using var stream = file.OpenReadStream();
    using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
    var worksheet = workbook.Worksheet(1);

    var products = new List<Product>();
    foreach (var row in worksheet.RowsUsed().Skip(1))
    {
        var product = new Product
        {
            Name = row.Cell(1).GetValue<string>(),
            UnitName = row.Cell(2).GetValue<string>(),
            Price = row.Cell(3).GetValue<decimal>(),
            Quantity = row.Cell(4).GetValue<int>()
        };
        products.Add(product);
    }

    await db.Products.AddRangeAsync(products);
    await db.SaveChangesAsync();

    return Results.Ok(new { Count = products.Count });
}).DisableAntiforgery();

// Endpoint получения сгруппированных товаров
app.MapGet("/group-products", async (ShopDbContext db, ProductGroupingService service) =>
{
    var products = await db.Products.AsNoTracking().ToListAsync();
    var groups = await service.GroupProductsAsync(products);

    // Преобразуем для JSON с нужными полями
    var result = groups.Select(g => new
    {
        GroupName = g.GroupName,
        Items = g.Items.Select(i => new
        {
            Name = i.Name,
            UnitName = i.UnitName,
            Price = i.Price,
            Quantity = i.Quantity
        }).ToList()
    });

    return Results.Json(result, new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true
    });
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();