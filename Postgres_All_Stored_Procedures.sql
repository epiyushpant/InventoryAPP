-- =====================================================
-- PostgreSQL Functions/Procedures for All Inventory Tables
-- =====================================================

-- =====================================================
-- INVENTORY PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetInventory(p_InventoryID INT DEFAULT NULL)
RETURNS TABLE (
    "InventoryID" INT, 
    "ProductID" INT, 
    "QuantityInStock" INT, 
    "Location" TEXT, 
    "LastUpdated" TIMESTAMP
) AS $$
BEGIN
    IF p_InventoryID IS NULL THEN
        RETURN QUERY SELECT i."InventoryID", i."ProductID", i."QuantityInStock", i."Location", i."LastUpdated"
        FROM "Inventory" i
        ORDER BY i."InventoryID";
    ELSE
        RETURN QUERY SELECT i."InventoryID", i."ProductID", i."QuantityInStock", i."Location", i."LastUpdated"
        FROM "Inventory" i
        WHERE i."InventoryID" = p_InventoryID;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertInventory(
    p_ProductID INT,
    p_QuantityInStock INT,
    p_Location TEXT DEFAULT NULL
) AS $$
BEGIN
    INSERT INTO "Inventory" ("ProductID", "QuantityInStock", "Location", "LastUpdated")
    VALUES (p_ProductID, p_QuantityInStock, p_Location, CURRENT_TIMESTAMP);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdateInventory(
    p_InventoryID INT,
    p_ProductID INT,
    p_QuantityInStock INT,
    p_Location TEXT DEFAULT NULL
) AS $$
BEGIN
    UPDATE "Inventory"
    SET "ProductID" = p_ProductID,
        "QuantityInStock" = p_QuantityInStock,
        "Location" = p_Location,
        "LastUpdated" = CURRENT_TIMESTAMP
    WHERE "InventoryID" = p_InventoryID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeleteInventory(
    p_InventoryID INT
) AS $$
BEGIN
    DELETE FROM "Inventory"
    WHERE "InventoryID" = p_InventoryID;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- PURCHASE ORDER PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetPurchaseOrders(p_PurchaseOrderID INT DEFAULT NULL)
RETURNS TABLE (
    "PurchaseOrderID" INT, 
    "SupplierID" INT, 
    "OrderDate" TIMESTAMP, 
    "ExpectedDate" TIMESTAMP, 
    "Status" TEXT
) AS $$
BEGIN
    IF p_PurchaseOrderID IS NULL THEN
        RETURN QUERY SELECT po."PurchaseOrderID", po."SupplierID", po."OrderDate", po."ExpectedDate", po."Status"
        FROM "PurchaseOrders" po
        ORDER BY po."PurchaseOrderID" DESC;
    ELSE
        RETURN QUERY SELECT po."PurchaseOrderID", po."SupplierID", po."OrderDate", po."ExpectedDate", po."Status"
        FROM "PurchaseOrders" po
        WHERE po."PurchaseOrderID" = p_PurchaseOrderID;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertPurchaseOrder(
    p_SupplierID INT,
    p_OrderDate TIMESTAMP,
    p_ExpectedDate TIMESTAMP DEFAULT NULL,
    p_Status TEXT DEFAULT NULL
) AS $$
BEGIN
    INSERT INTO "PurchaseOrders" ("SupplierID", "OrderDate", "ExpectedDate", "Status")
    VALUES (p_SupplierID, p_OrderDate, p_ExpectedDate, p_Status);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdatePurchaseOrder(
    p_PurchaseOrderID INT,
    p_SupplierID INT,
    p_OrderDate TIMESTAMP,
    p_ExpectedDate TIMESTAMP DEFAULT NULL,
    p_Status TEXT DEFAULT NULL
) AS $$
BEGIN
    UPDATE "PurchaseOrders"
    SET "SupplierID" = p_SupplierID,
        "OrderDate" = p_OrderDate,
        "ExpectedDate" = p_ExpectedDate,
        "Status" = p_Status
    WHERE "PurchaseOrderID" = p_PurchaseOrderID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeletePurchaseOrder(
    p_PurchaseOrderID INT
) AS $$
BEGIN
    DELETE FROM "PurchaseOrders"
    WHERE "PurchaseOrderID" = p_PurchaseOrderID;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- PURCHASE ORDER DETAIL PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetPurchaseOrderDetails(p_PODetailID INT DEFAULT NULL)
