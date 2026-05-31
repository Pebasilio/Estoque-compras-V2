using System.Data.SQLite;

namespace ApiEstoqueRoupas.Data
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Categories (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL UNIQUE
                        );

                        CREATE TABLE IF NOT EXISTS Products (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Quantity INTEGER NOT NULL DEFAULT 0,
                            ReorderThreshold INTEGER NOT NULL DEFAULT 0,
                            Price DECIMAL(10, 2) NOT NULL DEFAULT 0,
                            CategoryId INTEGER NOT NULL,
                            FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
                        );

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
                            FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE CASCADE
                        );
                    ";
                    command.ExecuteNonQuery();
                }
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }
    }
}
