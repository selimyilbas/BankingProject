-- =============================================
-- Banking Application Database Setup Script
-- =============================================

USE BankingDB;
GO

-- Drop existing objects if they exist
IF OBJECT_ID('Transfer', 'U') IS NOT NULL DROP TABLE Transfer;
IF OBJECT_ID('Transaction', 'U') IS NOT NULL DROP TABLE [Transaction];
IF OBJECT_ID('Account', 'U') IS NOT NULL DROP TABLE Account;
IF OBJECT_ID('Customer', 'U') IS NOT NULL DROP TABLE Customer;
IF OBJECT_ID('ExchangeRateHistory', 'U') IS NOT NULL DROP TABLE ExchangeRateHistory;
IF OBJECT_ID('CustomerNumberSequence', 'U') IS NOT NULL DROP TABLE CustomerNumberSequence;
IF OBJECT_ID('AccountNumberSequence', 'U') IS NOT NULL DROP TABLE AccountNumberSequence;
IF OBJECT_ID('GenerateCustomerNumber', 'P') IS NOT NULL DROP PROCEDURE GenerateCustomerNumber;
IF OBJECT_ID('GenerateAccountNumber', 'P') IS NOT NULL DROP PROCEDURE GenerateAccountNumber;
GO

PRINT 'Creating tables...';
GO

-- 1. Customer Table
CREATE TABLE Customer (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerNumber VARCHAR(12) UNIQUE NOT NULL,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    TCKN CHAR(11) UNIQUE NOT NULL,
    DateOfBirth DATE NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE()
);
GO

-- 2. Account Table
CREATE TABLE Account (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    AccountNumber VARCHAR(12) UNIQUE NOT NULL,
    CustomerId INT NOT NULL,
    Currency VARCHAR(3) NOT NULL CHECK (Currency IN ('TL', 'EUR', 'USD')),
    Balance DECIMAL(18, 2) DEFAULT 0.00,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CustomerId) REFERENCES Customer(CustomerId)
);
GO

-- 3. Transaction Table
CREATE TABLE [Transaction] (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    TransactionCode VARCHAR(20) UNIQUE NOT NULL,
    AccountId INT NOT NULL,
    TransactionType VARCHAR(20) NOT NULL CHECK (TransactionType IN ('DEPOSIT', 'WITHDRAWAL', 'TRANSFER_IN', 'TRANSFER_OUT')),
    Amount DECIMAL(18, 2) NOT NULL,
    Currency VARCHAR(3) NOT NULL,
    ExchangeRate DECIMAL(10, 6) DEFAULT 1.000000,
    Description NVARCHAR(255),
    TransactionDate DATETIME DEFAULT GETDATE(),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (AccountId) REFERENCES Account(AccountId)
);
GO

-- 4. Transfer Table
CREATE TABLE Transfer (
    TransferId INT IDENTITY(1,1) PRIMARY KEY,
    TransferCode VARCHAR(20) UNIQUE NOT NULL,
    FromAccountId INT NOT NULL,
    ToAccountId INT NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    FromCurrency VARCHAR(3) NOT NULL,
    ToCurrency VARCHAR(3) NOT NULL,
    ExchangeRate DECIMAL(10, 6) DEFAULT 1.000000,
    ConvertedAmount DECIMAL(18, 2) NOT NULL,
    Description NVARCHAR(255),
    TransferDate DATETIME DEFAULT GETDATE(),
    Status VARCHAR(20) DEFAULT 'COMPLETED' CHECK (Status IN ('PENDING', 'COMPLETED', 'FAILED', 'CANCELLED')),
    FromTransactionId INT,
    ToTransactionId INT,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (FromAccountId) REFERENCES Account(AccountId),
    FOREIGN KEY (ToAccountId) REFERENCES Account(AccountId),
    FOREIGN KEY (FromTransactionId) REFERENCES [Transaction](TransactionId),
    FOREIGN KEY (ToTransactionId) REFERENCES [Transaction](TransactionId)
);
GO

-- 5. Exchange Rate History Table
CREATE TABLE ExchangeRateHistory (
    RateId INT IDENTITY(1,1) PRIMARY KEY,
    FromCurrency VARCHAR(3) NOT NULL,
    ToCurrency VARCHAR(3) NOT NULL,
    Rate DECIMAL(10, 6) NOT NULL,
    Source VARCHAR(50),
    CaptureDate DATETIME DEFAULT GETDATE()
);
GO

-- 6. Customer Number Sequence Table
CREATE TABLE CustomerNumberSequence (
    Id INT PRIMARY KEY DEFAULT 1,
    LastNumber BIGINT NOT NULL DEFAULT 0,
    CHECK (Id = 1)
);
GO

-- 7. Account Number Sequence Table
CREATE TABLE AccountNumberSequence (
    Currency VARCHAR(3) PRIMARY KEY,
    LastNumber BIGINT NOT NULL DEFAULT 0
);
GO

