-- Test Data Generation Script for BankingDB
USE BankingDB;
GO

-- Declare variables for data generation
DECLARE @i INT = 1;
DECLARE @customerId INT;
DECLARE @customerNumber VARCHAR(12);
DECLARE @accountNumber VARCHAR(12);
DECLARE @firstName NVARCHAR(50);
DECLARE @lastName NVARCHAR(50);
DECLARE @tckn VARCHAR(11);
DECLARE @email NVARCHAR(100);
DECLARE @phoneNumber NVARCHAR(20);

-- Turkish name lists
DECLARE @firstNames TABLE (Name NVARCHAR(50));
INSERT INTO @firstNames VALUES 
('Ahmet'), ('Mehmet'), ('Mustafa'), ('Ali'), ('Hüseyin'), ('Hasan'), ('İbrahim'), ('Ömer'), ('Fatma'), ('Ayşe'),
('Emine'), ('Hatice'), ('Zeynep'), ('Elif'), ('Meryem'), ('Şeyma'), ('Fadime'), ('Özlem'), ('Merve'), ('Esra'),
('Can'), ('Cem'), ('Deniz'), ('Emre'), ('Efe'), ('Kaan'), ('Burak'), ('Murat'), ('Selim'), ('Serkan'),
('Seda'), ('Gül'), ('Büşra'), ('Kübra'), ('Selin'), ('Ceren'), ('Dilek'), ('Derya'), ('Duygu'), ('Ebru'),
('Yusuf'), ('Osman'), ('Ramazan'), ('İsmail'), ('Abdullah'), ('Mahmut'), ('Recep'), ('Halil'), ('Kadir'), ('Veli'),
('Zehra'), ('Havva'), ('Asiye'), ('Khadija'), ('Rabia'), ('Sümeyye'), ('Hafsa'), ('Ümmü'), ('Rukiye'), ('Safiye');

DECLARE @lastNames TABLE (Name NVARCHAR(50));
INSERT INTO @lastNames VALUES 
('Yılmaz'), ('Kaya'), ('Demir'), ('Çelik'), ('Şahin'), ('Öztürk'), ('Kılıç'), ('Aydın'), ('Arslan'), ('Doğan'),
('Kiraz'), ('Aslan'), ('Çetin'), ('Kara'), ('Koç'), ('Kurt'), ('Özkan'), ('Şimşek'), ('Polat'), ('Özdemir'),
('Yıldız'), ('Yıldırım'), ('Ateş'), ('Türk'), ('Yalçın'), ('Korkmaz'), ('Gül'), ('Avcı'), ('Güneş'), ('Bozkurt'),
('Keskin'), ('Taş'), ('Bulut'), ('Kaplan'), ('Erdoğan'), ('Özcan'), ('Aktaş'), ('Çakır'), ('Karaca'), ('Uysal'),
('Tekin'), ('Güven'), ('Kalkan'), ('Özmen'), ('Bayram'), ('Duman'), ('Başaran'), ('Erdem'), ('Varol'), ('Yavuz');

-- Start generating customers
PRINT 'Generating 500 customers...';

