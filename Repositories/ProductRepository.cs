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
    // ==========================================
    // REPOSITÓRIO DE PRODUTOS
    // Contém as queries de SQL puro (Raw SQL) para fazer o CRUD no banco SQLite
    // ==========================================
    public class ProductRepository : IProductRepository
    {
        // Instância do ajudante que gerencia e entrega novas conexões
        private readonly DatabaseHelper _databaseHelper;

        // Recebe o helper de banco via injeção de dependência para obter conexões
        public ProductRepository(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        // Busca todos os produtos usando um 'JOIN' para trazer já o nome da Categoria junto
        public async Task<List<Product>> GetAllAsync()
        {
            var products = new List<Product>();
            // Abre a conexão com o banco
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

        // Busca um produto específico pelo ID, fazendo JOIN com a tabela de categorias
        // Retorna null se o produto não existir
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

        // Uma das funções mais importantes do sistema: Busca APENAS produtos com estoque em perigo
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

        // Insere um novo produto no banco.
        // Utiliza parâmetros nomeados (@Name, @Price) para evitar vulnerabilidades de "SQL Injection"
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
                        -- Após inserir, retorna o ID gerado automaticamente
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

        // Atualiza todos os campos de um produto existente no banco
        // Retorna true se a operação afetou pelo menos uma linha (produto encontrado e atualizado)
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

                    // ExecuteNonQueryAsync retorna o número de linhas afetadas pelo UPDATE
                    var result = await command.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
        }

        // Remove um produto do banco pelo ID. Retorna true se o produto foi encontrado e excluído
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

        // Verifica rapidamente se um produto existe, sem carregar todos os seus dados na memória
        // Útil para validações rápidas antes de operações que dependem da existência do produto
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

        // Método de compatibilidade com a interface. No ADO.NET puro, cada comando já é "commitado" automaticamente,
        // então este método não precisa fazer nada. Existe apenas para manter o contrato da interface
        public async Task SaveAsync()
        {
            // ADO.NET faz commit imediato, então isso é um no-op (operação vazia)
            // Mantido para compatibilidade com a interface
            await Task.CompletedTask;
        }

        // Método utilitário privado que transforma uma linha do banco (DbDataReader) em um objeto Product
        // Também cria o objeto Category aninhado usando os dados do JOIN
        private Product MapProduct(DbDataReader reader)
        {
            return new Product(
                reader.GetString(1),   // Coluna 1 = Name
                reader.GetInt32(5),    // Coluna 5 = CategoryId
                reader.GetInt32(2),    // Coluna 2 = Quantity
                reader.GetInt32(3),    // Coluna 3 = ReorderThreshold
                reader.GetDecimal(4)   // Coluna 4 = Price
            )
            {
                Id = reader.GetInt32(0), // Coluna 0 = Id do produto
                // Monta o objeto Category inline para preencher a propriedade de navegação
                Category = new Category
                {
                    Id = reader.GetInt32(5),    // Mesmo CategoryId
                    Name = reader.GetString(6)  // Coluna 6 = CategoryName (vindo do JOIN)
                }
            };
        }
    }
}
