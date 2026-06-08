using System.Data.SQLite;

namespace ApiEstoqueRoupas.Data
{
    // ==========================================
    // CLASSE DE CONFIGURAÇÃO DE BANCO DE DADOS
    // Responsável por gerenciar a conexão e criar as tabelas iniciais usando SQLite nativo
    // ==========================================
    public class DatabaseHelper
    {
        // Armazena a string de conexão informando onde o arquivo do banco está salvo (ex: "Data Source=estoque.db")
        private readonly string _connectionString;

        // Construtor que recebe a string de conexão configurada no Program.cs
        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ==========================================
        // MÉTODO DE INICIALIZAÇÃO
        // Executado sempre que a API liga para garantir que o banco de dados exista
        // ==========================================
        public void Initialize()
        {
            // Abre uma conexão temporária usando o padrão 'using' para garantir o fechamento/liberação da memória ao final
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                
                // Cria um comando SQL para rodar instruções diretamente no banco
                using (var command = connection.CreateCommand())
                {
                    // Instruções em "Raw SQL" (SQL puro) para criar tabelas SE elas não existirem
                    // Isso substitui o uso do Entity Framework Migrations, mantendo o sistema bem leve
                    command.CommandText = @"
                        -- Tabela de Categorias
                        CREATE TABLE IF NOT EXISTS Categories (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL UNIQUE
                        );

                        -- Tabela de Produtos (contém chaves estrangeiras)
                        CREATE TABLE IF NOT EXISTS Products (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Quantity INTEGER NOT NULL DEFAULT 0,
                            ReorderThreshold INTEGER NOT NULL DEFAULT 0,
                            Price DECIMAL(10, 2) NOT NULL DEFAULT 0,
                            CategoryId INTEGER NOT NULL,
                            -- ON DELETE RESTRICT garante que não podemos excluir uma categoria se ela tiver produtos vinculados
                            FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
                        );

                        -- Tabela de Histórico de Movimentações
                        CREATE TABLE IF NOT EXISTS StockMovements (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductId INTEGER NOT NULL,
                            ProductName TEXT NOT NULL,
                            Type TEXT NOT NULL,
                            Quantity INTEGER NOT NULL,
                            StockBefore INTEGER NOT NULL,
                            StockAfter INTEGER NOT NULL,
                            Reason TEXT NOT NULL DEFAULT '',
                            Date TEXT NOT NULL,
                            -- ON DELETE CASCADE apaga o histórico de movimentações caso o produto seja deletado do sistema
                            FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
                        );
                    ";
                    
                    // Dispara as queries SQL acima que não esperam um retorno de dados (apenas executam)
                    command.ExecuteNonQuery();
                }
            }
        }

        // Método utilitário que os Repositórios vão usar para abrir novas conexões com o banco a cada requisição
        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}
