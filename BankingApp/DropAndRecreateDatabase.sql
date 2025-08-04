-- =============================================
-- BankingDB Complete Drop and Recreate Script
-- =============================================
-- This script completely drops and recreates the BankingDB database
-- WARNING: This will delete ALL data in the database!

PRINT 'Starting BankingDB drop and recreate process...';
GO

-- Step 1: Switch to master database to drop the database
USE master;
GO

-- Step 2: Terminate all connections to BankingDB
DECLARE @kill varchar(8000) = '';  
SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'  
FROM sys.dm_exec_sessions
WHERE database_id  = db_id('BankingDB');
EXEC(@kill);
GO

-- Step 3: Drop the database if it exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'BankingDB')
BEGIN
    DROP DATABASE BankingDB;
    PRINT 'BankingDB database dropped successfully.';
END
ELSE
BEGIN
    PRINT 'BankingDB database does not exist.';
END
GO

-- Step 4: Create the database
CREATE DATABASE BankingDB;
PRINT 'BankingDB database created successfully.';
GO

-- Step 5: Switch to the new database
USE BankingDB;
GO

PRINT 'Creating tables...';
GO

-- Create Customers Table with PasswordHash, Email, and PhoneNumber
CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerNumber VARCHAR(12) NOT NULL UNIQUE,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    TCKN VARCHAR(11) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL, -- Added for authentication
    DateOfBirth DATE NOT NULL,
    Email NVARCHAR(100) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NULL
);
GO

-- Create Accounts Table
CREATE TABLE Accounts (
    AccountId INT IDENTITY(1,1) PRIMARY KEY,
    AccountNumber VARCHAR(12) NOT NULL UNIQUE,
    CustomerId INT NOT NULL,
    Currency VARCHAR(3) NOT NULL CHECK (Currency IN ('TL', 'EUR', 'USD')),
    Balance DECIMAL(18, 2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NULL,
    CONSTRAINT FK_Accounts_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);
GO

-- Create Transactions Table
CREATE TABLE Transactions (
    TransactionId INT IDENTITY(1,1) PRIMARY KEY,
    TransactionCode VARCHAR(20) NOT NULL UNIQUE,
    AccountId INT NOT NULL,
    TransactionType VARCHAR(20) NOT NULL CHECK (TransactionType IN ('DEPOSIT', 'WITHDRAWAL', 'TRANSFER_IN', 'TRANSFER_OUT')),
    Amount DECIMAL(18, 2) NOT NULL,
    Currency VARCHAR(3) NOT NULL,
    ExchangeRate DECIMAL(18, 6) NOT NULL DEFAULT 1.000000,
    Description NVARCHAR(500) NULL,
    TransactionDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Transactions_Accounts FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId)
);
GO

