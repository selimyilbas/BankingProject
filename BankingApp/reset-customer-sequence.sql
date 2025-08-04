-- Reset Customer Number Sequence to avoid duplicates
USE BankingDB;
GO

-- Get the maximum customer number currently in use
DECLARE @maxCustomerNumber BIGINT = 0;

SELECT @maxCustomerNumber = ISNULL(MAX(CAST(CustomerNumber AS BIGINT)), 0)
FROM Customers;

PRINT 'Current maximum customer number: ' + CAST(@maxCustomerNumber AS VARCHAR);

-- Update the sequence to start after the maximum existing number
UPDATE CustomerNumberSequence 
SET LastNumber = @maxCustomerNumber
WHERE Id = 1;

PRINT 'Customer number sequence reset to: ' + CAST(@maxCustomerNumber AS VARCHAR);

-- Test the next number generation
DECLARE @testNumber VARCHAR(12);
EXEC GenerateCustomerNumber @testNumber OUTPUT;
PRINT 'Next customer number will be: ' + @testNumber;

GO 