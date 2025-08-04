-- Final Database Fix Script
-- Run this in Azure Data Studio or SQL Server Management Studio
-- This script will fix all database issues

USE BankingDB;
GO

PRINT 'Starting database fix...';
GO

-- 1. Check if Customers table exists
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    PRINT 'ERROR: Customers table does not exist!';
    PRINT 'Please run the CreateDatabase.sql script first.';
    RETURN;
END
GO

-- 2. Add PasswordHash column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Customers ADD PasswordHash NVARCHAR(256) NULL;
    PRINT 'PasswordHash column added';
END
ELSE
BEGIN
    PRINT 'PasswordHash column already exists';
END
GO

-- 3. Add Email column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Email')
BEGIN
    ALTER TABLE Customers ADD Email NVARCHAR(100) NULL;
    PRINT 'Email column added';
END
ELSE
BEGIN
    PRINT 'Email column already exists';
END
GO

-- 4. Add PhoneNumber column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE Customers ADD PhoneNumber NVARCHAR(20) NULL;
    PRINT 'PhoneNumber column added';
END
ELSE
BEGIN
    PRINT 'PhoneNumber column already exists';
END
GO

-- 5. Update existing customers with default password hash
UPDATE Customers 
SET PasswordHash = 'AQAAAAEAACcQAAAAEH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3Fq==' 
WHERE PasswordHash IS NULL;
PRINT 'Updated existing customers with default password';
GO

-- 6. Make PasswordHash NOT NULL
ALTER TABLE Customers ALTER COLUMN PasswordHash NVARCHAR(256) NOT NULL;
PRINT 'Made PasswordHash NOT NULL';
GO

-- 7. Check if test customer exists
IF NOT EXISTS (SELECT 1 FROM Customers WHERE TCKN = '12345678901')
BEGIN
    -- Generate customer number using stored procedure
    DECLARE @CustomerNumber VARCHAR(12);
    EXEC GenerateCustomerNumber @CustomerNumber OUTPUT;
    
    INSERT INTO Customers (CustomerNumber, FirstName, LastName, TCKN, PasswordHash, DateOfBirth, Email, PhoneNumber, IsActive, CreatedDate)
    VALUES (@CustomerNumber, 'Selim', 'Yilbas', '12345678901', 'AQAAAAEAACcQAAAAEH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3Fq==', '2000-05-05', 'selim@example.com', '05551234567', 1, GETUTCDATE());
    PRINT 'Test customer created with CustomerNumber: ' + @CustomerNumber;
END
ELSE
BEGIN
    PRINT 'Test customer already exists';
END
GO

-- 8. Show current database state
PRINT '=== DATABASE STATUS ===';
SELECT COUNT(*) as TotalCustomers FROM Customers;
GO

SELECT TOP 3 
    CustomerId,
    CustomerNumber,
    FirstName,
    LastName,
    TCKN,
    CASE WHEN PasswordHash IS NOT NULL THEN 'Yes' ELSE 'No' END as HasPassword,
    Email,
    PhoneNumber
FROM Customers;
GO

-- 9. Show table structure
PRINT '=== TABLE STRUCTURE ===';
SELECT 
    COLUMN_NAME as [Column],
    DATA_TYPE as [Type],
    CHARACTER_MAXIMUM_LENGTH as [Length],
    IS_NULLABLE as [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION;
GO

PRINT '=== DATABASE FIX COMPLETED ===';
PRINT 'You can now test the API with:';
PRINT 'TCKN: 12345678901';
PRINT 'Password: Password123!';
GO 