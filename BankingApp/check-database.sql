-- Check Database Structure
USE BankingDB;
GO

-- Check if Customers table exists
SELECT 'Customers table exists' as Status, COUNT(*) as Count 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME = 'Customers';
GO

-- Check Customers table structure
SELECT 
    COLUMN_NAME as [Column],
    DATA_TYPE as [Type],
    CHARACTER_MAXIMUM_LENGTH as [Length],
    IS_NULLABLE as [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION;
GO

-- Check if PasswordHash column exists
SELECT 'PasswordHash column exists' as Status, COUNT(*) as Count 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' AND COLUMN_NAME = 'PasswordHash';
GO

-- Check sample data
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

-- Count total customers
SELECT COUNT(*) as TotalCustomers FROM Customers;
GO 