-- Initialize sequences
INSERT INTO CustomerNumberSequence (Id, LastNumber) VALUES (1, 0);
INSERT INTO AccountNumberSequence (Currency, LastNumber) VALUES 
    ('TL', 0),
    ('EUR', 0),
    ('USD', 0);
GO

PRINT 'Tables created successfully!';
GO

-- Create Stored Procedures
PRINT 'Creating stored procedures...';
GO

-- Stored Procedure: Generate Customer Number
CREATE PROCEDURE GenerateCustomerNumber
    @newNumber VARCHAR(12) OUTPUT
AS
BEGIN
    DECLARE @lastNum BIGINT;
    
    BEGIN TRANSACTION;
    
    SELECT @lastNum = LastNumber 
    FROM CustomerNumberSequence WITH (UPDLOCK)
    WHERE Id = 1;
    
    SET @lastNum = @lastNum + 1;
    
    UPDATE CustomerNumberSequence 
    SET LastNumber = @lastNum 
    WHERE Id = 1;
    
    SET @newNumber = RIGHT('000000000000' + CAST(@lastNum AS VARCHAR(12)), 12);
    
    COMMIT;
END;
GO

-- Stored Procedure: Generate Account Number
CREATE PROCEDURE GenerateAccountNumber
    @currencyType VARCHAR(3),
    @newNumber VARCHAR(12) OUTPUT
AS
BEGIN
    DECLARE @lastNum BIGINT;
    DECLARE @prefix CHAR(1);
    
    SET @prefix = CASE @currencyType
        WHEN 'TL' THEN '1'
        WHEN 'EUR' THEN '2'
        WHEN 'USD' THEN '3'
        ELSE NULL
    END;
    
    IF @prefix IS NULL
    BEGIN
        RAISERROR('Invalid currency type', 16, 1);
        RETURN;
    END;
    
    BEGIN TRANSACTION;
    
    SELECT @lastNum = LastNumber 
    FROM AccountNumberSequence WITH (UPDLOCK)
    WHERE Currency = @currencyType;
    
    SET @lastNum = @lastNum + 1;
    
    UPDATE AccountNumberSequence 
    SET LastNumber = @lastNum 
    WHERE Currency = @currencyType;
    
    SET @newNumber = @prefix + RIGHT('00000000000' + CAST(@lastNum AS VARCHAR(11)), 11);
    
    COMMIT;
END;
GO

PRINT 'Stored procedures created successfully!';
GO

-- Create Indexes
PRINT 'Creating indexes...';
GO

CREATE INDEX idx_customer_number ON Customer(CustomerNumber);
CREATE INDEX idx_tckn ON Customer(TCKN);
CREATE INDEX idx_account_number ON Account(AccountNumber);
CREATE INDEX idx_customer_id ON Account(CustomerId);
CREATE INDEX idx_transaction_account ON [Transaction](AccountId);
CREATE INDEX idx_transaction_date ON [Transaction](TransactionDate);
CREATE INDEX idx_transfer_from_account ON Transfer(FromAccountId);
CREATE INDEX idx_transfer_to_account ON Transfer(ToAccountId);
CREATE INDEX idx_exchange_rate_currencies ON ExchangeRateHistory(FromCurrency, ToCurrency);
GO

PRINT 'Indexes created successfully!';
GO

-- Create Views
PRINT 'Creating views...';
GO

-- Customer Account Summary View
CREATE VIEW CustomerAccountSummary AS
SELECT 
    c.CustomerId,
    c.CustomerNumber,
    c.FirstName,
    c.LastName,
    c.TCKN,
    a.AccountId,
    a.AccountNumber,
    a.Currency,
    a.Balance,
    a.IsActive AS AccountActive
FROM Customer c
LEFT JOIN Account a ON c.CustomerId = a.CustomerId
WHERE c.IsActive = 1;
GO

-- Transaction History View
CREATE VIEW TransactionHistoryView AS
SELECT 
    t.TransactionId,
    t.TransactionCode,
    a.AccountNumber,
    c.CustomerNumber,
    CONCAT(c.FirstName, ' ', c.LastName) AS CustomerName,
    t.TransactionType,
    t.Amount,
    t.Currency,
    t.ExchangeRate,
    t.Description,
    t.TransactionDate
FROM [Transaction] t
JOIN Account a ON t.AccountId = a.AccountId
JOIN Customer c ON a.CustomerId = c.CustomerId;
GO

PRINT 'Views created successfully!';
GO

-- Test the setup
PRINT 'Testing setup...';
GO

-- Test customer number generation
DECLARE @testCustomerNum VARCHAR(12);
EXEC GenerateCustomerNumber @newNumber = @testCustomerNum OUTPUT;
PRINT 'Generated Customer Number: ' + @testCustomerNum;
GO

-- Test account number generation
DECLARE @testTLAccount VARCHAR(12);
EXEC GenerateAccountNumber 'TL', @newNumber = @testTLAccount OUTPUT;
PRINT 'Generated TL Account Number: ' + @testTLAccount;
GO

PRINT '✅ Database setup completed successfully!';
GO
