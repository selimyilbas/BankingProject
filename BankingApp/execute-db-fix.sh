#!/bin/bash

echo "Starting database schema fix..."

# Connect to SQL Server and execute commands
docker exec -i banking-sqlserver bash -c "
echo 'USE BankingDB;' > /tmp/fix.sql
echo 'GO' >> /tmp/fix.sql
echo '' >> /tmp/fix.sql
echo '-- Add PasswordHash column if it does not exist' >> /tmp/fix.sql
echo 'IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(\"Customers\") AND name = \"PasswordHash\")' >> /tmp/fix.sql
echo 'BEGIN' >> /tmp/fix.sql
echo '    ALTER TABLE Customers ADD PasswordHash NVARCHAR(256) NULL;' >> /tmp/fix.sql
echo '    PRINT \"PasswordHash column added successfully\";' >> /tmp/fix.sql
echo 'END' >> /tmp/fix.sql
echo 'GO' >> /tmp/fix.sql
echo '' >> /tmp/fix.sql
echo '-- Update existing customers with default password hash' >> /tmp/fix.sql
echo 'UPDATE Customers SET PasswordHash = \"AQAAAAEAACcQAAAAEH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3FqKH3+7Y3+3Fq==\" WHERE PasswordHash IS NULL;' >> /tmp/fix.sql
echo 'GO' >> /tmp/fix.sql
echo '' >> /tmp/fix.sql
echo '-- Make PasswordHash NOT NULL' >> /tmp/fix.sql
echo 'ALTER TABLE Customers ALTER COLUMN PasswordHash NVARCHAR(256) NOT NULL;' >> /tmp/fix.sql
echo 'GO' >> /tmp/fix.sql
echo '' >> /tmp/fix.sql
echo '-- Add Email column if it does not exist' >> /tmp/fix.sql
echo 'IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(\"Customers\") AND name = \"Email\")' >> /tmp/fix.sql
echo 'BEGIN' >> /tmp/fix.sql
echo '    ALTER TABLE Customers ADD Email NVARCHAR(100) NULL;' >> /tmp/fix.sql
echo '    PRINT \"Email column added successfully\";' >> /tmp/fix.sql
echo 'END' >> /tmp/fix.sql
echo 'GO' >> /tmp/fix.sql
echo '' >> /tmp/fix.sql
echo '-- Add PhoneNumber column if it does not exist' >> /tmp/fix.sql
echo 'IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(\"Customers\") AND name = \"PhoneNumber\")' >> /tmp/fix.sql
echo 'BEGIN' >> /tmp/fix.sql
echo '    ALTER TABLE Customers ADD PhoneNumber NVARCHAR(20) NULL;' >> /tmp/fix.sql
echo '    PRINT \"PhoneNumber column added successfully\";' >> /tmp/fix.sql
echo 'END' >> /tmp/fix.sql
echo 'GO' >> /tmp/fix.sql
"

echo "SQL script created. Now executing..."

# Try to execute using sqlcmd if available
if docker exec banking-sqlserver which sqlcmd > /dev/null 2>&1; then
    docker exec -i banking-sqlserver sqlcmd -S localhost -U sa -P 'Selim@123456789' -d BankingDB -i /tmp/fix.sql
else
    echo "sqlcmd not available in container. Please execute the SQL manually:"
    echo "1. Connect to your SQL Server instance"
    echo "2. Execute the contents of /tmp/fix.sql in the container"
    echo "3. Or use Azure Data Studio / SQL Server Management Studio"
fi

echo "Database fix script completed!" 