-- Quick Database Fix
-- Run this in Azure Data Studio or SQL Server Management Studio

USE BankingDB;
GO

-- Add PasswordHash column if it doesn't exist
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

-- Update existing customers with default password
UPDATE Customers SET PasswordHash = 'AQAAAAEAACcQAAAAEH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3Fq==' WHERE PasswordHash IS NULL;
PRINT 'Updated customers with default password';
GO

-- Make PasswordHash NOT NULL
ALTER TABLE Customers ALTER COLUMN PasswordHash NVARCHAR(256) NOT NULL;
PRINT 'Made PasswordHash NOT NULL';
GO

-- Add Email column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Email')
BEGIN
    ALTER TABLE Customers ADD Email NVARCHAR(100) NULL;
    PRINT 'Email column added';
END
GO

-- Add PhoneNumber column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE Customers ADD PhoneNumber NVARCHAR(20) NULL;
    PRINT 'PhoneNumber column added';
END
GO

-- Show the result
SELECT TOP 3 CustomerId, CustomerNumber, FirstName, LastName, TCKN, 
       CASE WHEN PasswordHash IS NOT NULL THEN 'Yes' ELSE 'No' END as HasPassword
FROM Customers;
GO

PRINT 'Database fix completed!';
GO 