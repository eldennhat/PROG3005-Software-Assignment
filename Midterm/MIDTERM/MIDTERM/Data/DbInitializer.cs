using Microsoft.Data.SqlClient;

namespace Lesson3_CNLTWeb.Data
{
    /// <summary>
    /// Tự động tạo database BookManagement và bảng Book khi ứng dụng khởi động.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(IConfiguration configuration)
        {
            var bookConnectionString = configuration.GetConnectionString("BookManagement")
                ?? throw new InvalidOperationException("Connection string 'BookManagement' not found.");

            // Kết nối tới database master để tạo BookManagement nếu chưa tồn tại
            var masterConnectionString = bookConnectionString
                .Replace("Database=BookManagement", "Database=master", StringComparison.OrdinalIgnoreCase);

            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = """
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BookManagement')
                    BEGIN
                        CREATE DATABASE BookManagement;
                    END
                    """;
                command.ExecuteNonQuery();
            }

            // Kết nối tới BookManagement để tạo bảng Book
            using (var connection = new SqlConnection(bookConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = """
                    IF NOT EXISTS (
                        SELECT * FROM sys.tables WHERE name = N'Book' AND schema_id = SCHEMA_ID(N'dbo')
                    )
                    BEGIN
                        CREATE TABLE dbo.Book (
                            Id   INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(200) NOT NULL,
                            Price DECIMAL(18,2) NOT NULL
                        );
                    END
                    """;
                command.ExecuteNonQuery();
            }
        }
    }
}