-- Create Transfers Table
CREATE TABLE Transfers (
    TransferId INT IDENTITY(1,1) PRIMARY KEY,
    TransferCode VARCHAR(20) NOT NULL UNIQUE,
    FromAccountId INT NOT NULL,
    ToAccountId INT NOT NULL,
    Amount DECIMAL(18, 2) NOT NULL,
    FromCurrency VARCHAR(3) NOT NULL,
    ToCurrency VARCHAR(3) NOT NULL,
    ExchangeRate DECIMAL(18, 6) NULL DEFAULT 1.000000,
    ConvertedAmount DECIMAL(18, 2) NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'PENDING' CHECK (Status IN ('PENDING', 'COMPLETED', 'FAILED', 'CANCELLED')),
    Description NVARCHAR(500) NULL,
    TransferDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CompletedDate DATETIME2 NULL,
    FromTransactionId INT NULL,
    ToTransactionId INT NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Transfers_FromAccount FOREIGN KEY (FromAccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT FK_Transfers_ToAccount FOREIGN KEY (ToAccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT FK_Transfers_FromTransaction FOREIGN KEY (FromTransactionId) REFERENCES Transactions(TransactionId),
    CONSTRAINT FK_Transfers_ToTransaction FOREIGN KEY (ToTransactionId) REFERENCES Transactions(TransactionId)
);
GO

-- Create Exchange Rate History Table
CREATE TABLE ExchangeRateHistory (
    RateId INT IDENTITY(1,1) PRIMARY KEY,
    FromCurrency VARCHAR(3) NOT NULL,
    ToCurrency VARCHAR(3) NOT NULL,
    Rate DECIMAL(18, 6) NOT NULL,
    CaptureDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    Source VARCHAR(100) NOT NULL
);
GO

-- Create CustomerNumberSequence Table
CREATE TABLE CustomerNumberSequence (
    Id INT PRIMARY KEY,
    LastNumber BIGINT NOT NULL
);
GO

-- Create AccountNumberSequence Table
CREATE TABLE AccountNumberSequence (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Currency VARCHAR(3) NOT NULL UNIQUE,
    LastNumber BIGINT NOT NULL
);
GO

PRINT 'Tables created successfully!';
GO

-- Initialize sequences
INSERT INTO CustomerNumberSequence (Id, LastNumber) VALUES (1, 0);
INSERT INTO AccountNumberSequence (Currency, LastNumber) VALUES 
    ('TL', 0),
    ('EUR', 0),
    ('USD', 0);
GO

PRINT 'Sequences initialized successfully!';
GO

-- Create Indexes
PRINT 'Creating indexes...';
GO

-- Customer indexes
CREATE INDEX IX_Customers_CustomerNumber ON Customers(CustomerNumber);
CREATE INDEX IX_Customers_TCKN ON Customers(TCKN);
CREATE INDEX IX_Customers_IsActive ON Customers(IsActive);

-- Account indexes
CREATE INDEX IX_Accounts_CustomerId_Currency ON Accounts(CustomerId, Currency);
CREATE INDEX IX_Accounts_AccountNumber ON Accounts(AccountNumber);
CREATE INDEX IX_Accounts_IsActive ON Accounts(IsActive);

-- Transaction indexes
CREATE INDEX IX_Transactions_AccountId_TransactionDate ON Transactions(AccountId, TransactionDate);
CREATE INDEX IX_Transactions_TransactionCode ON Transactions(TransactionCode);
CREATE INDEX IX_Transactions_TransactionType ON Transactions(TransactionType);

-- Transfer indexes
CREATE INDEX IX_Transfers_TransferDate ON Transfers(TransferDate);
CREATE INDEX IX_Transfers_FromAccountId_TransferDate ON Transfers(FromAccountId, TransferDate);
CREATE INDEX IX_Transfers_ToAccountId_TransferDate ON Transfers(ToAccountId, TransferDate);
CREATE INDEX IX_Transfers_Status ON Transfers(Status);
CREATE INDEX IX_Transfers_TransferCode ON Transfers(TransferCode);

-- Exchange Rate indexes
CREATE INDEX IX_ExchangeRate_Currencies_Date ON ExchangeRateHistory(FromCurrency, ToCurrency, CaptureDate);

PRINT 'Indexes created successfully!';
GO

-- Create Stored Procedures
PRINT 'Creating stored procedures...';
GO

-- Create Stored Procedure for Customer Number Generation
CREATE PROCEDURE GenerateCustomerNumber
    @newNumber VARCHAR(12) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @lastNumber BIGINT;
    
    BEGIN TRANSACTION;
    
    -- Get and update the last number with row lock
    UPDATE CustomerNumberSequence WITH (ROWLOCK)
    SET @lastNumber = LastNumber = LastNumber + 1
    WHERE Id = 1;
    
    -- Format the number with leading zeros
    SET @newNumber = RIGHT('000000000000' + CAST(@lastNumber AS VARCHAR(12)), 12);
    
    COMMIT TRANSACTION;
END
GO

-- Create Stored Procedure for Account Number Generation
CREATE PROCEDURE GenerateAccountNumber
    @currencyType VARCHAR(3),
    @newNumber VARCHAR(12) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @lastNumber BIGINT;
    DECLARE @prefix CHAR(1);
    
    -- Set prefix based on currency
    SET @prefix = CASE @currencyType
        WHEN 'TL' THEN '1'
        WHEN 'EUR' THEN '2'
        WHEN 'USD' THEN '3'
        ELSE '0'
    END;
    
    BEGIN TRANSACTION;
    
    -- Get and update the last number with row lock
    UPDATE AccountNumberSequence WITH (ROWLOCK)
    SET @lastNumber = LastNumber = LastNumber + 1
    WHERE Currency = @currencyType;
    
    -- Format the number with prefix and leading zeros
    SET @newNumber = @prefix + RIGHT('00000000000' + CAST(@lastNumber AS VARCHAR(11)), 11);
    
    COMMIT TRANSACTION;
END
GO

PRINT 'Stored procedures created successfully!';
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
FROM Customers c
LEFT JOIN Accounts a ON c.CustomerId = a.CustomerId
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
FROM Transactions t
JOIN Accounts a ON t.AccountId = a.AccountId
JOIN Customers c ON a.CustomerId = c.CustomerId;
GO

-- Transfer Summary View
CREATE VIEW TransferSummaryView AS
SELECT 
    tr.TransferId,
    tr.TransferCode,
    tr.Status,
    tr.Amount,
    tr.FromCurrency,
    tr.ToCurrency,
    tr.ExchangeRate,
    tr.ConvertedAmount,
    tr.TransferDate,
    tr.CompletedDate,
    tr.Description,
    -- From Account Info
    fa.AccountNumber AS FromAccountNumber,
    fc.CustomerNumber AS FromCustomerNumber,
    CONCAT(fc.FirstName, ' ', fc.LastName) AS FromCustomerName,
    -- To Account Info
    ta.AccountNumber AS ToAccountNumber,
    tc.CustomerNumber AS ToCustomerNumber,
    CONCAT(tc.FirstName, ' ', tc.LastName) AS ToCustomerName
FROM Transfers tr
JOIN Accounts fa ON tr.FromAccountId = fa.AccountId
JOIN Customers fc ON fa.CustomerId = fc.CustomerId
JOIN Accounts ta ON tr.ToAccountId = ta.AccountId
JOIN Customers tc ON ta.CustomerId = tc.CustomerId;
GO

PRINT 'Views created successfully!';
GO

-- Insert initial exchange rates
INSERT INTO ExchangeRateHistory (FromCurrency, ToCurrency, Rate, CaptureDate, Source)
VALUES 
    ('USD', 'TL', 32.50, GETUTCDATE(), 'INITIAL'),
    ('EUR', 'TL', 35.20, GETUTCDATE(), 'INITIAL'),
    ('USD', 'EUR', 0.92, GETUTCDATE(), 'INITIAL'),
    ('TL', 'USD', 0.0308, GETUTCDATE(), 'INITIAL'),
    ('TL', 'EUR', 0.0284, GETUTCDATE(), 'INITIAL'),
    ('EUR', 'USD', 1.087, GETUTCDATE(), 'INITIAL');
GO

-- Test the setup
PRINT 'Testing setup...';
GO

-- Test customer number generation
DECLARE @testCustomerNum VARCHAR(12);
EXEC GenerateCustomerNumber @newNumber = @testCustomerNum OUTPUT;
PRINT 'Generated Customer Number: ' + @testCustomerNum;
GO

-- Test account number generation for each currency
DECLARE @testTLAccount VARCHAR(12);
DECLARE @testEURAccount VARCHAR(12);
DECLARE @testUSDAccount VARCHAR(12);

EXEC GenerateAccountNumber 'TL', @newNumber = @testTLAccount OUTPUT;
EXEC GenerateAccountNumber 'EUR', @newNumber = @testEURAccount OUTPUT;
EXEC GenerateAccountNumber 'USD', @newNumber = @testUSDAccount OUTPUT;

PRINT 'Generated TL Account Number: ' + @testTLAccount;
PRINT 'Generated EUR Account Number: ' + @testEURAccount;
PRINT 'Generated USD Account Number: ' + @testUSDAccount;
GO

-- Verify database objects
PRINT 'Verifying database objects...';
GO

SELECT 'Tables' AS ObjectType, COUNT(*) AS Count FROM sys.tables
UNION ALL
SELECT 'Stored Procedures' AS ObjectType, COUNT(*) AS Count FROM sys.procedures
UNION ALL
SELECT 'Views' AS ObjectType, COUNT(*) AS Count FROM sys.views
UNION ALL
SELECT 'Indexes' AS ObjectType, COUNT(*) AS Count FROM sys.indexes WHERE object_id IN (SELECT object_id FROM sys.tables);
GO

PRINT '✅ BankingDB database has been completely dropped and recreated successfully!';
PRINT 'Database is now ready for use with a fresh schema.';
GO 