RETURNS TABLE (
    "PODetailID" INT, 
    "PurchaseOrderID" INT, 
    "ProductID" INT, 
    "Quantity" INT, 
    "UnitCost" DECIMAL(10,2)
) AS $$
BEGIN
    IF p_PODetailID IS NULL THEN
        RETURN QUERY SELECT pod."PODetailID", pod."PurchaseOrderID", pod."ProductID", pod."Quantity", pod."UnitCost"
        FROM "PurchaseOrderDetails" pod
        ORDER BY pod."PODetailID";
    ELSE
        RETURN QUERY SELECT pod."PODetailID", pod."PurchaseOrderID", pod."ProductID", pod."Quantity", pod."UnitCost"
        FROM "PurchaseOrderDetails" pod
        WHERE pod."PODetailID" = p_PODetailID;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertPurchaseOrderDetail(
    p_PurchaseOrderID INT,
    p_ProductID INT,
    p_Quantity INT,
    p_UnitCost DECIMAL(10,2)
) AS $$
BEGIN
    INSERT INTO "PurchaseOrderDetails" ("PurchaseOrderID", "ProductID", "Quantity", "UnitCost")
    VALUES (p_PurchaseOrderID, p_ProductID, p_Quantity, p_UnitCost);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdatePurchaseOrderDetail(
    p_PODetailID INT,
    p_PurchaseOrderID INT,
    p_ProductID INT,
    p_Quantity INT,
    p_UnitCost DECIMAL(10,2)
) AS $$
BEGIN
    UPDATE "PurchaseOrderDetails"
    SET "PurchaseOrderID" = p_PurchaseOrderID,
        "ProductID" = p_ProductID,
        "Quantity" = p_Quantity,
        "UnitCost" = p_UnitCost
    WHERE "PODetailID" = p_PODetailID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeletePurchaseOrderDetail(
    p_PODetailID INT
) AS $$
BEGIN
    DELETE FROM "PurchaseOrderDetails"
    WHERE "PODetailID" = p_PODetailID;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- SALES PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetSales(p_SaleID INT DEFAULT NULL)
RETURNS TABLE (
    "SaleID" INT, 
    "SaleDate" TIMESTAMP, 
    "CustomerName" TEXT, 
    "TotalAmount" DECIMAL(10,2), 
    "Status" TEXT
) AS $$
BEGIN
    IF p_SaleID IS NULL THEN
        RETURN QUERY SELECT s."SaleID", s."SaleDate", s."CustomerName", s."TotalAmount", s."Status"
        FROM "Sales" s
        ORDER BY s."SaleID" DESC;
    ELSE
        RETURN QUERY SELECT s."SaleID", s."SaleDate", s."CustomerName", s."TotalAmount", s."Status"
        FROM "Sales" s
        WHERE s."SaleID" = p_SaleID;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertSale(
    p_SaleDate TIMESTAMP,
    p_CustomerName TEXT DEFAULT NULL,
    p_TotalAmount DECIMAL(10,2) DEFAULT NULL,
    p_Status TEXT DEFAULT NULL
) AS $$
BEGIN
    INSERT INTO "Sales" ("SaleDate", "CustomerName", "TotalAmount", "Status")
    VALUES (p_SaleDate, p_CustomerName, p_TotalAmount, p_Status);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdateSale(
    p_SaleID INT,
    p_SaleDate TIMESTAMP,
    p_CustomerName TEXT DEFAULT NULL,
    p_TotalAmount DECIMAL(10,2) DEFAULT NULL,
    p_Status TEXT DEFAULT NULL
) AS $$
BEGIN
    UPDATE "Sales"
    SET "SaleDate" = p_SaleDate,
        "CustomerName" = p_CustomerName,
        "TotalAmount" = p_TotalAmount,
        "Status" = p_Status
    WHERE "SaleID" = p_SaleID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeleteSale(
    p_SaleID INT
) AS $$
BEGIN
    DELETE FROM "Sales"
    WHERE "SaleID" = p_SaleID;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- SALES DETAIL PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetSaleDetails(p_SaleDetailID INT DEFAULT NULL)