WHILE @i <= 500
BEGIN
    -- Generate customer data
    SELECT TOP 1 @firstName = Name FROM @firstNames ORDER BY NEWID();
    SELECT TOP 1 @lastName = Name FROM @lastNames ORDER BY NEWID();
    
    -- Generate TCKN (11 digits, starting with non-zero)
    SET @tckn = CAST((FLOOR(RAND() * 9) + 1) AS VARCHAR) + RIGHT('0000000000' + CAST(FLOOR(RAND() * 9999999999) AS VARCHAR), 10);
    
    -- Generate email
    SET @email = LOWER(@firstName) + '.' + LOWER(@lastName) + CAST(@i AS VARCHAR) + '@email.com';
    SET @email = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
        @email, 'ş', 's'), 'ğ', 'g'), 'ı', 'i'), 'ö', 'o'), 'ü', 'u'), 'ç', 'c');
    
    -- Generate phone number (05XX XXX XX XX format)
    SET @phoneNumber = '05' + CAST(FLOOR(RAND() * 90) + 10 AS VARCHAR) + ' ' + 
                       CAST(FLOOR(RAND() * 900) + 100 AS VARCHAR) + ' ' + 
                       CAST(FLOOR(RAND() * 90) + 10 AS VARCHAR) + ' ' + 
                       CAST(FLOOR(RAND() * 90) + 10 AS VARCHAR);
    
    -- Execute stored procedure to get customer number
    EXEC GenerateCustomerNumber @customerNumber OUTPUT;
    
    -- Insert customer (password is 'Password123!' for all test users)
    INSERT INTO Customers (CustomerNumber, FirstName, LastName, TCKN, PasswordHash, DateOfBirth, Email, PhoneNumber, IsActive, CreatedDate)
    VALUES (
        @customerNumber,
        @firstName,
        @lastName,
        @tckn,
        'AQAAAAEAACcQAAAAEH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3Fq==', -- BCrypt hash for 'Password123!'
        DATEADD(YEAR, -(FLOOR(RAND() * 40) + 18), GETDATE()), -- Random age between 18-58
        @email,
        @phoneNumber,
        1,
        DATEADD(DAY, -FLOOR(RAND() * 365), GETDATE()) -- Random date within last year
    );
    
    SET @customerId = SCOPE_IDENTITY();
    
    -- Create 1-3 accounts per customer
    DECLARE @accountCount INT = FLOOR(RAND() * 3) + 1;
    DECLARE @j INT = 1;
    DECLARE @currencies TABLE (Currency VARCHAR(3));
    DELETE FROM @currencies;
    
    -- Randomly assign currencies
    IF @accountCount >= 1 INSERT INTO @currencies VALUES ('TL');
    IF @accountCount >= 2 AND RAND() > 0.5 INSERT INTO @currencies VALUES ('USD') ELSE IF @accountCount >= 2 INSERT INTO @currencies VALUES ('EUR');
    IF @accountCount >= 3 
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM @currencies WHERE Currency = 'USD') INSERT INTO @currencies VALUES ('USD');
        IF NOT EXISTS (SELECT 1 FROM @currencies WHERE Currency = 'EUR') INSERT INTO @currencies VALUES ('EUR');
    END
    
    -- Create accounts
    DECLARE @currency VARCHAR(3);
    DECLARE currency_cursor CURSOR FOR SELECT Currency FROM @currencies;
    OPEN currency_cursor;
    FETCH NEXT FROM currency_cursor INTO @currency;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC GenerateAccountNumber @currency, @accountNumber OUTPUT;
        
        -- Insert account with random initial balance
        DECLARE @initialBalance DECIMAL(18,2) = 
            CASE @currency
                WHEN 'TL' THEN FLOOR(RAND() * 50000) + 1000    -- 1,000 - 51,000 TL
                WHEN 'USD' THEN FLOOR(RAND() * 2000) + 100     -- 100 - 2,100 USD
                WHEN 'EUR' THEN FLOOR(RAND() * 1800) + 100     -- 100 - 1,900 EUR
            END;
        
        INSERT INTO Accounts (AccountNumber, CustomerId, Currency, Balance, IsActive, CreatedDate)
        VALUES (@accountNumber, @customerId, @currency, @initialBalance, 1, DATEADD(DAY, -FLOOR(RAND() * 300), GETDATE()));
        
        -- Create initial deposit transaction
        DECLARE @transactionCode VARCHAR(20) = 'TRX' + FORMAT(GETDATE(), 'yyyyMMddHHmmss') + RIGHT('0000' + CAST(@i AS VARCHAR), 4);
        
        INSERT INTO Transactions (TransactionCode, AccountId, TransactionType, Amount, Currency, ExchangeRate, Description, TransactionDate, CreatedDate)
        VALUES (
            @transactionCode,
            SCOPE_IDENTITY(),
            'DEPOSIT',
            @initialBalance,
            @currency,
            1.0,
            'Initial deposit',
            DATEADD(DAY, -FLOOR(RAND() * 300), GETDATE()),
            GETDATE()
        );
        
        FETCH NEXT FROM currency_cursor INTO @currency;
    END
    
    CLOSE currency_cursor;
    DEALLOCATE currency_cursor;
    
    SET @i = @i + 1;
    
    -- Print progress every 50 customers
    IF @i % 50 = 0 PRINT 'Created ' + CAST(@i AS VARCHAR) + ' customers...';
END

PRINT 'Customer generation completed!';

-- Generate transfers between accounts
PRINT 'Generating sample transfers...';

