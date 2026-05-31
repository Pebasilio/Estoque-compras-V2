using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Data;
using ApiEstoqueRoupas.Models;

namespace ApiEstoqueRoupas.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly DatabaseHelper _databaseHelper;

        public ProductRepository(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var products = new List<Product>();
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT p.Id, p.Name, p.Quantity, p.ReorderThreshold, p.Price, 
                               p.CategoryId, c.Name as CategoryName
                        FROM Products p
                        JOIN Categories c ON p.CategoryId = c.Id
                        ORDER BY p.Name";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(MapProduct(reader));
                        }
                    }
                }
            }
            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT p.Id, p.Name, p.Quantity, p.ReorderThreshold, p.Price, 
                               p.CategoryId, c.Name as CategoryName
                        FROM Products p
                        JOIN Categories c ON p.CategoryId = c.Id
                        WHERE p.Id = @Id";

                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapProduct(reader);
                        }
                    }
                }
            }
            return null;
        }

        public async Task<List<Product>> GetLowStockAsync()
        {
            var products = new List<Product>();
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT p.Id, p.Name, p.Quantity, p.ReorderThreshold, p.Price, 
                               p.CategoryId, c.Name as CategoryName
                        FROM Products p
                        JOIN Categories c ON p.CategoryId = c.Id
                        WHERE p.Quantity <= p.ReorderThreshold
                        ORDER BY p.Quantity ASC";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            products.Add(MapProduct(reader));
                        }
                    }
                }
            }
            return products;
        }

        public async Task<Product> AddAsync(Product product)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Products (Name, Quantity, ReorderThreshold, Price, CategoryId)
                        VALUES (@Name, @Quantity, @ReorderThreshold, @Price, @CategoryId);
                        SELECT last_insert_rowid();";

                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@Quantity", product.Quantity);
                    command.Parameters.AddWithValue("@ReorderThreshold", product.ReorderThreshold);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);

                    var id = (long)await command.ExecuteScalarAsync();
                    product.Id = (int)id;
                }
            }
            return product;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Products
                        SET Name = @Name, 
                            CategoryId = @CategoryId,
                            Quantity = @Quantity, 
                            ReorderThreshold = @ReorderThreshold,
                            Price = @Price
                        WHERE Id = @Id";

                    command.Parameters.AddWithValue("@Id", product.Id);
                    command.Parameters.AddWithValue("@Name", product.Name);
                    command.Parameters.AddWithValue("@CategoryId", product.CategoryId);
                    command.Parameters.AddWithValue("@Quantity", product.Quantity);
                    command.Parameters.AddWithValue("@ReorderThreshold", product.ReorderThreshold);
                    command.Parameters.AddWithValue("@Price", product.Price);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Products WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Products WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    var count = (long)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        public async Task SaveAsync()
        {
            // ADO.NET commits changes immediately, so this is a no-op
            // but keeping for interface compatibility
            await Task.CompletedTask;
        }

        private Product MapProduct(DbDataReader reader)
        {
            return new Product(
                reader.GetString(1),
                reader.GetInt32(5),
                reader.GetInt32(2),
                reader.GetInt32(3),
                reader.GetDecimal(4)
            )
            {
                Id = reader.GetInt32(0),
                Category = new Category
                {
                    Id = reader.GetInt32(5),
                    Name = reader.GetString(6)
                }
            };
        }
    }
}