RETURNS TABLE (
    "SaleDetailID" INT, 
    "SaleID" INT, 
    "ProductID" INT, 
    "Quantity" INT, 
    "UnitPrice" DECIMAL(10,2)
) AS $$
BEGIN
    IF p_SaleDetailID IS NULL THEN
        RETURN QUERY SELECT sd."SaleDetailID", sd."SaleID", sd."ProductID", sd."Quantity", sd."UnitPrice"
        FROM "SalesDetails" sd
        ORDER BY sd."SaleDetailID";
    ELSE
        RETURN QUERY SELECT sd."SaleDetailID", sd."SaleID", sd."ProductID", sd."Quantity", sd."UnitPrice"
        FROM "SalesDetails" sd
        WHERE sd."SaleDetailID" = p_SaleDetailID;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertSaleDetail(
    p_SaleID INT,
    p_ProductID INT,
    p_Quantity INT,
    p_UnitPrice DECIMAL(10,2)
) AS $$
BEGIN
    INSERT INTO "SalesDetails" ("SaleID", "ProductID", "Quantity", "UnitPrice")
    VALUES (p_SaleID, p_ProductID, p_Quantity, p_UnitPrice);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdateSaleDetail(
    p_SaleDetailID INT,
    p_SaleID INT,
    p_ProductID INT,
    p_Quantity INT,
    p_UnitPrice DECIMAL(10,2)
) AS $$
BEGIN
    UPDATE "SalesDetails"
    SET "SaleID" = p_SaleID,
        "ProductID" = p_ProductID,
        "Quantity" = p_Quantity,
        "UnitPrice" = p_UnitPrice
    WHERE "SaleDetailID" = p_SaleDetailID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeleteSaleDetail(
    p_SaleDetailID INT
) AS $$
BEGIN
    DELETE FROM "SalesDetails"
    WHERE "SaleDetailID" = p_SaleDetailID;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- STOCK MOVEMENT PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetStockMovements(p_MovementID INT DEFAULT NULL)
RETURNS TABLE (
    "MovementID" INT, 
    "ProductID" INT, 
    "MovementType" TEXT, 
    "QuantityChange" INT, 
    "MovementDate" TIMESTAMP, 
    "Reference" TEXT
) AS $$
BEGIN
    IF p_MovementID IS NULL THEN
        RETURN QUERY SELECT sm."MovementID", sm."ProductID", sm."MovementType", sm."QuantityChange", sm."MovementDate", sm."Reference"
        FROM "StockMovements" sm
        ORDER BY sm."MovementID" DESC;
    ELSE
        RETURN QUERY SELECT sm."MovementID", sm."ProductID", sm."MovementType", sm."QuantityChange", sm."MovementDate", sm."Reference"
        FROM "StockMovements" sm
        WHERE sm."MovementID" = p_MovementID;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertStockMovement(
    p_ProductID INT,
    p_MovementType TEXT,
    p_QuantityChange INT,
    p_MovementDate TIMESTAMP,
    p_Reference TEXT DEFAULT NULL
) AS $$
BEGIN
    INSERT INTO "StockMovements" ("ProductID", "MovementType", "QuantityChange", "MovementDate", "Reference")
    VALUES (p_ProductID, p_MovementType, p_QuantityChange, p_MovementDate, p_Reference);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdateStockMovement(
    p_MovementID INT,
    p_ProductID INT,
    p_MovementType TEXT,
    p_QuantityChange INT,
    p_MovementDate TIMESTAMP,
    p_Reference TEXT DEFAULT NULL
) AS $$
BEGIN
    UPDATE "StockMovements"
    SET "ProductID" = p_ProductID,
        "MovementType" = p_MovementType,
        "QuantityChange" = p_QuantityChange,
        "MovementDate" = p_MovementDate,
        "Reference" = p_Reference
    WHERE "MovementID" = p_MovementID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeleteStockMovement(
    p_MovementID INT
) AS $$
BEGIN
    DELETE FROM "StockMovements"
    WHERE "MovementID" = p_MovementID;
END;
$$ LANGUAGE plpgsql;
