-- SQL Stored Procedures for Suppliers Table
-- Execute these in your SQL Server database

-- 1. Create Suppliers Table (if not already created)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Suppliers')
BEGIN
    CREATE TABLE dbo.Suppliers (
        SupplierID INT PRIMARY KEY IDENTITY(1,1),
        SupplierName NVARCHAR(MAX) NOT NULL,
        ContactPerson NVARCHAR(MAX) NULL,
        Email NVARCHAR(MAX) NULL,
        Phone NVARCHAR(MAX) NULL,
        Address NVARCHAR(MAX) NULL,
        City NVARCHAR(MAX) NULL,
        Country NVARCHAR(MAX) NULL
    )
END

-- 2. Get All Suppliers
CREATE OR ALTER PROCEDURE dbo.spGetSuppliers
AS
BEGIN
    SELECT SupplierID, SupplierName, ContactPerson, Email, Phone, Address, City, Country
    FROM dbo.Suppliers
    ORDER BY SupplierName
END

-- 3. Get Supplier By ID
CREATE OR ALTER PROCEDURE dbo.spGetSupplierById
    @SupplierID INT
AS
BEGIN
    SELECT SupplierID, SupplierName, ContactPerson, Email, Phone, Address, City, Country
    FROM dbo.Suppliers
    WHERE SupplierID = @SupplierID
END

-- 4. Insert Supplier
CREATE OR ALTER PROCEDURE dbo.spInsertSupplier
    @SupplierName NVARCHAR(MAX),
    @ContactPerson NVARCHAR(MAX) = NULL,
    @Email NVARCHAR(MAX) = NULL,
    @Phone NVARCHAR(MAX) = NULL,
    @Address NVARCHAR(MAX) = NULL,
    @City NVARCHAR(MAX) = NULL,
    @Country NVARCHAR(MAX) = NULL,
    @SupplierID INT OUTPUT
AS
BEGIN
    INSERT INTO dbo.Suppliers (SupplierName, ContactPerson, Email, Phone, Address, City, Country)
    VALUES (@SupplierName, @ContactPerson, @Email, @Phone, @Address, @City, @Country)
    
    SET @SupplierID = SCOPE_IDENTITY()
END

-- 5. Update Supplier
CREATE OR ALTER PROCEDURE dbo.spUpdateSupplier
    @SupplierID INT,
    @SupplierName NVARCHAR(MAX),
    @ContactPerson NVARCHAR(MAX) = NULL,
    @Email NVARCHAR(MAX) = NULL,
    @Phone NVARCHAR(MAX) = NULL,
    @Address NVARCHAR(MAX) = NULL,
    @City NVARCHAR(MAX) = NULL,
    @Country NVARCHAR(MAX) = NULL
AS
BEGIN
    UPDATE dbo.Suppliers
    SET SupplierName = @SupplierName,
        ContactPerson = @ContactPerson,
        Email = @Email,
        Phone = @Phone,
        Address = @Address,
        City = @City,
        Country = @Country
    WHERE SupplierID = @SupplierID
END

-- 6. Delete Supplier
CREATE OR ALTER PROCEDURE dbo.spDeleteSupplier
    @SupplierID INT
AS
BEGIN
    DELETE FROM dbo.Suppliers
    WHERE SupplierID = @SupplierID
END

