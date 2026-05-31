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
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly DatabaseHelper _databaseHelper;

        public StockMovementRepository(DatabaseHelper databaseHelper)
        {
            _databaseHelper = databaseHelper;
        }

        public async Task<StockMovement> AddAsync(StockMovement movement)
        {
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

                    command.Parameters.AddWithValue("@ProductId", movement.ProductId);
                    command.Parameters.AddWithValue("@ProductName", movement.ProductName);
                    command.Parameters.AddWithValue("@Type", movement.Type.ToString());
                    command.Parameters.AddWithValue("@Quantity", movement.Quantity);
                    command.Parameters.AddWithValue("@StockBefore", movement.StockBefore);
                    command.Parameters.AddWithValue("@StockAfter", movement.StockAfter);
                    command.Parameters.AddWithValue("@Reason", movement.Reason ?? "");
                    command.Parameters.AddWithValue("@Date", movement.Date.ToString("O"));

                    var id = (long)await command.ExecuteScalarAsync();
                    movement.Id = (int)id;
                }
            }

            return movement;
        }

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

                    if (type.HasValue)
                    {
                        sql += " WHERE Type = @Type";
                        command.Parameters.AddWithValue("@Type", type.Value.ToString());
                    }

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

        public async Task<List<StockMovement>> GetTodayAsync()
        {
            var movements = new List<StockMovement>();
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

        private StockMovement MapMovement(SQLiteDataReader reader)
        {
            return new StockMovement
            {
                Id = reader.GetInt32(0),
                ProductId = reader.GetInt32(1),
                ProductName = reader.GetString(2),
                Type = Enum.Parse<MovementType>(reader.GetString(3)),
                Quantity = reader.GetInt32(4),
                StockBefore = reader.GetInt32(5),
                StockAfter = reader.GetInt32(6),
                Reason = reader.GetString(7),
                Date = DateTime.Parse(reader.GetString(8))
            };
        }
    }
}