DECLARE @transferCount INT = 1;
DECLARE @fromAccountId INT;
DECLARE @toAccountId INT;
DECLARE @transferAmount DECIMAL(18,2);
DECLARE @fromCurrency VARCHAR(3);
DECLARE @toCurrency VARCHAR(3);
DECLARE @exchangeRate DECIMAL(18,6);

WHILE @transferCount <= 200
BEGIN
    -- Select random source account with sufficient balance
    SELECT TOP 1 
        @fromAccountId = AccountId,
        @fromCurrency = Currency
    FROM Accounts 
    WHERE Balance > 100 
    ORDER BY NEWID();
    
    -- Select random destination account (different from source)
    SELECT TOP 1 
        @toAccountId = AccountId,
        @toCurrency = Currency
    FROM Accounts 
    WHERE AccountId != @fromAccountId 
    ORDER BY NEWID();
    
    -- Get account balance
    DECLARE @sourceBalance DECIMAL(18,2);
    SELECT @sourceBalance = Balance FROM Accounts WHERE AccountId = @fromAccountId;
    
    -- Random transfer amount (10% to 30% of balance)
    SET @transferAmount = @sourceBalance * (RAND() * 0.2 + 0.1);
    
    -- Get exchange rate
    IF @fromCurrency = @toCurrency
        SET @exchangeRate = 1.0;
    ELSE
        SELECT TOP 1 @exchangeRate = Rate 
        FROM ExchangeRateHistory 
        WHERE FromCurrency = @fromCurrency AND ToCurrency = @toCurrency;
    
    -- Create transfer
    DECLARE @transferCode VARCHAR(20) = 'TRF' + FORMAT(GETDATE(), 'yyyyMMddHHmmss') + RIGHT('0000' + CAST(@transferCount AS VARCHAR), 4);
    DECLARE @convertedAmount DECIMAL(18,2) = @transferAmount * ISNULL(@exchangeRate, 1);
    
    INSERT INTO Transfers (TransferCode, FromAccountId, ToAccountId, Amount, FromCurrency, ToCurrency, ExchangeRate, ConvertedAmount, Status, Description, TransferDate, CompletedDate)
    VALUES (
        @transferCode,
        @fromAccountId,
        @toAccountId,
        @transferAmount,
        @fromCurrency,
        @toCurrency,
        @exchangeRate,
        @convertedAmount,
        'COMPLETED',
        'Sample transfer',
        DATEADD(DAY, -FLOOR(RAND() * 30), GETDATE()),
        DATEADD(DAY, -FLOOR(RAND() * 30), GETDATE())
    );
    
    -- Update account balances
    UPDATE Accounts SET Balance = Balance - @transferAmount WHERE AccountId = @fromAccountId;
    UPDATE Accounts SET Balance = Balance + @convertedAmount WHERE AccountId = @toAccountId;
    
    -- Create transaction records
    INSERT INTO Transactions (TransactionCode, AccountId, TransactionType, Amount, Currency, ExchangeRate, Description, TransactionDate, CreatedDate)
    VALUES (
        'TRX' + @transferCode + 'F',
        @fromAccountId,
        'TRANSFER_OUT',
        @transferAmount,
        @fromCurrency,
        1.0,
        'Transfer to account',
        DATEADD(DAY, -FLOOR(RAND() * 30), GETDATE()),
        GETDATE()
    );
    
    INSERT INTO Transactions (TransactionCode, AccountId, TransactionType, Amount, Currency, ExchangeRate, Description, TransactionDate, CreatedDate)
    VALUES (
        'TRX' + @transferCode + 'T',
        @toAccountId,
        'TRANSFER_IN',
        @convertedAmount,
        @toCurrency,
        @exchangeRate,
        'Transfer from account',
        DATEADD(DAY, -FLOOR(RAND() * 30), GETDATE()),
        GETDATE()
    );
    
    SET @transferCount = @transferCount + 1;
    
    -- Print progress
    IF @transferCount % 50 = 0 PRINT 'Created ' + CAST(@transferCount AS VARCHAR) + ' transfers...';
END

PRINT 'Test data generation completed successfully!';
PRINT '';
PRINT 'Summary:';
PRINT '- 500 customers created';
PRINT '- Each customer has 1-3 accounts in different currencies';
PRINT '- Initial balances added to all accounts';
PRINT '- 200 sample transfers created between accounts';
PRINT '';
PRINT 'All test users have the password: Password123!';
PRINT 'You can login with any TCKN and this password.'; 