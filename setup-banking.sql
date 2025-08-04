USE BankingDB;
GO

-- Create all tables
CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerNumber VARCHAR(12) NOT NULL UNIQUE,
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    TCKN VARCHAR(11) NOT NULL UNIQUE,
    Password NVARCHAR(50) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Email NVARCHAR(100) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedDate DATETIME2 NULL
);

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
    CONSTRAINT FK_Transactions_Accounts FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId)
);

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
    CONSTRAINT FK_Transfers_ToAccount FOREIGN KEY (ToAccountId) REFERENCES Accounts(AccountId)
);

CREATE TABLE ExchangeRateHistory (
    RateId INT IDENTITY(1,1) PRIMARY KEY,
    FromCurrency VARCHAR(3) NOT NULL,
    ToCurrency VARCHAR(3) NOT NULL,
    Rate DECIMAL(18, 6) NOT NULL,
    CaptureDate DATETIME2 NOT NULL,
    Source VARCHAR(100) NOT NULL
);

CREATE TABLE CustomerNumberSequence (
    Id INT PRIMARY KEY,
    LastNumber BIGINT NOT NULL
);

CREATE TABLE AccountNumberSequence (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Currency VARCHAR(3) NOT NULL UNIQUE,
    LastNumber BIGINT NOT NULL
);

-- Insert initial data
INSERT INTO CustomerNumberSequence (Id, LastNumber) VALUES (1, 0);
INSERT INTO AccountNumberSequence (Currency, LastNumber) VALUES ('TL', 0), ('EUR', 0), ('USD', 0);

-- Insert test customers
INSERT INTO Customers (CustomerNumber, FirstName, LastName, TCKN, Password, DateOfBirth, Email, PhoneNumber)
VALUES 
('000000000001', 'Ahmet', 'Yılmaz', '12345678901', '123456', '1990-01-15', 'ahmet@email.com', '0532 111 22 33'),
('000000000002', 'Ayşe', 'Kaya', '98765432109', '123456', '1985-05-20', 'ayse@email.com', '0533 222 33 44'),
('000000000003', 'Test', 'User', '11111111111', '123456', '1995-03-10', 'test@email.com', '0534 333 44 55');

-- Insert test accounts
INSERT INTO Accounts (AccountNumber, CustomerId, Currency, Balance)
VALUES 
('100000000001', 1, 'TL', 50000),
('300000000001', 1, 'USD', 1000),
('100000000002', 2, 'TL', 75000),
('200000000001', 2, 'EUR', 2000),
('100000000003', 3, 'TL', 100000);

-- Insert exchange rates
INSERT INTO ExchangeRateHistory (FromCurrency, ToCurrency, Rate, CaptureDate, Source)
VALUES 
('USD', 'TL', 32.50, GETUTCDATE(), 'INITIAL'),
('EUR', 'TL', 35.20, GETUTCDATE(), 'INITIAL'),
('USD', 'EUR', 0.92, GETUTCDATE(), 'INITIAL'),
('TL', 'USD', 0.0308, GETUTCDATE(), 'INITIAL'),
('TL', 'EUR', 0.0284, GETUTCDATE(), 'INITIAL'),
('EUR', 'USD', 1.087, GETUTCDATE(), 'INITIAL');

PRINT 'Database setup completed successfully!';
GO
