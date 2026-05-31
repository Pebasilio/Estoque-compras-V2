using ApiEstoqueRoupas.Data;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using System.Data.SQLite;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Data Source=estoque.db";

builder.Services.AddSingleton(new DatabaseHelper(connectionString));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Initialize database
var databaseHelper = app.Services.GetRequiredService<DatabaseHelper>();
databaseHelper.Initialize();

// Seed initial data if empty
using (var connection = new SQLiteConnection(connectionString))
{
    connection.Open();
    using (var command = connection.CreateCommand())
    {
        command.CommandText = "SELECT COUNT(*) FROM Categories";
        var count = (long)command.ExecuteScalar();

        if (count == 0)
        {
            // Add categories
            var categories = new[] { "Camisas", "Jaquetas", "Calças", "Meias" };
            var categoryIds = new Dictionary<string, int>();

            foreach (var categoryName in categories)
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO Categories (Name) VALUES (@Name); SELECT last_insert_rowid();";
                    cmd.Parameters.AddWithValue("@Name", categoryName);
                    var id = (long)cmd.ExecuteScalar();
                    categoryIds[categoryName] = (int)id;
                }
            }

            // Add products
            var produtosIniciais = new List<(string name, string category, int quantity, int reorderThreshold, decimal price)>
            {
                ("Camisa Polo Azul", "Camisas", 30, 5, 89.90m),
                ("Camisa Branca", "Camisas", 25, 5, 59.90m),
                ("Camisa Preta", "Camisas", 40, 8, 59.90m),
                ("Jaqueta Jeans", "Jaquetas", 20, 3, 199.90m),
                ("Jaqueta de Couro", "Jaquetas", 10, 2, 499.90m),
                ("Calça Jeans Azul", "Calças", 35, 6, 149.90m),
                ("Calça Moletom Cinza", "Calças", 28, 4, 119.90m),
                ("Calça Preta", "Calças", 18, 3, 129.90m),
                ("Meias Brancas (par)", "Meias", 100, 20, 14.90m),
                ("Meias Pretas (par)", "Meias", 80, 15, 14.90m),
                ("Camisa Social Azul", "Camisas", 25, 5, 129.90m),
                ("Camisa Social Branca", "Camisas", 30, 6, 129.90m),
                ("Camisa Estampada", "Camisas", 22, 4, 79.90m),
                ("Jaqueta de Moletom", "Jaquetas", 15, 3, 169.90m),
                ("Jaqueta Puffer", "Jaquetas", 12, 2, 289.90m),
                ("Calça Cargo Verde", "Calças", 20, 5, 139.90m),
                ("Calça Social Preta", "Calças", 17, 3, 159.90m),
                ("Calça Jeans Clara", "Calças", 33, 6, 149.90m),
                ("Meias Coloridas (par)", "Meias", 70, 10, 19.90m),
                ("Meias Esportivas (par)", "Meias", 90, 15, 24.90m)
            };

            foreach (var (name, category, quantity, reorderThreshold, price) in produtosIniciais)
            {
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        INSERT INTO Products (Name, Quantity, ReorderThreshold, Price, CategoryId)
                        VALUES (@Name, @Quantity, @ReorderThreshold, @Price, @CategoryId)";

                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@ReorderThreshold", reorderThreshold);
                    cmd.Parameters.AddWithValue("@Price", price);
                    cmd.Parameters.AddWithValue("@CategoryId", categoryIds[category]);

                    cmd.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"Banco criado com {produtosIniciais.Count} produtos e 4 categorias.");
        }
        else
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Products";
                var productCount = (long)cmd.ExecuteScalar();
                Console.WriteLine($"Banco existente com {productCount} produtos.");
            }
        }
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

app.MapControllers();

Console.WriteLine("\nSERVIDOR RODANDO");
Console.WriteLine("  Swagger:    http://localhost:5123/swagger");
Console.WriteLine("  Produtos:   http://localhost:5123/api/products");
Console.WriteLine("  Categorias: http://localhost:5123/api/categories");
Console.WriteLine("  Estoque:    http://localhost:5123/api/stock/...\n");

app.Run();

app.Run();
