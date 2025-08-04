-- Simple Database Fix - Remove PasswordHash, Use Plain Password
USE BankingDB;
GO

PRINT 'Starting simple database fix...';
GO

-- 1. Drop PasswordHash column if it exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Customers DROP COLUMN PasswordHash;
    PRINT 'PasswordHash column dropped';
END
ELSE
BEGIN
    PRINT 'PasswordHash column does not exist';
END
GO

-- 2. Make sure Password column exists and has correct data
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Password')
BEGIN
    ALTER TABLE Customers ADD Password NVARCHAR(100) NOT NULL DEFAULT 'Password123!';
    PRINT 'Password column added with default value';
END
ELSE
BEGIN
    -- Update existing customers with default password
    UPDATE Customers SET Password = 'Password123!' WHERE Password IS NULL OR Password = '';
    PRINT 'Updated existing customers with default password';
END
GO

-- 3. Make sure test customer exists
IF NOT EXISTS (SELECT 1 FROM Customers WHERE TCKN = '12345678901')
BEGIN
    -- Generate customer number using stored procedure
    DECLARE @CustomerNumber VARCHAR(12);
    EXEC GenerateCustomerNumber @CustomerNumber OUTPUT;
    
    INSERT INTO Customers (CustomerNumber, FirstName, LastName, TCKN, Password, DateOfBirth, Email, PhoneNumber, IsActive, CreatedDate)
    VALUES (@CustomerNumber, 'Selim', 'Yilbas', '12345678901', 'Password123!', '2000-05-05', 'selim@example.com', '05551234567', 1, GETUTCDATE());
    PRINT 'Test customer created with CustomerNumber: ' + @CustomerNumber;
END
ELSE
BEGIN
    -- Update test customer password
    UPDATE Customers SET Password = 'Password123!' WHERE TCKN = '12345678901';
    PRINT 'Test customer password updated';
END
GO

-- 4. Show current database state
PRINT '=== DATABASE STATUS ===';
SELECT COUNT(*) as TotalCustomers FROM Customers;
GO

SELECT TOP 3 
    CustomerId,
    CustomerNumber,
    FirstName,
    LastName,
    TCKN,
    Password,
    Email,
    PhoneNumber
FROM Customers;
GO

-- 5. Show table structure
PRINT '=== TABLE STRUCTURE ===';
SELECT 
    COLUMN_NAME as [Column],
    DATA_TYPE as [Type],
    CHARACTER_MAXIMUM_LENGTH as [Length],
    IS_NULLABLE as [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION;
GO

PRINT '=== SIMPLE DATABASE FIX COMPLETED ===';
PRINT 'You can now test the API with:';
PRINT 'TCKN: 12345678901';
PRINT 'Password: Password123!';
GO 