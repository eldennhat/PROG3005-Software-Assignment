using Microsoft.Data.SqlClient;
using MIDTERM.Models;
using System.Data;

namespace MIDTERM.Data
{
    /// <summary>
    /// Implements CRUD operations for Dish, Category, and Image tables via raw ADO.NET 
    /// </summary>
    public class DishRepository
    {
        private readonly string _connectionString;

        public DishRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MID_BIT240199")
                                ?? throw new InvalidOperationException("Connection string 'MID_BIT240199' not found.");
        }
        

        public List<DishCategory_BIT240199> GetAllCategories()
        {
            var categories = new List<DishCategory_BIT240199>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Description FROM dbo.DishCategories_BIT240199 ORDER BY Name ASC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                categories.Add(MapCategory(reader));
            }

            return categories;
        }

        public DishCategory_BIT240199? GetCategoryById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Name, Description FROM dbo.DishCategories_BIT240199 WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapCategory(reader) : null;
        }

        public bool CheckCategoryHasDishes(int categoryId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(1) FROM dbo.Dishes_BIT240199 WHERE DishCategoryId = @DishCategoryId";
            command.Parameters.AddWithValue("@DishCategoryId", categoryId);

            int count = (int)command.ExecuteScalar()!;
            return count > 0;
        }

        public bool DeleteCategory(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM dbo.DishCategories_BIT240199 WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            return command.ExecuteNonQuery() > 0;
        }

        // ==================== DISH OPERATIONS (Requirement 2: Search & Filter) ====================

        public List<Dish_BIT240199> SearchDishes(string? searchTerm, int? categoryId, string? availability, decimal? minPrice, decimal? maxPrice, string? sortBy)
        {
            var dishes = new List<Dish_BIT240199>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            
            // Build raw dynamic SQL command text natively to filter on DB tier directly
            string query = @"
                SELECT d.Id, d.Name, d.Price, d.PreparationTime, d.IsAvailable, d.Description, d.DishCategoryId,
                       c.Name AS CategoryName,
                       (SELECT TOP 1 ImageUrl FROM dbo.DishImages_BIT240199 WHERE DishId = d.Id AND IsThumbnail = 1) AS ThumbnailUrl
                FROM dbo.Dishes_BIT240199 d
                JOIN dbo.DishCategories_BIT240199 c ON d.DishCategoryId = c.Id
                WHERE 1=1";

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query += " AND d.Name LIKE @SearchTerm";
                command.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
            }

            if (categoryId.HasValue)
            {
                query += " AND d.DishCategoryId = @DishCategoryId";
                command.Parameters.AddWithValue("@DishCategoryId", categoryId.Value);
            }

            if (!string.IsNullOrEmpty(availability))
            {
                if (availability == "Available")
                {
                    query += " AND d.IsAvailable = 1";
                }
                else if (availability == "Unavailable")
                {
                    query += " AND d.IsAvailable = 0";
                }
            }

            if (minPrice.HasValue)
            {
                query += " AND d.Price >= @MinPrice";
                command.Parameters.AddWithValue("@MinPrice", minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query += " AND d.Price <= @MaxPrice";
                command.Parameters.AddWithValue("@MaxPrice", maxPrice.Value);
            }

            // Ordering conditions matching requirement specifications
            if (sortBy == "PriceAsc") query += " ORDER BY d.Price ASC";
            else if (sortBy == "PriceDesc") query += " ORDER BY d.Price DESC";
            else if (sortBy == "PrepTimeAsc") query += " ORDER BY d.PreparationTime ASC";
            else query += " ORDER BY d.Id DESC";

            command.CommandText = query;

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var dish = MapDish(reader);
                
                // Read joined category metadata natively
                dish.DishCategory = new DishCategory_BIT240199
                {
                    Id = dish.DishCategoryId,
                    Name = reader.GetString(reader.GetOrdinal("CategoryName"))
                };

                // Read injected subquery thumbnail natively if available
                int thumbColIndex = reader.GetOrdinal("ThumbnailUrl");
                if (!reader.IsDBNull(thumbColIndex))
                {
                    dish.DishImages.Add(new DishImage_BIT240199 
                    { 
                        ImageUrl = reader.GetString(thumbColIndex), 
                        IsThumbnail = true 
                    });
                }

                dishes.Add(dish);
            }

            return dishes;
        }

        public Dish_BIT240199? GetDishById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT d.Id, d.Name, d.Price, d.PreparationTime, d.IsAvailable, d.Description, d.DishCategoryId,
                       c.Name AS CategoryName
                FROM dbo.Dishes_BIT240199 d
                JOIN dbo.DishCategories_BIT240199 c ON d.DishCategoryId = c.Id
                WHERE d.Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var dish = MapDish(reader);
                dish.DishCategory = new DishCategory_BIT240199
                {
                    Id = dish.DishCategoryId,
                    Name = reader.GetString(reader.GetOrdinal("CategoryName"))
                };
                return dish;
            }
            return null;
        }

        public bool IsDishNameExists(string name, int? excludeId, int categoryId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            string query = "SELECT COUNT(1) FROM dbo.Dishes_BIT240199 WHERE Name = @Name AND DishCategoryId = @DishCategoryId";
            
            if (excludeId.HasValue)
            {
                query += " AND Id != @ExcludeId";
                command.Parameters.AddWithValue("@ExcludeId", excludeId.Value);
            }

            command.CommandText = query;
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@DishCategoryId", categoryId);

            int count = (int)command.ExecuteScalar()!;
            return count > 0;
        }

        public void AddDish(Dish_BIT240199 dish)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO dbo.Dishes_BIT240199 (Name, Price, PreparationTime, IsAvailable, Description, DishCategoryId)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Price, @PreparationTime, @IsAvailable, @Description, @DishCategoryId);
                """;
            command.Parameters.AddWithValue("@Name", dish.Name);
            command.Parameters.AddWithValue("@Price", dish.Price);
            command.Parameters.AddWithValue("@PreparationTime", dish.PreparationTime);
            command.Parameters.AddWithValue("@IsAvailable", dish.IsAvailable ? 1 : 0);
            command.Parameters.AddWithValue("@Description", dish.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DishCategoryId", dish.DishCategoryId);

            dish.Id = (int)command.ExecuteScalar()!;
        }

        public bool UpdateDish(Dish_BIT240199 dish)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = """
                UPDATE dbo.Dishes_BIT240199
                SET Name = @Name, Price = @Price, PreparationTime = @PreparationTime, 
                    IsAvailable = @IsAvailable, Description = @Description, DishCategoryId = @DishCategoryId
                WHERE Id = @Id;
                """;
            command.Parameters.AddWithValue("@Id", dish.Id);
            command.Parameters.AddWithValue("@Name", dish.Name);
            command.Parameters.AddWithValue("@Price", dish.Price);
            command.Parameters.AddWithValue("@PreparationTime", dish.PreparationTime);
            command.Parameters.AddWithValue("@IsAvailable", dish.IsAvailable ? 1 : 0);
            command.Parameters.AddWithValue("@Description", dish.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@DishCategoryId", dish.DishCategoryId);

            return command.ExecuteNonQuery() > 0;
        }

        public bool DeleteDish(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Clear related images first to satisfy constraint rules safely
            using (var imgCmd = connection.CreateCommand())
            {
                imgCmd.CommandText = "DELETE FROM dbo.DishImages_BIT240199 WHERE DishId = @DishId";
                imgCmd.Parameters.AddWithValue("@DishId", id);
                imgCmd.ExecuteNonQuery();
            }

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM dbo.Dishes_BIT240199 WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            return command.ExecuteNonQuery() > 0;
        }

        // ==================== DISH IMAGE OPERATIONS (Requirement 4) ====================

        public List<DishImage_BIT240199> GetImagesByDishId(int dishId)
        {
            var images = new List<DishImage_BIT240199>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, ImageUrl, IsThumbnail, DishId FROM dbo.DishImages_BIT240199 WHERE DishId = @DishId";
            command.Parameters.AddWithValue("@DishId", dishId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                images.Add(MapDishImage(reader));
            }

            return images;
        }

        public DishImage_BIT240199? GetImageById(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, ImageUrl, IsThumbnail, DishId FROM dbo.DishImages_BIT240199 WHERE Id = @Id";
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapDishImage(reader) : null;
        }

        public void AddDishImage(DishImage_BIT240199 img)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Rule setup: If requested as thumbnail, clear previous selections first
            if (img.IsThumbnail)
            {
                using var resetCmd = connection.CreateCommand();
                resetCmd.CommandText = "UPDATE dbo.DishImages_BIT240199 SET IsThumbnail = 0 WHERE DishId = @DishId";
                resetCmd.Parameters.AddWithValue("@DishId", img.DishId);
                resetCmd.ExecuteNonQuery();
            }
            else
            {
                // Fallback check: Set as thumbnail if this is the first image record
                using var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(1) FROM dbo.DishImages_BIT240199 WHERE DishId = @DishId";
                checkCmd.Parameters.AddWithValue("@DishId", img.DishId);
                if ((int)checkCmd.ExecuteScalar()! == 0)
                {
                    img.IsThumbnail = true;
                }
            }

            using var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO dbo.DishImages_BIT240199 (ImageUrl, IsThumbnail, DishId)
                VALUES (@ImageUrl, @IsThumbnail, @DishId);
                """;
            command.Parameters.AddWithValue("@ImageUrl", img.ImageUrl);
            command.Parameters.AddWithValue("@IsThumbnail", img.IsThumbnail ? 1 : 0);
            command.Parameters.AddWithValue("@DishId", img.DishId);

            command.ExecuteNonQuery();
        }

        public void SetThumbnail(int imageId, int dishId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // Clear old thumbnail configurations
            using (var resetCmd = connection.CreateCommand())
            {
                resetCmd.CommandText = "UPDATE dbo.DishImages_BIT240199 SET IsThumbnail = 0 WHERE DishId = @DishId";
                resetCmd.Parameters.AddWithValue("@DishId", dishId);
                resetCmd.ExecuteNonQuery();
            }

            // Assign new thumbnail status
            using (var setCmd = connection.CreateCommand())
            {
                setCmd.CommandText = "UPDATE dbo.DishImages_BIT240199 SET IsThumbnail = 1 WHERE Id = @Id";
                setCmd.Parameters.AddWithValue("@Id", imageId);
                setCmd.ExecuteNonQuery();
            }
        }

        // ==================== OBJECT MAPPING CORE METHODS ====================

        private static DishCategory_BIT240199 MapCategory(SqlDataReader reader)
        {
            var category = new DishCategory_BIT240199
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name"))
            };

            int descCol = reader.GetOrdinal("Description");
            if (!reader.IsDBNull(descCol))
            {
                category.Description = reader.GetString(descCol);
            }

            return category;
        }

        private static Dish_BIT240199 MapDish(SqlDataReader reader)
        {
            var dish = new Dish_BIT240199
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                PreparationTime = reader.GetInt32(reader.GetOrdinal("PreparationTime")),
                IsAvailable = reader.GetBoolean(reader.GetOrdinal("IsAvailable")),
                DishCategoryId = reader.GetInt32(reader.GetOrdinal("DishCategoryId"))
            };

            int descCol = reader.GetOrdinal("Description");
            if (!reader.IsDBNull(descCol))
            {
                dish.Description = reader.GetString(descCol);
            }

            return dish;
        }

        private static DishImage_BIT240199 MapDishImage(SqlDataReader reader)
        {
            return new DishImage_BIT240199
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                ImageUrl = reader.GetString(reader.GetOrdinal("ImageUrl")),
                IsThumbnail = reader.GetBoolean(reader.GetOrdinal("IsThumbnail")),
                DishId = reader.GetInt32(reader.GetOrdinal("DishId"))
            };
        }
    }
}