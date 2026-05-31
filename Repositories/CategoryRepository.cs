using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Data;
using ApiEstoqueRoupas.Models;

namespace ApiEstoqueRoupas.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DatabaseHelper _databaseHelper;

        public CategoryRepository(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public async Task<List<Category>> GetAllAsync()
        {
            var categories = new List<Category>();
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT DISTINCT c.Id, c.Name
                        FROM Categories c
                        LEFT JOIN Products p ON c.Id = p.CategoryId
                        ORDER BY c.Name";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(MapCategory(reader));
                        }
                    }
                }

                // Load products for each category
                foreach (var category in categories)
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            SELECT Id, Name, Quantity, ReorderThreshold, Price, CategoryId
                            FROM Products
                            WHERE CategoryId = @CategoryId
                            ORDER BY Name";

                        command.Parameters.AddWithValue("@CategoryId", category.Id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                category.Products.Add(new Product(
                                    reader.GetString(1),
                                    reader.GetInt32(5),
                                    reader.GetInt32(2),
                                    reader.GetInt32(3),
                                    reader.GetDecimal(4)
                                )
                                {
                                    Id = reader.GetInt32(0)
                                });
                            }
                        }
                    }
                }
            }
            return categories;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                Category? category = null;

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id, Name FROM Categories WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            category = MapCategory(reader);
                        }
                    }
                }

                if (category == null) return null;

                // Load products
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Id, Name, Quantity, ReorderThreshold, Price, CategoryId
                        FROM Products
                        WHERE CategoryId = @CategoryId
                        ORDER BY Name";

                    command.Parameters.AddWithValue("@CategoryId", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            category.Products.Add(new Product(
                                reader.GetString(1),
                                reader.GetInt32(5),
                                reader.GetInt32(2),
                                reader.GetInt32(3),
                                reader.GetDecimal(4)
                            )
                            {
                                Id = reader.GetInt32(0)
                            });
                        }
                    }
                }

                return category;
            }
        }

        public async Task<Category> AddAsync(Category category)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO Categories (Name)
                        VALUES (@Name);
                        SELECT last_insert_rowid();";

                    command.Parameters.AddWithValue("@Name", category.Name);

                    var id = (long)await command.ExecuteScalarAsync();
                    category.Id = (int)id;
                }
            }
            return category;
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE Categories SET Name = @Name WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Name", category.Name);
                    command.Parameters.AddWithValue("@Id", category.Id);

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

                // Check if category has products
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId";
                    command.Parameters.AddWithValue("@CategoryId", id);

                    var count = (long)await command.ExecuteScalarAsync();
                    if (count > 0) return false;
                }

                // Delete category
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Categories WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        private Category MapCategory(DbDataReader reader)
        {
            return new Category
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Products = new List<Product>()
            };
        }
    }
}
