USE BankingDB;
GO

-- Add PasswordHash column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PasswordHash')
BEGIN
    ALTER TABLE Customers ADD PasswordHash NVARCHAR(256) NULL;
    PRINT 'PasswordHash column added successfully';
END
ELSE
BEGIN
    PRINT 'PasswordHash column already exists';
END
GO

-- Update existing customers with default password hash
UPDATE Customers 
SET PasswordHash = 'AQAAAAEAACcQAAAAEH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3Fq==' 
WHERE PasswordHash IS NULL;
PRINT 'Updated existing customers with default password hash';
GO

-- Make PasswordHash NOT NULL after updating
ALTER TABLE Customers ALTER COLUMN PasswordHash NVARCHAR(256) NOT NULL;
PRINT 'PasswordHash column made NOT NULL';
GO

-- Add Email if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Email')
BEGIN
    ALTER TABLE Customers ADD Email NVARCHAR(100) NULL;
    PRINT 'Email column added successfully';
END
GO

-- Add PhoneNumber if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE Customers ADD PhoneNumber NVARCHAR(20) NULL;
    PRINT 'PhoneNumber column added successfully';
END
GO

-- Show the updated table structure
SELECT 
    COLUMN_NAME as [Column],
    DATA_TYPE as [Type],
    CHARACTER_MAXIMUM_LENGTH as [Length],
    IS_NULLABLE as [Nullable]
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Customers' 
ORDER BY ORDINAL_POSITION;
GO

PRINT 'Database schema fix completed successfully!';
GO 