-- Update Customer Schema to match the exact database structure
USE BankingDB;
GO

PRINT 'Starting Customer schema update...';
GO

-- Step 1: Add new columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Customers ADD PasswordHash NVARCHAR(256) NOT NULL DEFAULT '';
    PRINT 'PasswordHash column added successfully.';
END
ELSE
BEGIN
    PRINT 'PasswordHash column already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Email')
BEGIN
    ALTER TABLE Customers ADD Email NVARCHAR(100) NULL;
    PRINT 'Email column added successfully.';
END
ELSE
BEGIN
    PRINT 'Email column already exists.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE Customers ADD PhoneNumber NVARCHAR(20) NULL;
    PRINT 'PhoneNumber column added successfully.';
END
ELSE
BEGIN
    PRINT 'PhoneNumber column already exists.';
END
GO

-- Step 2: Migrate existing Password data to PasswordHash if Password column exists
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Password')
BEGIN
    UPDATE Customers 
    SET PasswordHash = Password 
    WHERE PasswordHash = '' AND Password IS NOT NULL;
    PRINT 'Existing Password data migrated to PasswordHash.';
END
GO

-- Step 3: Update existing test data with email and phone number
UPDATE Customers 
SET Email = 'selim.yilbas@example.com',
    PhoneNumber = '+905551234567'
WHERE CustomerNumber = '000000000001';
GO

UPDATE Customers 
SET Email = 'test.user@example.com',
    PhoneNumber = '+905559876543'
WHERE CustomerNumber = '000000000002';
GO

-- Step 4: Verify the changes
PRINT 'Verifying schema changes...';
GO

SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION;
GO

PRINT '✅ Customer schema update completed successfully!';
PRINT 'New columns: PasswordHash (256 chars), Email (nullable, 100 chars), PhoneNumber (nullable, 20 chars) have been added.';
PRINT 'Existing test data has been updated with sample email and phone numbers.';
GO 