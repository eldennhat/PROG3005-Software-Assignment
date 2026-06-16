using ImageUpload.Models;
using Microsoft.Data.SqlClient;

namespace ImageUpload.Data
{
    /// <summary>
    /// Thực hiện các thao tác CRUD với bảng Book qua ADO.NET.
    /// </summary>
    public class BookRepository {
        private readonly string _connectionString;

        public BookRepository(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("BookManagement")
                ?? throw new InvalidOperationException("Connection string 'BookManagement' not found.");
        }

        public List<Book> GetAll() {
            var books = new List<Book>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Price, ImageUrl FROM dbo.Book ORDER BY Id";

            using var reader = command.ExecuteReader();
            while (reader.Read()) {
                books.Add(MapBook(reader));
            }

            return books;
        }

        public Book? GetById(int id) {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Price, ImageUrl FROM dbo.Book WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapBook(reader) : null;
        }

        public void Add(Book book) {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO dbo.Book (Name, Price, ImageUrl)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Price, @ImageUrl);
                """;
            command.Parameters.AddWithValue("@Name", book.Name);
            command.Parameters.AddWithValue("@Price", book.Price);
            command.Parameters.AddWithValue("@ImageUrl", book.ImageUrl ?? (object)DBNull.Value);

            book.Id = (int)command.ExecuteScalar()!;
        }

        public bool Update(Book book) {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE dbo.Book
                SET Name = @Name, Price = @Price, ImageUrl = @ImageUrl
                WHERE Id = @Id;
                """;
            command.Parameters.AddWithValue("@Id", book.Id);
            command.Parameters.AddWithValue("@Name", book.Name);
            command.Parameters.AddWithValue("@Price", book.Price);
            command.Parameters.AddWithValue("@ImageUrl", book.ImageUrl ?? (object)DBNull.Value);

            return command.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id) {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM dbo.Book WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            return command.ExecuteNonQuery() > 0;
        }

        private static Book MapBook(SqlDataReader reader)
        {
            var book = new Book() {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price"))
            };

            int imageColIndex = reader.GetOrdinal("ImageUrl");
            if (!reader.IsDBNull(imageColIndex)) {
                book.ImageUrl = reader.GetString(imageColIndex);
            }
            return book;
        }
    }
}
