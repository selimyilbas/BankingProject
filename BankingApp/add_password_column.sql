-- Add Password column to Customers table
USE BankingDB;

-- Add Password column
ALTER TABLE Customers ADD Password NVARCHAR(100) NOT NULL DEFAULT '123456';

-- Add index on TCKN for faster authentication
CREATE INDEX IX_Customers_TCKN_Password ON Customers(TCKN, Password);

-- Insert test customers with passwords
INSERT INTO Customers (CustomerNumber, FirstName, LastName, TCKN, Password, DateOfBirth, IsActive)
VALUES 
('000000000001', 'Selim', 'Yilbas', '12345678901', '123456', '2000-05-05', 1),
('000000000002', 'Test', 'User', '11111111111', '123456', '1990-01-01', 1);

-- Insert test accounts
INSERT INTO Accounts (AccountNumber, CustomerId, Currency, Balance, IsActive)
VALUES 
('100000000001', 1, 'TL', 1000.00, 1),
('200000000001', 1, 'EUR', 500.00, 1),
('300000000001', 1, 'USD', 300.00, 1);

PRINT 'Password column added and test data inserted successfully!'; 