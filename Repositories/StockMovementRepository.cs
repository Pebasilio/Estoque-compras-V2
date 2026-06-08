using System;
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
    // REPOSITÓRIO DE MOVIMENTAÇÃO DE ESTOQUE
    // Controla o "Livro Razão" ou diário de tudo que entra e sai.
    // ==========================================
    public class StockMovementRepository : IStockMovementRepository
    {
        // Referência ao helper de banco para obter novas conexões SQLite
        private readonly DatabaseHelper _databaseHelper;

        // Construtor: recebe o DatabaseHelper por injeção de dependência
        public StockMovementRepository(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        // Registra uma nova movimentação. Os dados da movimentação são IMUTÁVEIS (não existe função 'Update' para o histórico).
        public async Task<StockMovement> AddAsync(StockMovement movement)
        {
            // Força a data atual na hora de salvar
            movement.Date = DateTime.Now;

            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        INSERT INTO StockMovements (ProductId, ProductName, Type, Quantity, StockBefore, StockAfter, Reason, Date)
                        VALUES (@ProductId, @ProductName, @Type, @Quantity, @StockBefore, @StockAfter, @Reason, @Date);
                        SELECT last_insert_rowid();";

                    // Preenche todos os parâmetros da query com os dados da movimentação
                    command.Parameters.AddWithValue("@ProductId", movement.ProductId);
                    command.Parameters.AddWithValue("@ProductName", movement.ProductName);
                    // Converte o Enum para string (ex: "ENTRADA") antes de salvar no banco
                    command.Parameters.AddWithValue("@Type", movement.Type.ToString());
                    command.Parameters.AddWithValue("@Quantity", movement.Quantity);
                    command.Parameters.AddWithValue("@StockBefore", movement.StockBefore);
                    command.Parameters.AddWithValue("@StockAfter", movement.StockAfter);
                    // Usa operação de null-coalescing (??) para evitar gravar null no banco
                    command.Parameters.AddWithValue("@Reason", movement.Reason ?? "");
                    // Formato "O" (Round-trip) garante precisão total na data, incluindo fuso horário
                    command.Parameters.AddWithValue("@Date", movement.Date.ToString("O"));

                    // Executa a inserção e captura o ID gerado automaticamente
                    var id = (long)await command.ExecuteScalarAsync();
                    movement.Id = (int)id;
                }
            }

            return movement;
        }

        // Busca todas as movimentações de um produto específico, ordenadas da mais recente para a mais antiga
        public async Task<List<StockMovement>> GetByProductAsync(int productId)
        {
            var movements = new List<StockMovement>();
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Id, ProductId, ProductName, Type, Quantity, StockBefore, StockAfter, Reason, Date
                        FROM StockMovements
                        WHERE ProductId = @ProductId
                        ORDER BY Date DESC";

                    command.Parameters.AddWithValue("@ProductId", productId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            movements.Add(MapMovement((SQLiteDataReader)reader));
                        }
                    }
                }
            }
            return movements;
        }

        // Busca movimentações globais com filtro opcional por tipo (ENTRADA/SAIDA) e limite de resultados
        // A construção dinâmica do SQL adiciona a cláusula WHERE apenas se um filtro for informado
        public async Task<List<StockMovement>> GetAllAsync(MovementType? type, int take = 100)
        {
            var movements = new List<StockMovement>();
            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    var sql = @"
                        SELECT Id, ProductId, ProductName, Type, Quantity, StockBefore, StockAfter, Reason, Date
                        FROM StockMovements";

                    // Se um tipo foi especificado, adiciona a cláusula WHERE dinamicamente
                    if (type.HasValue)
                    {
                        sql += " WHERE Type = @Type";
                        command.Parameters.AddWithValue("@Type", type.Value.ToString());
                    }

                    // Concatena ordenação e limite para não sobrecarregar o Frontend com muitos dados
                    sql += @"
                        ORDER BY Date DESC
                        LIMIT @Take";

                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@Take", take);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            movements.Add(MapMovement((SQLiteDataReader)reader));
                        }
                    }
                }
            }
            return movements;
        }

        // Gera o relatório restrito às movimentações que aconteceram HOJE (para a tela inicial do Dashboard)
        public async Task<List<StockMovement>> GetTodayAsync()
        {
            var movements = new List<StockMovement>();
            // Pega as extremidades do dia de hoje (00:00 até amanhã 00:00) para criar o intervalo seguro no banco
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            using (var connection = _databaseHelper.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        SELECT Id, ProductId, ProductName, Type, Quantity, StockBefore, StockAfter, Reason, Date
                        FROM StockMovements
                        WHERE Date >= @Today AND Date < @Tomorrow
                        ORDER BY Date DESC";

                    command.Parameters.AddWithValue("@Today", today.ToString("O"));
                    command.Parameters.AddWithValue("@Tomorrow", tomorrow.ToString("O"));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            movements.Add(MapMovement((SQLiteDataReader)reader));
                        }
                    }
                }
            }
            return movements;
        }

        // Método utilitário privado que converte uma linha do banco (SQLiteDataReader) em um objeto StockMovement
        // Mapeia cada coluna pelo índice posicional retornado pelo SELECT
        private StockMovement MapMovement(SQLiteDataReader reader)
        {
            return new StockMovement
            {
                Id = reader.GetInt32(0),                              // Coluna 0 = Id
                ProductId = reader.GetInt32(1),                       // Coluna 1 = ProductId
                ProductName = reader.GetString(2),                    // Coluna 2 = ProductName
                Type = Enum.Parse<MovementType>(reader.GetString(3)), // Coluna 3 = Type (converte string para Enum)
                Quantity = reader.GetInt32(4),                        // Coluna 4 = Quantity
                StockBefore = reader.GetInt32(5),                     // Coluna 5 = StockBefore
                StockAfter = reader.GetInt32(6),                      // Coluna 6 = StockAfter
                Reason = reader.GetString(7),                         // Coluna 7 = Reason
                Date = DateTime.Parse(reader.GetString(8))            // Coluna 8 = Date (converte string ISO para DateTime)
            };
        }
    }
}
