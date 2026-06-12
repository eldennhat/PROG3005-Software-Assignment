CREATE TABLE dbo.Book (
    Id INT IDENTITY(1,1) PRIMARY KEY, -- IDENTITY(1,1) để ID tự động tăng, khớp với OUTPUT INSERTED.Id trong code
    Name NVARCHAR(255) NOT NULL,      -- NVARCHAR để lưu được tiếng Việt có dấu
    Price DECIMAL(18, 2) NOT NULL     -- DECIMAL phù hợp để lưu giá tiền
);
GO

INSERT INTO dbo.Book (Name, Price)
VALUES 
(N'Lập trình C# cơ bản', 150000),
(N'ASP.NET Core MVC thực chiến', 250000),
(N'Cấu trúc dữ liệu và giải thuật', 120000);
GO

SELECT * FROM dbo.Book;