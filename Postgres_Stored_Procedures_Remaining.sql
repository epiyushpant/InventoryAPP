-- =====================================================
-- CATEGORY PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetCategories()
RETURNS TABLE (
    "CategoryID" INT, 
    "CategoryName" TEXT, 
    "Description" TEXT
) AS $$
BEGIN
    RETURN QUERY SELECT c."CategoryID", c."CategoryName", c."Description"
    FROM "Categories" c
    ORDER BY c."CategoryName";
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertCategory(
    p_CategoryName TEXT,
    p_Description TEXT DEFAULT NULL
) AS $$
BEGIN
    INSERT INTO "Categories" ("CategoryName", "Description")
    VALUES (p_CategoryName, p_Description);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdateCategory(
    p_CategoryID INT,
    p_CategoryName TEXT,
    p_Description TEXT DEFAULT NULL
) AS $$
BEGIN
    UPDATE "Categories"
    SET "CategoryName" = p_CategoryName,
        "Description" = p_Description
    WHERE "CategoryID" = p_CategoryID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeleteCategory(
    p_CategoryID INT
) AS $$
BEGIN
    DELETE FROM "Categories"
    WHERE "CategoryID" = p_CategoryID;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- PRODUCT PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetProducts(p_ProductID INT DEFAULT NULL)
RETURNS TABLE (
    "ProductID" INT, 
    "ProductName" TEXT, 
    "CategoryID" INT, 
    "SupplierID" INT, 
    "SKU" TEXT, 
    "Description" TEXT, 
    "UnitPrice" DECIMAL(10,2), 
    "ReorderLevel" INT, 
    "IsActive" BOOLEAN
) AS $$
BEGIN
    IF p_ProductID IS NULL THEN
        RETURN QUERY SELECT p."ProductID", p."ProductName", p."CategoryID", p."SupplierID", p."SKU", p."Description", p."UnitPrice", p."ReorderLevel", p."IsActive"
        FROM "Products" p
        ORDER BY p."ProductName";
    ELSE
        RETURN QUERY SELECT p."ProductID", p."ProductName", p."CategoryID", p."SupplierID", p."SKU", p."Description", p."UnitPrice", p."ReorderLevel", p."IsActive"
        FROM "Products" p
        WHERE p."ProductID" = p_ProductID;
    END IF;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertProduct(
    p_ProductName TEXT,
    p_CategoryID INT DEFAULT NULL,
    p_SupplierID INT DEFAULT NULL,
    p_SKU TEXT DEFAULT NULL,
    p_Description TEXT DEFAULT NULL,
    p_UnitPrice DECIMAL(10,2) DEFAULT NULL,
    p_ReorderLevel INT DEFAULT NULL,
    p_IsActive BOOLEAN DEFAULT TRUE
) AS $$
BEGIN
    INSERT INTO "Products" ("ProductName", "CategoryID", "SupplierID", "SKU", "Description", "UnitPrice", "ReorderLevel", "IsActive")
    VALUES (p_ProductName, p_CategoryID, p_SupplierID, p_SKU, p_Description, p_UnitPrice, p_ReorderLevel, p_IsActive);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdateProduct(
    p_ProductID INT,
    p_ProductName TEXT,
    p_CategoryID INT DEFAULT NULL,
    p_SupplierID INT DEFAULT NULL,
    p_SKU TEXT DEFAULT NULL,
    p_Description TEXT DEFAULT NULL,
    p_UnitPrice DECIMAL(10,2),
    p_ReorderLevel INT DEFAULT NULL,
    p_IsActive BOOLEAN DEFAULT TRUE
) AS $$
BEGIN
    UPDATE "Products"
    SET "ProductName" = p_ProductName,
        "CategoryID" = p_CategoryID,
        "SupplierID" = p_SupplierID,
        "SKU" = p_SKU,
        "Description" = p_Description,
        "UnitPrice" = p_UnitPrice,
        "ReorderLevel" = p_ReorderLevel,
        "IsActive" = p_IsActive
    WHERE "ProductID" = p_ProductID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeleteProduct(
    p_ProductID INT
) AS $$
BEGIN
    DELETE FROM "Products"
    WHERE "ProductID" = p_ProductID;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- SUPPLIER PROCEDURES
-- =====================================================

CREATE OR REPLACE FUNCTION spGetSuppliers()
RETURNS TABLE (
    "SupplierID" INT, 
    "SupplierName" TEXT, 
    "ContactPerson" TEXT, 
    "Email" TEXT, 
    "Phone" TEXT, 
    "Address" TEXT, 
    "City" TEXT, 
    "Country" TEXT
) AS $$
BEGIN
    RETURN QUERY SELECT s."SupplierID", s."SupplierName", s."ContactPerson", s."Email", s."Phone", s."Address", s."City", s."Country"
    FROM "Suppliers" s
    ORDER BY s."SupplierName";
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION spGetSupplierById(p_SupplierID INT)
RETURNS TABLE (
    "SupplierID" INT, 
    "SupplierName" TEXT, 
    "ContactPerson" TEXT, 
    "Email" TEXT, 
    "Phone" TEXT, 
    "Address" TEXT, 
    "City" TEXT, 
    "Country" TEXT
) AS $$
BEGIN
    RETURN QUERY SELECT s."SupplierID", s."SupplierName", s."ContactPerson", s."Email", s."Phone", s."Address", s."City", s."Country"
    FROM "Suppliers" s
    WHERE s."SupplierID" = p_SupplierID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spInsertSupplier(
    p_SupplierName TEXT,
    p_ContactPerson TEXT DEFAULT NULL,
    p_Email TEXT DEFAULT NULL,
    p_Phone TEXT DEFAULT NULL,
    p_Address TEXT DEFAULT NULL,
    p_City TEXT DEFAULT NULL,
    p_Country TEXT DEFAULT NULL
) AS $$
BEGIN
    INSERT INTO "Suppliers" ("SupplierName", "ContactPerson", "Email", "Phone", "Address", "City", "Country")
    VALUES (p_SupplierName, p_ContactPerson, p_Email, p_Phone, p_Address, p_City, p_Country);
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spUpdateSupplier(
    p_SupplierID INT,
    p_SupplierName TEXT,
    p_ContactPerson TEXT DEFAULT NULL,
    p_Email TEXT DEFAULT NULL,
    p_Phone TEXT DEFAULT NULL,
    p_Address TEXT DEFAULT NULL,
    p_City TEXT DEFAULT NULL,
    p_Country TEXT DEFAULT NULL
) AS $$
BEGIN
    UPDATE "Suppliers"
    SET "SupplierName" = p_SupplierName,
        "ContactPerson" = p_ContactPerson,
        "Email" = p_Email,
        "Phone" = p_Phone,
        "Address" = p_Address,
        "City" = p_City,
        "Country" = p_Country
    WHERE "SupplierID" = p_SupplierID;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE PROCEDURE spDeleteSupplier(
    p_SupplierID INT
) AS $$
BEGIN
    DELETE FROM "Suppliers"
    WHERE "SupplierID" = p_SupplierID;
END;
$$ LANGUAGE plpgsql;
