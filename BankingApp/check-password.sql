-- Check Password Hash
USE BankingDB;
GO

-- Check the actual password hash for the test customer
SELECT 
    CustomerId,
    CustomerNumber,
    FirstName,
    LastName,
    TCKN,
    PasswordHash,
    LEN(PasswordHash) as HashLength
FROM Customers 
WHERE TCKN = '12345678901';
GO

-- Check if there are any customers with NULL password hash
SELECT COUNT(*) as CustomersWithNullPassword
FROM Customers 
WHERE PasswordHash IS NULL;
GO

-- Show all customers and their password status
SELECT 
    CustomerId,
    CustomerNumber,
    FirstName,
    LastName,
    TCKN,
    CASE WHEN PasswordHash IS NOT NULL THEN 'Yes' ELSE 'No' END as HasPassword,
    LEN(PasswordHash) as HashLength
FROM Customers;
GO 