-- Test Database
USE BankingDB;
GO

-- Check if we have any customers
SELECT COUNT(*) as TotalCustomers FROM Customers;
GO

-- Check if we have the test customer
SELECT CustomerId, CustomerNumber, FirstName, LastName, TCKN, 
       CASE WHEN PasswordHash IS NOT NULL THEN 'Yes' ELSE 'No' END as HasPassword
FROM Customers WHERE TCKN = '12345678901';
GO

-- Check table structure
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION;
GO

-- If no customers exist, let's create a test customer
IF NOT EXISTS (SELECT 1 FROM Customers WHERE TCKN = '12345678901')
BEGIN
    INSERT INTO Customers (CustomerNumber, FirstName, LastName, TCKN, PasswordHash, DateOfBirth, Email, PhoneNumber, IsActive, CreatedDate)
    VALUES ('000000000001', 'Selim', 'Yilbas', '12345678901', 'AQAAAAEAACcQAAAAEH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3Fq==', '2000-05-05', 'selim@example.com', '05551234567', 1, GETUTCDATE());
    PRINT 'Test customer created';
END
ELSE
BEGIN
    PRINT 'Test customer already exists';
END
GO

-- Show the test customer
SELECT CustomerId, CustomerNumber, FirstName, LastName, TCKN, 
       CASE WHEN PasswordHash IS NOT NULL THEN 'Yes' ELSE 'No' END as HasPassword
FROM Customers WHERE TCKN = '12345678901';
GO 