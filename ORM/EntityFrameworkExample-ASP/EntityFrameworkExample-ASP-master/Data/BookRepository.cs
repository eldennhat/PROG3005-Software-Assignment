using Lesson3_CNLTWeb.Models;
using Microsoft.Data.SqlClient;

namespace Lesson3_CNLTWeb.Data
{
    /// <summary>
    /// Thực hiện các thao tác CRUD với bảng Book qua ADO.NET.
    /// </summary>
    public class BookRepository
    {
        private readonly string _connectionString;

        public BookRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("BookManagement")
                ?? throw new InvalidOperationException("Connection string 'BookManagement' not found.");
        }

        public List<Book> GetAll()
        {
            var books = new List<Book>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Price FROM dbo.Book ORDER BY Id";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                books.Add(MapBook(reader));
            }

            return books;
        }

        public Book? GetById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Price FROM dbo.Book WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapBook(reader) : null;
        }

        public void Add(Book book)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO dbo.Book (Name, Price)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Price);
                """;
            command.Parameters.AddWithValue("@Name", book.Name);
            command.Parameters.AddWithValue("@Price", book.Price);

            book.Id = (int)command.ExecuteScalar()!;
        }

        public bool Update(Book book)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE dbo.Book
                SET Name = @Name, Price = @Price
                WHERE Id = @Id;
                """;
            command.Parameters.AddWithValue("@Id", book.Id);
            command.Parameters.AddWithValue("@Name", book.Name);
            command.Parameters.AddWithValue("@Price", book.Price);

            return command.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM dbo.Book WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            return command.ExecuteNonQuery() > 0;
        }

        private static Book MapBook(SqlDataReader reader)
        {
            return new Book
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price"))
            };
        }
    }
}
