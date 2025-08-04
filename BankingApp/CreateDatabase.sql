-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'BankingDB')
BEGIN
    CREATE DATABASE BankingDB;
END
GO

USE BankingDB;
GO

-- Create Customers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        CustomerId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerNumber VARCHAR(12) NOT NULL UNIQUE,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        TCKN VARCHAR(11) NOT NULL UNIQUE,
        DateOfBirth DATE NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL
    );
END
GO

-- Create Accounts Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Accounts')
BEGIN
    CREATE TABLE Accounts (
        AccountId INT IDENTITY(1,1) PRIMARY KEY,
        AccountNumber VARCHAR(12) NOT NULL UNIQUE,
        CustomerId INT NOT NULL,
        Currency VARCHAR(3) NOT NULL CHECK (Currency IN ('TL', 'EUR', 'USD')),
        Balance DECIMAL(18, 2) NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedDate DATETIME2 NULL,
        CONSTRAINT FK_Accounts_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
        INDEX IX_Accounts_CustomerId_Currency (CustomerId, Currency)
    );
END
GO

-- Create Transactions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        TransactionId INT IDENTITY(1,1) PRIMARY KEY,
        TransactionCode VARCHAR(20) NOT NULL UNIQUE,
        AccountId INT NOT NULL,
        TransactionType VARCHAR(20) NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        Currency VARCHAR(3) NOT NULL,
        ExchangeRate DECIMAL(18, 6) NOT NULL,
        Description NVARCHAR(500) NULL,
        TransactionDate DATETIME2 NOT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Transactions_Accounts FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
        INDEX IX_Transactions_AccountId_TransactionDate (AccountId, TransactionDate)
    );
END
GO

-- Create Transfers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transfers')
BEGIN
    CREATE TABLE Transfers (
        TransferId INT IDENTITY(1,1) PRIMARY KEY,
        TransferCode VARCHAR(20) NOT NULL UNIQUE,
        FromAccountId INT NOT NULL,
        ToAccountId INT NOT NULL,
        Amount DECIMAL(18, 2) NOT NULL,
        FromCurrency VARCHAR(3) NOT NULL,
        ToCurrency VARCHAR(3) NOT NULL,
        ExchangeRate DECIMAL(18, 6) NULL,
        ConvertedAmount DECIMAL(18, 2) NULL,
        Status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
        Description NVARCHAR(500) NULL,
        TransferDate DATETIME2 NOT NULL,
        CompletedDate DATETIME2 NULL,
        CONSTRAINT FK_Transfers_FromAccount FOREIGN KEY (FromAccountId) REFERENCES Accounts(AccountId),
        CONSTRAINT FK_Transfers_ToAccount FOREIGN KEY (ToAccountId) REFERENCES Accounts(AccountId),
        INDEX IX_Transfers_TransferDate (TransferDate),
        INDEX IX_Transfers_FromAccountId_TransferDate (FromAccountId, TransferDate),
        INDEX IX_Transfers_ToAccountId_TransferDate (ToAccountId, TransferDate)
    );
END
GO

-- Create Exchange Rate History Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ExchangeRateHistory')
BEGIN
    CREATE TABLE ExchangeRateHistory (
        RateId INT IDENTITY(1,1) PRIMARY KEY,
        FromCurrency VARCHAR(3) NOT NULL,
        ToCurrency VARCHAR(3) NOT NULL,
        Rate DECIMAL(18, 6) NOT NULL,
        CaptureDate DATETIME2 NOT NULL,
        Source VARCHAR(100) NOT NULL,
        INDEX IX_ExchangeRate_Currencies_Date (FromCurrency, ToCurrency, CaptureDate)
    );
END
GO

-- Create CustomerNumberSequence Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerNumberSequence')
BEGIN
    CREATE TABLE CustomerNumberSequence (
        Id INT PRIMARY KEY,
        LastNumber BIGINT NOT NULL
    );
    
    -- Insert initial value
    INSERT INTO CustomerNumberSequence (Id, LastNumber) VALUES (1, 0);
END
GO

-- Create AccountNumberSequence Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AccountNumberSequence')
BEGIN
    CREATE TABLE AccountNumberSequence (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Currency VARCHAR(3) NOT NULL UNIQUE,
        LastNumber BIGINT NOT NULL
    );
    
    -- Insert initial values
    INSERT INTO AccountNumberSequence (Currency, LastNumber) VALUES ('TL', 0);
    INSERT INTO AccountNumberSequence (Currency, LastNumber) VALUES ('EUR', 0);
    INSERT INTO AccountNumberSequence (Currency, LastNumber) VALUES ('USD', 0);
END
GO

-- Create Stored Procedure for Customer Number Generation
CREATE OR ALTER PROCEDURE GenerateCustomerNumber
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
CREATE OR ALTER PROCEDURE GenerateAccountNumber
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

PRINT 'Database and tables created successfully!';