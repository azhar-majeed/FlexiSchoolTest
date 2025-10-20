-- =============================================
-- Flexischools Database Creation Script
-- SQL Server Database Schema
-- =============================================

-- Create Database
USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'Flexischools')
BEGIN
    ALTER DATABASE Flexischools SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE Flexischools;
END
GO

CREATE DATABASE Flexischools;
GO

USE Flexischools;
GO

-- =============================================
-- Create Tables
-- =============================================

-- Parents Table
CREATE TABLE Parents (
    Id INT IDENTITY(1,1) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    WalletBalance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    CONSTRAINT PK_Parents PRIMARY KEY (Id),
    CONSTRAINT CK_Parents_WalletBalance CHECK (WalletBalance >= 0),
    CONSTRAINT UQ_Parents_Email UNIQUE (Email)
);
GO

-- Canteens Table
CREATE TABLE Canteens (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    OpeningDays NVARCHAR(200) NULL,
    OrderCutOffTime NVARCHAR(5) NULL,
    CONSTRAINT PK_Canteens PRIMARY KEY (Id)
);
GO

-- Students Table
CREATE TABLE Students (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    ParentId INT NOT NULL,
	Allergens NVARCHAR(100)  NULL,
    CONSTRAINT PK_Students PRIMARY KEY (Id),
    CONSTRAINT FK_Students_Parents FOREIGN KEY (ParentId) REFERENCES Parents(Id) ON DELETE CASCADE
);
GO

-- MenuItems Table
CREATE TABLE MenuItems (
    Id INT IDENTITY(1,1) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(18,2) NOT NULL,
    DailyStockCount INT NULL,
    AllergenTags NVARCHAR(200) NULL,
    CanteenId INT NOT NULL,
    CONSTRAINT PK_MenuItems PRIMARY KEY (Id),
    CONSTRAINT FK_MenuItems_Canteens FOREIGN KEY (CanteenId) REFERENCES Canteens(Id) ON DELETE CASCADE,
    CONSTRAINT CK_MenuItems_Price CHECK (Price >= 0),
    CONSTRAINT CK_MenuItems_DailyStockCount CHECK (DailyStockCount IS NULL OR DailyStockCount >= 0)
);
GO

-- Orders Table
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) NOT NULL,
    ParentId INT NOT NULL,
    StudentId INT NOT NULL,
    CanteenId INT NOT NULL,
    FulfilmentDate DATETIME2 NOT NULL,
    Status INT NOT NULL DEFAULT 1, -- 1=Placed, 2=Fulfilled, 3=Cancelled
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
	IdempotencyKey  NVARCHAR(100)  NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (Id),
    CONSTRAINT FK_Orders_Parents FOREIGN KEY (ParentId) REFERENCES Parents(Id),
    CONSTRAINT FK_Orders_Students FOREIGN KEY (StudentId) REFERENCES Students(Id),
    CONSTRAINT FK_Orders_Canteens FOREIGN KEY (CanteenId) REFERENCES Canteens(Id),
    CONSTRAINT CK_Orders_Status CHECK (Status IN (1, 2, 3))
);
GO

-- OrderItems Table
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) NOT NULL,
    OrderId INT NOT NULL,
    MenuItemId INT NOT NULL,
    Quantity INT NOT NULL,
    CONSTRAINT PK_OrderItems PRIMARY KEY (Id),
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_MenuItems FOREIGN KEY (MenuItemId) REFERENCES MenuItems(Id),
    CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity >= 1)
);
GO

-- =============================================
-- Insert Sample Data
-- =============================================

-- Insert Sample Parents
INSERT INTO Parents (Email, Name, WalletBalance) VALUES
('fareed.kamran@email.com', 'Fareed Kamran', 50.00),
('azhar.majeed@email.com', 'Azhar Majeed', 25.50),
('shane.warne@email.com', 'Shane Warne', 100.00);
GO

-- Insert Sample Canteens
INSERT INTO Canteens (Name, OpeningDays, OrderCutOffTime) VALUES
('Primary School Canteen', 'Monday,Tuesday,Wednesday,Thursday,Friday', '09:30'),
('High School Canteen', 'Monday,Tuesday,Wednesday,Thursday,Friday', '08:45'),
('Sports Canteen', 'Monday,Wednesday,Friday', '10:00'),
('Hostel Canteen', 'Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday', '11:00')
;
GO

