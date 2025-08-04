-- Create CustomerNumberSequence Table (if it doesn't exist)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CustomerNumberSequence')
BEGIN
    CREATE TABLE CustomerNumberSequence (
        Id INT PRIMARY KEY,
        LastNumber BIGINT NOT NULL
    );
    
    -- Insert initial value
    INSERT INTO CustomerNumberSequence (Id, LastNumber) VALUES (1, 0);
    PRINT 'CustomerNumberSequence table created with initial value';
END
ELSE
BEGIN
    PRINT 'CustomerNumberSequence table already exists';
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

PRINT 'GenerateCustomerNumber stored procedure created successfully!';
GO

-- Test the stored procedure
DECLARE @testNumber VARCHAR(12);
EXEC GenerateCustomerNumber @testNumber OUTPUT;
PRINT 'Test customer number generated: ' + @testNumber;
GO 