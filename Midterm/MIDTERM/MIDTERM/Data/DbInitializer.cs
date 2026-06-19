using Microsoft.Data.SqlClient;

namespace MIDTERM.Data
{
    /// <summary>
    /// Automatically ensures database and required tables exist on application startup with seeded records for DishManagement.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(IConfiguration configuration)
        {
            var targetConnectionString = configuration.GetConnectionString("MID_BIT240199")
                ?? throw new InvalidOperationException("Connection string 'MID_BIT240199' not found.");
            
            var builder = new SqlConnectionStringBuilder(targetConnectionString);
            builder.InitialCatalog = "master"; 
            var masterConnectionString = builder.ConnectionString;

            // STEP 1: Connect to master database to create target database if not exists
            using (var connection = new SqlConnection(masterConnectionString))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = """
                    IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'MID_BIT240199')
                    BEGIN
                        CREATE DATABASE MID_BIT240199;
                    END
                    """;
                command.ExecuteNonQuery();
            }

            // STEP 2: Connect to MID_BIT240199 database to construct infrastructure and tables
            using (var connection = new SqlConnection(targetConnectionString))
            {
                connection.Open();

                // 2.1. Create DishCategories table
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = """
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'DishCategories_BIT240199' AND schema_id = SCHEMA_ID(N'dbo'))
                        BEGIN
                            CREATE TABLE dbo.DishCategories_BIT240199 (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Name NVARCHAR(100) NOT NULL,
                                Description NVARCHAR(MAX) NULL
                            );
                        END
                        """;
                    command.ExecuteNonQuery();
                }

                // 2.2. Create Dishes table with foreign key reference
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = """
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'Dishes_BIT240199' AND schema_id = SCHEMA_ID(N'dbo'))
                        BEGIN
                            CREATE TABLE dbo.Dishes_BIT240199 (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Name NVARCHAR(100) NOT NULL,
                                Price DECIMAL(18,2) NOT NULL,
                                PreparationTime INT NOT NULL,
                                IsAvailable BIT NOT NULL DEFAULT 1,
                                Description NVARCHAR(MAX) NULL,
                                DishCategoryId INT NOT NULL,
                                CONSTRAINT FK_Dishes_Categories_BIT240199 FOREIGN KEY (DishCategoryId) 
                                    REFERENCES dbo.DishCategories_BIT240199(Id)
                            );
                        END
                        """;
                    command.ExecuteNonQuery();
                }

                // 2.3. Create DishImages table with foreign key reference
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = """
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = N'DishImages_BIT240199' AND schema_id = SCHEMA_ID(N'dbo'))
                        BEGIN
                            CREATE TABLE dbo.DishImages_BIT240199 (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                ImageUrl NVARCHAR(MAX) NOT NULL,
                                IsThumbnail BIT NOT NULL DEFAULT 0,
                                DishId INT NOT NULL,
                                CONSTRAINT FK_Images_Dishes_BIT240199 FOREIGN KEY (DishId) 
                                    REFERENCES dbo.Dishes_BIT240199(Id)
                            );
                        END
                        """;
                    command.ExecuteNonQuery();
                }

                // STEP 3: Seed data tables if empty (Requirement specifications minimum: 3 categories, 5 dishes)
                using (var checkCommand = connection.CreateCommand())
                {
                    checkCommand.CommandText = "SELECT COUNT(1) FROM dbo.DishCategories_BIT240199";
                    int count = (int)checkCommand.ExecuteScalar()!;
                    
                    if (count == 0)
                    {
                        // 3.1. Insert 3 categories and retrieve individual Identity keys
                        int catAppetizerId, catMainId, catDessertId;

                        using (var insCmd = connection.CreateCommand())
                        {
                            insCmd.CommandText = "INSERT INTO dbo.DishCategories_BIT240199 (Name, Description) OUTPUT INSERTED.Id VALUES (N'Appetizer', N'Light starters');";
                            catAppetizerId = (int)insCmd.ExecuteScalar()!;
                        }
                        using (var insCmd = connection.CreateCommand())
                        {
                            insCmd.CommandText = "INSERT INTO dbo.DishCategories_BIT240199 (Name, Description) OUTPUT INSERTED.Id VALUES (N'Main Course', N'Hearty Core Dishes');";
                            catMainId = (int)insCmd.ExecuteScalar()!;
                        }
                        using (var insCmd = connection.CreateCommand())
                        {
                            insCmd.CommandText = "INSERT INTO dbo.DishCategories_BIT240199 (Name, Description) OUTPUT INSERTED.Id VALUES (N'Dessert', N'Sweet treats and fruits');";
                            catDessertId = (int)insCmd.ExecuteScalar()!;
                        }

                        // Insert 5 Active Dishes + 1 Suspended Dish for testing criteria
                        using (var insDishCmd = connection.CreateCommand())
                        {
                            insDishCmd.CommandText = $"""
                                INSERT INTO dbo.Dishes_BIT240199 (Name, Price, PreparationTime, IsAvailable, Description, DishCategoryId) VALUES 
                                (N'Seafood Soup', 45000, 12, 1, N'Hot savory marine starter broth', {catAppetizerId}),
                                (N'Shrimp Lotus Salad', 75000, 10, 1, N'Fresh sweet and sour texture blend', {catAppetizerId}),
                                (N'Yangzhou Fried Rice', 65000, 20, 1, N'Traditional combined protein pan rice', {catMainId}),
                                (N'Shaking Beef with Fries', 150000, 25, 1, N'Tender cubed beef in savory premium glaze', {catMainId}),
                                (N'Caramel Coconut Flan', 25000, 5, 1, N'Smooth custard with aromatic milk base', {catDessertId}),
                                (N'Discontinued Testing Dish', 99000, 15, 0, N'Suspended record exclusively for filter demonstration tasks', {catMainId});
                                """;
                            insDishCmd.ExecuteNonQuery();
                        }

                        // Insert initial fallback images mapping dynamically to newly created dishes
                        using (var insImgCmd = connection.CreateCommand())
                        {
                            insImgCmd.CommandText = """
                                INSERT INTO dbo.DishImages_BIT240199 (ImageUrl, IsThumbnail, DishId)
                                SELECT 'https://images.unsplash.com/photo-1547592180-85f173990554', 1, Id FROM dbo.Dishes_BIT240199;
                                """;
                            insImgCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}