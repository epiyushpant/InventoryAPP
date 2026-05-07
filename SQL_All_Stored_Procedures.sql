-- =====================================================
-- SQL Stored Procedures for All Inventory Tables
-- Execute these in your SQL Server database
-- =====================================================

-- =====================================================
-- INVENTORY PROCEDURES
-- =====================================================

CREATE OR ALTER PROCEDURE [dbo].[spGetInventory]
    @InventoryID INT = NULL
AS
BEGIN
    IF @InventoryID IS NULL
    BEGIN
        SELECT InventoryID, ProductID, QuantityInStock, Location, LastUpdated
        FROM [dbo].[Inventory]
        ORDER BY InventoryID;
    END
    ELSE
    BEGIN
        SELECT InventoryID, ProductID, QuantityInStock, Location, LastUpdated
        FROM [dbo].[Inventory]
        WHERE InventoryID = @InventoryID;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spInsertInventory]
    @ProductID INT = NULL,
    @QuantityInStock INT,
    @Location NVARCHAR(100) = NULL
AS
BEGIN
    INSERT INTO [dbo].[Inventory] (ProductID, QuantityInStock, Location, LastUpdated)
    VALUES (@ProductID, @QuantityInStock, @Location, GETDATE());
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUpdateInventory]
    @InventoryID INT,
    @ProductID INT = NULL,
    @QuantityInStock INT,
    @Location NVARCHAR(100) = NULL
AS
BEGIN
    UPDATE [dbo].[Inventory]
    SET ProductID = @ProductID,
        QuantityInStock = @QuantityInStock,
        Location = @Location,
        LastUpdated = GETDATE()
    WHERE InventoryID = @InventoryID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spDeleteInventory]
    @InventoryID INT
AS
BEGIN
    DELETE FROM [dbo].[Inventory]
    WHERE InventoryID = @InventoryID;
END
GO

-- =====================================================
-- PURCHASE ORDER PROCEDURES
-- =====================================================

CREATE OR ALTER PROCEDURE [dbo].[spGetPurchaseOrders]
    @PurchaseOrderID INT = NULL
AS
BEGIN
    IF @PurchaseOrderID IS NULL
    BEGIN
        SELECT PurchaseOrderID, SupplierID, OrderDate, ExpectedDate, Status
        FROM [dbo].[PurchaseOrders]
        ORDER BY PurchaseOrderID DESC;
    END
    ELSE
    BEGIN
        SELECT PurchaseOrderID, SupplierID, OrderDate, ExpectedDate, Status
        FROM [dbo].[PurchaseOrders]
        WHERE PurchaseOrderID = @PurchaseOrderID;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spInsertPurchaseOrder]
    @SupplierID INT = NULL,
    @OrderDate DATETIME,
    @ExpectedDate DATETIME = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    INSERT INTO [dbo].[PurchaseOrders] (SupplierID, OrderDate, ExpectedDate, Status)
    VALUES (@SupplierID, @OrderDate, @ExpectedDate, @Status);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUpdatePurchaseOrder]
    @PurchaseOrderID INT,
    @SupplierID INT = NULL,
    @OrderDate DATETIME,
    @ExpectedDate DATETIME = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    UPDATE [dbo].[PurchaseOrders]
    SET SupplierID = @SupplierID,
        OrderDate = @OrderDate,
        ExpectedDate = @ExpectedDate,
        Status = @Status
    WHERE PurchaseOrderID = @PurchaseOrderID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spDeletePurchaseOrder]
    @PurchaseOrderID INT
AS
BEGIN
    DELETE FROM [dbo].[PurchaseOrders]
    WHERE PurchaseOrderID = @PurchaseOrderID;
END
GO

-- =====================================================
-- PURCHASE ORDER DETAIL PROCEDURES
-- =====================================================

CREATE OR ALTER PROCEDURE [dbo].[spGetPurchaseOrderDetails]
    @PODetailID INT = NULL
AS
BEGIN
    IF @PODetailID IS NULL
    BEGIN
        SELECT PODetailID, PurchaseOrderID, ProductID, Quantity, UnitCost
        FROM [dbo].[PurchaseOrderDetails]
        ORDER BY PODetailID;
    END
    ELSE
    BEGIN
        SELECT PODetailID, PurchaseOrderID, ProductID, Quantity, UnitCost
        FROM [dbo].[PurchaseOrderDetails]
        WHERE PODetailID = @PODetailID;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spInsertPurchaseOrderDetail]
    @PurchaseOrderID INT = NULL,
    @ProductID INT = NULL,
    @Quantity INT,
    @UnitCost DECIMAL(10,2)
AS
BEGIN
    INSERT INTO [dbo].[PurchaseOrderDetails] (PurchaseOrderID, ProductID, Quantity, UnitCost)
    VALUES (@PurchaseOrderID, @ProductID, @Quantity, @UnitCost);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUpdatePurchaseOrderDetail]
    @PODetailID INT,
    @PurchaseOrderID INT = NULL,
    @ProductID INT = NULL,
    @Quantity INT,
    @UnitCost DECIMAL(10,2)
AS
BEGIN
    UPDATE [dbo].[PurchaseOrderDetails]
    SET PurchaseOrderID = @PurchaseOrderID,
        ProductID = @ProductID,
        Quantity = @Quantity,
        UnitCost = @UnitCost
    WHERE PODetailID = @PODetailID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spDeletePurchaseOrderDetail]
    @PODetailID INT
AS
BEGIN
    DELETE FROM [dbo].[PurchaseOrderDetails]
    WHERE PODetailID = @PODetailID;
END
GO

