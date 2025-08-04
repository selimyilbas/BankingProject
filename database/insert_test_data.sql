USE BankingDB;
GO

-- Insert test customers
DECLARE @custNum1 VARCHAR(12), @custNum2 VARCHAR(12);

EXEC GenerateCustomerNumber @newNumber = @custNum1 OUTPUT;
INSERT INTO Customer (CustomerNumber, FirstName, LastName, TCKN, DateOfBirth)
VALUES (@custNum1, N'Ali', N'Yılmaz', '12345678901', '1990-01-15');

EXEC GenerateCustomerNumber @newNumber = @custNum2 OUTPUT;
INSERT INTO Customer (CustomerNumber, FirstName, LastName, TCKN, DateOfBirth)
VALUES (@custNum2, N'Ayşe', N'Demir', '98765432109', '1985-06-20');

-- Get customer IDs
DECLARE @customerId1 INT = (SELECT CustomerId FROM Customer WHERE TCKN = '12345678901');
DECLARE @customerId2 INT = (SELECT CustomerId FROM Customer WHERE TCKN = '98765432109');

-- Create accounts for first customer
DECLARE @accNum1 VARCHAR(12), @accNum2 VARCHAR(12), @accNum3 VARCHAR(12);

EXEC GenerateAccountNumber 'TL', @newNumber = @accNum1 OUTPUT;
INSERT INTO Account (AccountNumber, CustomerId, Currency, Balance)
VALUES (@accNum1, @customerId1, 'TL', 5000.00);

EXEC GenerateAccountNumber 'USD', @newNumber = @accNum2 OUTPUT;
INSERT INTO Account (AccountNumber, CustomerId, Currency, Balance)
VALUES (@accNum2, @customerId1, 'USD', 1000.00);

-- Create account for second customer
EXEC GenerateAccountNumber 'EUR', @newNumber = @accNum3 OUTPUT;
INSERT INTO Account (AccountNumber, CustomerId, Currency, Balance)
VALUES (@accNum3, @customerId2, 'EUR', 2000.00);

-- Display test data
PRINT 'Test Customers:';
SELECT * FROM Customer;

PRINT 'Test Accounts:';
SELECT a.*, c.FirstName + ' ' + c.LastName AS CustomerName 
FROM Account a
JOIN Customer c ON a.CustomerId = c.CustomerId;
GO
