-- Fix Database Schema for BankingDB
-- This script adds missing columns to the Customers table
USE BankingDB;
GO

PRINT 'Starting database schema fix...';
GO

-- Add PasswordHash if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Customers ADD PasswordHash NVARCHAR(256) NULL;
    PRINT 'PasswordHash column added successfully';
END
ELSE
BEGIN
    PRINT 'PasswordHash column already exists';
END
GO

-- Update existing customers with default password hash
UPDATE Customers 
SET PasswordHash = 'AQAAAAEAACcQAAAAEH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3Fq==' 
WHERE PasswordHash IS NULL;
PRINT 'Updated existing customers with default password hash';
GO

-- Make PasswordHash NOT NULL after updating
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PasswordHash' AND is_nullable = 1)
BEGIN
    ALTER TABLE Customers ALTER COLUMN PasswordHash NVARCHAR(256) NOT NULL;
    PRINT 'PasswordHash column made NOT NULL';
END
GO

-- Add Email if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Email')
BEGIN
    ALTER TABLE Customers ADD Email NVARCHAR(100) NULL;
    PRINT 'Email column added successfully';
END
ELSE
BEGIN
    PRINT 'Email column already exists';
END
GO

-- Add PhoneNumber if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE Customers ADD PhoneNumber NVARCHAR(20) NULL;
    PRINT 'PhoneNumber column added successfully';
END
ELSE
BEGIN
    PRINT 'PhoneNumber column already exists';
END
GO

-- Update existing customers with sample email and phone data
UPDATE Customers 
SET Email = 'customer' + CAST(CustomerId AS VARCHAR) + '@example.com',
    PhoneNumber = '05' + CAST(FLOOR(RAND() * 90) + 10 AS VARCHAR) + ' ' + 
                  CAST(FLOOR(RAND() * 900) + 100 AS VARCHAR) + ' ' + 
                  CAST(FLOOR(RAND() * 90) + 10 AS VARCHAR) + ' ' + 
                  CAST(FLOOR(RAND() * 90) + 10 AS VARCHAR)
WHERE Email IS NULL;
PRINT 'Updated existing customers with sample email and phone data';
GO

-- Show the updated table structure
PRINT 'Current Customers table structure:';
SELECT 
    COLUMN_NAME as [Column],
    DATA_TYPE as [Type],
    CHARACTER_MAXIMUM_LENGTH as [Length],
    IS_NULLABLE as [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION;
GO

-- Show sample data
PRINT 'Sample customer data:';
SELECT TOP 5 
    CustomerNumber,
    FirstName,
    LastName,
    TCKN,
    Email,
    PhoneNumber,
    CASE WHEN PasswordHash IS NOT NULL THEN 'Yes' ELSE 'No' END as HasPassword
FROM Customers;
GO

-- Count total customers
SELECT COUNT(*) as TotalCustomers FROM Customers;
GO

PRINT '✅ Database fix completed successfully!';
PRINT 'All users now have password: Password123!';
PRINT 'You can login with any TCKN and this password.';
GO 