-- =====================================================
-- SALES PROCEDURES
-- =====================================================

CREATE OR ALTER PROCEDURE [dbo].[spGetSales]
    @SaleID INT = NULL
AS
BEGIN
    IF @SaleID IS NULL
    BEGIN
        SELECT SaleID, SaleDate, CustomerName, TotalAmount, Status
        FROM [dbo].[Sales]
        ORDER BY SaleID DESC;
    END
    ELSE
    BEGIN
        SELECT SaleID, SaleDate, CustomerName, TotalAmount, Status
        FROM [dbo].[Sales]
        WHERE SaleID = @SaleID;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spInsertSale]
    @SaleDate DATETIME,
    @CustomerName NVARCHAR(100) = NULL,
    @TotalAmount DECIMAL(10,2) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    INSERT INTO [dbo].[Sales] (SaleDate, CustomerName, TotalAmount, Status)
    VALUES (@SaleDate, @CustomerName, @TotalAmount, @Status);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUpdateSale]
    @SaleID INT,
    @SaleDate DATETIME,
    @CustomerName NVARCHAR(100) = NULL,
    @TotalAmount DECIMAL(10,2) = NULL,
    @Status NVARCHAR(50) = NULL
AS
BEGIN
    UPDATE [dbo].[Sales]
    SET SaleDate = @SaleDate,
        CustomerName = @CustomerName,
        TotalAmount = @TotalAmount,
        Status = @Status
    WHERE SaleID = @SaleID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spDeleteSale]
    @SaleID INT
AS
BEGIN
    DELETE FROM [dbo].[Sales]
    WHERE SaleID = @SaleID;
END
GO

-- =====================================================
-- SALES DETAIL PROCEDURES
-- =====================================================

CREATE OR ALTER PROCEDURE [dbo].[spGetSaleDetails]
    @SaleDetailID INT = NULL
AS
BEGIN
    IF @SaleDetailID IS NULL
    BEGIN
        SELECT SaleDetailID, SaleID, ProductID, Quantity, UnitPrice
        FROM [dbo].[SalesDetails]
        ORDER BY SaleDetailID;
    END
    ELSE
    BEGIN
        SELECT SaleDetailID, SaleID, ProductID, Quantity, UnitPrice
        FROM [dbo].[SalesDetails]
        WHERE SaleDetailID = @SaleDetailID;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spInsertSaleDetail]
    @SaleID INT = NULL,
    @ProductID INT = NULL,
    @Quantity INT,
    @UnitPrice DECIMAL(10,2)
AS
BEGIN
    INSERT INTO [dbo].[SalesDetails] (SaleID, ProductID, Quantity, UnitPrice)
    VALUES (@SaleID, @ProductID, @Quantity, @UnitPrice);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUpdateSaleDetail]
    @SaleDetailID INT,
    @SaleID INT = NULL,
    @ProductID INT = NULL,
    @Quantity INT,
    @UnitPrice DECIMAL(10,2)
AS
BEGIN
    UPDATE [dbo].[SalesDetails]
    SET SaleID = @SaleID,
        ProductID = @ProductID,
        Quantity = @Quantity,
        UnitPrice = @UnitPrice
    WHERE SaleDetailID = @SaleDetailID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spDeleteSaleDetail]
    @SaleDetailID INT
AS
BEGIN
    DELETE FROM [dbo].[SalesDetails]
    WHERE SaleDetailID = @SaleDetailID;
END
GO

-- =====================================================
-- STOCK MOVEMENT PROCEDURES
-- =====================================================

CREATE OR ALTER PROCEDURE [dbo].[spGetStockMovements]
    @MovementID INT = NULL
AS
BEGIN
    IF @MovementID IS NULL
    BEGIN
        SELECT MovementID, ProductID, MovementType, QuantityChange, MovementDate, Reference
        FROM [dbo].[StockMovements]
        ORDER BY MovementID DESC;
    END
    ELSE
    BEGIN
        SELECT MovementID, ProductID, MovementType, QuantityChange, MovementDate, Reference
        FROM [dbo].[StockMovements]
        WHERE MovementID = @MovementID;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spInsertStockMovement]
    @ProductID INT = NULL,
    @MovementType NVARCHAR(50) = NULL,
    @QuantityChange INT = NULL,
    @MovementDate DATETIME = NULL,
    @Reference NVARCHAR(255) = NULL
AS
BEGIN
    INSERT INTO [dbo].[StockMovements] (ProductID, MovementType, QuantityChange, MovementDate, Reference)
    VALUES (@ProductID, @MovementType, @QuantityChange, @MovementDate, @Reference);
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spUpdateStockMovement]
    @MovementID INT,
    @ProductID INT = NULL,
    @MovementType NVARCHAR(50) = NULL,
    @QuantityChange INT = NULL,
    @MovementDate DATETIME = NULL,
    @Reference NVARCHAR(255) = NULL
AS
BEGIN
    UPDATE [dbo].[StockMovements]
    SET ProductID = @ProductID,
        MovementType = @MovementType,
        QuantityChange = @QuantityChange,
        MovementDate = @MovementDate,
        Reference = @Reference
    WHERE MovementID = @MovementID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[spDeleteStockMovement]
    @MovementID INT
AS
BEGIN
    DELETE FROM [dbo].[StockMovements]
    WHERE MovementID = @MovementID;
END
GO

-- =====================================================
-- Verify all procedures were created
-- =====================================================

SELECT 
    name,
    type,
    SCHEMA_NAME(schema_id) as schema_name
FROM sys.objects
WHERE type = 'P' 
AND name LIKE 'sp%'
ORDER BY name;

