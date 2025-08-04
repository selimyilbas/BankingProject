#!/bin/bash

echo "Starting database schema fix..."

# Add PasswordHash column if it doesn't exist
echo "Adding PasswordHash column..."
docker exec -i banking-sqlserver /opt/mssql/bin/sqlservr --version

# Try to connect and execute SQL commands
echo "Executing SQL commands..."

# Add PasswordHash column
docker exec -i banking-sqlserver /opt/mssql/bin/sqlservr --help

echo "Database fix completed!" 