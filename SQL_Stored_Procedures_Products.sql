-- SQL Stored Procedures for Products Table
-- Execute these in your SQL Server database

-- 1. Get All Products
CREATE OR ALTER PROCEDURE dbo.spGetProducts
AS
BEGIN
    SELECT ProductID, ProductName, CategoryID, SupplierID, SKU, Description, UnitPrice, ReorderLevel, IsActive
    FROM dbo.Products
    ORDER BY ProductName
END

-- 2. Get Product By ID
CREATE OR ALTER PROCEDURE dbo.spGetProductById
    @ProductID INT
AS
BEGIN
    SELECT ProductID, ProductName, CategoryID, SupplierID, SKU, Description, UnitPrice, ReorderLevel, IsActive
    FROM dbo.Products
    WHERE ProductID = @ProductID
END

-- 3. Insert Product
CREATE OR ALTER PROCEDURE [dbo].[spInsertProduct]
    @ProductName NVARCHAR(100),
    @CategoryID INT = NULL,
    @SupplierID INT = NULL,
    @SKU NVARCHAR(50) = NULL,
    @Description NVARCHAR(255) = NULL,
    @UnitPrice DECIMAL(10,2),
    @ReorderLevel INT = NULL,
    @IsActive BIT = 1,
    @ProductID INT OUTPUT
AS
BEGIN
    INSERT INTO [dbo].[Products] (ProductName, CategoryID, SupplierID, SKU, Description, UnitPrice, ReorderLevel, IsActive)
    VALUES (@ProductName, @CategoryID, @SupplierID, @SKU, @Description, @UnitPrice, @ReorderLevel, @IsActive)
    
    SET @ProductID = SCOPE_IDENTITY()
END

-- 4. Update Product
CREATE OR ALTER PROCEDURE [dbo].[spUpdateProduct]
    @ProductID INT,
    @ProductName NVARCHAR(100),
    @CategoryID INT = NULL,
    @SupplierID INT = NULL,
    @SKU NVARCHAR(50) = NULL,
    @Description NVARCHAR(255) = NULL,
    @UnitPrice DECIMAL(10,2),
    @ReorderLevel INT = NULL,
    @IsActive BIT = 1
AS
BEGIN
    UPDATE [dbo].[Products]
    SET ProductName = @ProductName,
        CategoryID = @CategoryID,
        SupplierID = @SupplierID,
        SKU = @SKU,
        Description = @Description,
        UnitPrice = @UnitPrice,
        ReorderLevel = @ReorderLevel,
        IsActive = @IsActive
    WHERE ProductID = @ProductID;
END

-- 5. Delete Product
CREATE OR ALTER PROCEDURE [dbo].[spDeleteProduct]
    @ProductID INT
AS
BEGIN
    DELETE FROM [dbo].[Products]
    WHERE ProductID = @ProductID;
END