-- Insert Sample Students
INSERT INTO Students (Name, ParentId) VALUES
('Alina Zeb', 1),
('Irfan Javed', 1),
('Stephen Jones', 2),
('Ricky Ponting', 3);
GO

-- Insert Sample Menu Items
INSERT INTO MenuItems (Name, Description, Price, DailyStockCount, AllergenTags, CanteenId) VALUES
('Chicken Sandwich', 'Fresh chicken breast with lettuce and mayo', 6.50, 20, 'gluten,dairy', 1),
('Veggie Wrap', 'Mixed vegetables in a tortilla wrap', 5.00, 15, 'gluten', 1),
('Fruit Salad', 'Seasonal fresh fruits', 4.00, 25, NULL, 1),
('Pizza Slice', 'Margherita pizza slice', 4.50, 30, 'gluten,dairy', 2),
('Caesar Salad', 'Romaine lettuce with caesar dressing', 7.00, 12, 'dairy,eggs', 2),
('Energy Bar', 'Granola and nuts energy bar', 3.50, 40, 'nuts', 3),
('Smoothie', 'Mixed berry smoothie', 5.50, 20, 'dairy', 3);
GO

-- Insert Sample Orders
INSERT INTO Orders (ParentId, StudentId, CanteenId, FulfilmentDate, Status) VALUES
(1, 1, 1, '2024-01-15', 1), -- Placed
(1, 2, 1, '2024-01-15', 2), -- Fulfilled
(2, 3, 2, '2024-01-16', 1); -- Placed
GO

-- Insert Sample Order Items
INSERT INTO OrderItems (OrderId, MenuItemId, Quantity) VALUES
(1, 1, 1), -- Alina's chicken sandwich
(1, 3, 1), -- Alina's fruit salad
(2, 2, 1), -- Irfan's veggie wrap
(2, 3, 1), -- Stephen's fruit salad
(3, 4, 2), -- Ricky's pizza slices
(3, 5, 1); -- Ricky's caesar salad
GO

-- =============================================
-- Create Stored Procedures
-- =============================================

-- Procedure to get orders by parent
CREATE PROCEDURE sp_GetOrdersByParent
    @ParentId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        o.Id,
        o.FulfilmentDate,
        o.Status,
        o.CreatedAt,
        s.Name AS StudentName,
        c.Name AS CanteenName,
        SUM(oi.Quantity * mi.Price) AS TotalAmount
    FROM Orders o
    INNER JOIN Students s ON o.StudentId = s.Id
    INNER JOIN Canteens c ON o.CanteenId = c.Id
    LEFT JOIN OrderItems oi ON o.Id = oi.OrderId
    LEFT JOIN MenuItems mi ON oi.MenuItemId = mi.Id
    WHERE o.ParentId = @ParentId
    GROUP BY o.Id, o.FulfilmentDate, o.Status, o.CreatedAt, s.Name, c.Name
    ORDER BY o.CreatedAt DESC;
END
GO

-- Procedure to update order status
CREATE PROCEDURE sp_UpdateOrderStatus
    @OrderId INT,
    @Status INT,
    @UpdatedAt DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @UpdatedAt IS NULL
        SET @UpdatedAt = GETUTCDATE();
    
    UPDATE Orders 
    SET Status = @Status, UpdatedAt = @UpdatedAt
    WHERE Id = @OrderId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- Create Functions
-- =============================================

-- Function to check if canteen is open on a specific day
CREATE FUNCTION fn_IsCanteenOpenOnDay(@CanteenId INT, @DayOfWeek NVARCHAR(10))
RETURNS BIT
AS
BEGIN
    DECLARE @OpeningDays NVARCHAR(200);
    DECLARE @IsOpen BIT = 0;
    
    SELECT @OpeningDays = OpeningDays 
    FROM Canteens 
    WHERE Id = @CanteenId;
    
    IF @OpeningDays IS NOT NULL AND CHARINDEX(@DayOfWeek, @OpeningDays) > 0
        SET @IsOpen = 1;
    
    RETURN @IsOpen;
END
GO


PRINT 'Flexischools database created successfully!';
PRINT 'Database includes:';
PRINT '- 6 main tables (Parents, Students, Canteens, MenuItems, Orders, OrderItems)';
PRINT '- Proper foreign key relationships and constraints';
PRINT '- Performance indexes';
PRINT '- Useful views for common queries';
PRINT '- Sample data for testing';
PRINT '- Stored procedures and functions';
PRINT '- Triggers for automatic timestamp updates';
GO
