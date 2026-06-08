using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Data;
using ApiEstoqueRoupas.Models;

namespace ApiEstoqueRoupas.Repositories
{
    // ==========================================
    // REPOSITÓRIO DE CATEGORIAS
    // Executa operações relacionadas às Categorias e seus vínculos
    // ==========================================
    public class CategoryRepository : ICategoryRepository
    {
        // Referência ao helper que fornece novas conexões com o banco SQLite
        private readonly DatabaseHelper _databaseHelper;

        // Construtor: recebe o DatabaseHelper via injeção de dependência
        public CategoryRepository(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        // Carrega todas as categorias.
        // Além da categoria, ele executa uma segunda query (loop) para preencher os produtos dentro de cada categoria (Eager Loading manual)
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

                // Carrega os produtos vinculados a cada categoria (simula o "Include" do Entity Framework)
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

        // Busca uma única categoria pelo ID, incluindo seus produtos vinculados
        // Retorna null se a categoria não existir no banco
        public async Task<Category?> GetByIdAsync(int id)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                Category? category = null;

                // Consulta a tabela Categories buscando pelo ID parametrizado
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Id, Name FROM Categories WHERE Id = @Id";
                    // Parâmetro nomeado previne SQL Injection
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            category = MapCategory(reader);
                        }
                    }
                }

                // Se a categoria não foi encontrada, retorna null imediatamente
                if (category == null) return null;

                // Carrega os produtos pertencentes a esta categoria
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

        // Insere uma nova categoria no banco e retorna o objeto com o ID gerado
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

                    // Insere a categoria e recupera o ID gerado automaticamente pelo SQLite
                    var id = (long)await command.ExecuteScalarAsync();
                    // Atribui o ID gerado ao objeto para devolver ao Controller com a informação completa
                    category.Id = (int)id;
                }
            }
            return category;
        }

        // Atualiza o nome de uma categoria existente. Retorna true se alguma linha foi afetada
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

                    // Executa o UPDATE e verifica se alguma linha foi afetada
                    // Se result > 0, significa que a categoria foi encontrada e atualizada com sucesso
                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        // Deleta uma categoria se (e somente se) não houver produtos associados a ela
        public async Task<bool> DeleteAsync(int id)
        {
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();

                // Passo 1: Verifica se existem produtos usando esta categoria
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId";
                    command.Parameters.AddWithValue("@CategoryId", id);

                    var count = (long)await command.ExecuteScalarAsync();
                    if (count > 0) return false;
                }

                // Passo 2: Se não há produtos vinculados, prossegue com a exclusão da categoria
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM Categories WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    // Retorna true se alguma linha foi realmente removida
                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        // Método utilitário privado que converte uma linha do banco (DbDataReader) em um objeto Category
        // Centraliza a lógica de mapeamento para evitar duplicação de código
        private Category MapCategory(DbDataReader reader)
        {
            return new Category
            {
                Id = reader.GetInt32(0),      // Coluna 0 = Id
                Name = reader.GetString(1),    // Coluna 1 = Name
                Products = new List<Product>() // Inicializa a lista vazia (será preenchida depois, se necessário)
            };
        }
    }
}
