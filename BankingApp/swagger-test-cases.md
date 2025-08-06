# BankingApp Swagger Test Cases

## Base URL
- **API**: `http://localhost:5115`
- **Swagger UI**: `http://localhost:5115/swagger`

---

## 1. Authentication Endpoints

### 1.1 Login
**Endpoint**: `POST /api/auth/login`

**Test Cases**:

#### ✅ **Valid Login**
```json
{
  "tckn": "12345678901",
  "password": "123456"
}
```
**Expected Response**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "customerId": 1,
    "customerNumber": "000000000001",
    "firstName": "Selim",
    "lastName": "Yilbas",
    "tckn": "12345678901"
  }
}
```

#### ❌ **Invalid TCKN**
```json
{
  "tckn": "99999999999",
  "password": "123456"
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "Geçersiz TCKN veya şifre"
}
```

#### ❌ **Invalid Password**
```json
{
  "tckn": "12345678901",
  "password": "wrongpassword"
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "Geçersiz TCKN veya şifre"
}
```

#### ❌ **Empty Fields**
```json
{
  "tckn": "",
  "password": ""
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "TCKN ve şifre gereklidir"
}
```

### 1.2 Register
**Endpoint**: `POST /api/auth/register`

**Test Cases**:

#### ✅ **Valid Registration**
```json
{
  "firstName": "Test",
  "lastName": "User",
  "tckn": "55555555555",
  "password": "123456",
  "dateOfBirth": "1995-01-15",
  "email": "test@example.com",
  "phoneNumber": "05551234567"
}
```
**Expected Response**:
```json
{
  "success": true,
  "message": "Kayıt başarılı",
  "data": {
    "customerId": 2,
    "customerNumber": "000000000002",
    "firstName": "Test",
    "lastName": "User",
    "tckn": "55555555555",
    "email": "test@example.com",
    "phoneNumber": "05551234567",
    "dateOfBirth": "1995-01-15T00:00:00",
    "isActive": true,
    "createdDate": "2024-01-XX..."
  }
}
```

#### ❌ **Duplicate TCKN**
```json
{
  "firstName": "Test",
  "lastName": "User",
  "tckn": "12345678901",
  "password": "123456",
  "dateOfBirth": "1995-01-15"
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "Bu TCKN ile kayıtlı bir müşteri zaten mevcut"
}
```

---

## 2. Customer Endpoints

### 2.1 Create Customer
**Endpoint**: `POST /api/Customer`

**Test Cases**:

#### ✅ **Valid Customer Creation**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "tckn": "11111111111",
  "password": "123456",
  "dateOfBirth": "1990-05-20",
  "email": "john.doe@example.com",
  "phoneNumber": "05551111111"
}
```
**Expected Response**:
```json
{
  "success": true,
  "message": "Customer created successfully",
  "data": {
    "customerId": 3,
    "customerNumber": "000000000003",
    "firstName": "John",
    "lastName": "Doe",
    "tckn": "11111111111",
    "email": "john.doe@example.com",
    "phoneNumber": "05551111111",
    "dateOfBirth": "1990-05-20T00:00:00",
    "isActive": true,
    "createdDate": "2024-01-XX..."
  }
}
```

#### ❌ **Duplicate TCKN**
```json
{
  "firstName": "Jane",
  "lastName": "Doe",
  "tckn": "12345678901",
  "password": "123456",
  "dateOfBirth": "1992-08-15"
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "Customer with this TCKN already exists"
}
```

#### ❌ **Invalid Date Format**
```json
{
  "firstName": "Test",
  "lastName": "User",
  "tckn": "22222222222",
  "password": "123456",
  "dateOfBirth": "invalid-date"
}
```
**Expected Response**: 400 Bad Request

### 2.2 Get Customer by ID
**Endpoint**: `GET /api/Customer/{id}`

**Test Cases**:

#### ✅ **Valid Customer ID**
- **URL**: `GET /api/Customer/1`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Customer retrieved successfully",
  "data": {
    "customerId": 1,
    "customerNumber": "000000000001",
    "firstName": "Selim",
    "lastName": "Yilbas",
    "tckn": "12345678901",
    "email": "selim@example.com",
    "phoneNumber": "05551234567",
    "dateOfBirth": "2000-05-05T00:00:00",
    "isActive": true,
    "createdDate": "2024-01-XX..."
  }
}
```

#### ❌ **Invalid Customer ID**
- **URL**: `GET /api/Customer/999`
- **Expected Response**:
```json
{
  "success": false,
  "message": "Customer not found"
}
```

### 2.3 Get All Customers
**Endpoint**: `GET /api/Customer`

**Test Cases**:

#### ✅ **Get All Customers**
- **URL**: `GET /api/Customer`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Customers retrieved successfully",
  "data": [
    {
      "customerId": 1,
      "customerNumber": "000000000001",
      "firstName": "Selim",
      "lastName": "Yilbas",
      "tckn": "12345678901",
      "email": "selim@example.com",
      "phoneNumber": "05551234567",
      "dateOfBirth": "2000-05-05T00:00:00",
      "isActive": true,
      "createdDate": "2024-01-XX..."
    }
    // ... more customers
  ]
}
```

---

## 3. Account Endpoints

### 3.1 Get Accounts by Customer ID
**Endpoint**: `GET /api/Account/customer/{customerId}`

**Test Cases**:

#### ✅ **Valid Customer with Accounts**
- **URL**: `GET /api/Account/customer/1`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Accounts retrieved successfully",
  "data": [
    {
      "accountId": 1,
      "accountNumber": "100000000001",
      "customerId": 1,
      "currency": "TL",
      "balance": 5000.00,
      "isActive": true,
      "createdDate": "2024-01-XX..."
    },
    {
      "accountId": 2,
      "accountNumber": "200000000001",
      "customerId": 1,
      "currency": "USD",
      "balance": 1000.00,
      "isActive": true,
      "createdDate": "2024-01-XX..."
    }
  ]
}
```

#### ✅ **Customer with No Accounts**
- **URL**: `GET /api/Account/customer/999`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Accounts retrieved successfully",
  "data": []
}
```

### 3.2 Get Account Balance
**Endpoint**: `GET /api/Account/{accountId}/balance`

**Test Cases**:

#### ✅ **Valid Account ID**
- **URL**: `GET /api/Account/1/balance`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Account balance retrieved successfully",
  "data": {
    "accountId": 1,
    "accountNumber": "100000000001",
    "balance": 5000.00,
    "currency": "TL"
  }
}
```

#### ❌ **Invalid Account ID**
- **URL**: `GET /api/Account/999/balance`
- **Expected Response**:
```json
{
  "success": false,
  "message": "Account not found"
}
```

---

## 4. Transaction Endpoints

### 4.1 Get Transactions by Account ID
**Endpoint**: `GET /api/Transaction/account/{accountId}`

**Test Cases**:

#### ✅ **Valid Account with Transactions**
- **URL**: `GET /api/Transaction/account/1`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Transactions retrieved successfully",
  "data": [
    {
      "transactionId": 1,
      "transactionCode": "TRX202401011200001",
      "accountId": 1,
      "transactionType": "DEPOSIT",
      "amount": 5000.00,
      "currency": "TL",
      "exchangeRate": 1.0,
      "description": "Initial deposit",
      "transactionDate": "2024-01-01T12:00:00",
      "createdDate": "2024-01-01T12:00:00"
    }
  ]
}
```

#### ✅ **Account with No Transactions**
- **URL**: `GET /api/Transaction/account/999`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Transactions retrieved successfully",
  "data": []
}
```

### 4.2 Create Deposit
**Endpoint**: `POST /api/Transaction/deposit`

**Test Cases**:

#### ✅ **Valid Deposit**
```json
{
  "accountId": 1,
  "amount": 1000.00,
  "description": "Salary deposit"
}
```
**Expected Response**:
```json
{
  "success": true,
  "message": "Deposit created successfully",
  "data": {
    "transactionId": 2,
    "transactionCode": "TRX202401011300001",
    "accountId": 1,
    "transactionType": "DEPOSIT",
    "amount": 1000.00,
    "currency": "TL",
    "exchangeRate": 1.0,
    "description": "Salary deposit",
    "transactionDate": "2024-01-01T13:00:00",
    "createdDate": "2024-01-01T13:00:00"
  }
}
```

#### ❌ **Invalid Account ID**
```json
{
  "accountId": 999,
  "amount": 1000.00,
  "description": "Test deposit"
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "Account not found"
}
```

#### ❌ **Negative Amount**
```json
{
  "accountId": 1,
  "amount": -100.00,
  "description": "Invalid deposit"
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "Amount must be positive"
}
```

---

## 5. Transfer Endpoints

### 5.1 Create Transfer
**Endpoint**: `POST /api/Transfer`

**Test Cases**:

#### ✅ **Valid Transfer**
```json
{
  "fromAccountId": 1,
  "toAccountId": 2,
  "amount": 500.00,
  "description": "Transfer to USD account"
}
```
**Expected Response**:
```json
{
  "success": true,
  "message": "Transfer created successfully",
  "data": {
    "transferId": 1,
    "transferCode": "TRF202401011400001",
    "fromAccountId": 1,
    "toAccountId": 2,
    "amount": 500.00,
    "fromCurrency": "TL",
    "toCurrency": "USD",
    "exchangeRate": 0.0308,
    "convertedAmount": 15.40,
    "status": "COMPLETED",
    "description": "Transfer to USD account",
    "transferDate": "2024-01-01T14:00:00",
    "completedDate": "2024-01-01T14:00:00"
  }
}
```

#### ❌ **Insufficient Balance**
```json
{
  "fromAccountId": 1,
  "toAccountId": 2,
  "amount": 10000.00,
  "description": "Transfer with insufficient balance"
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "Insufficient balance"
}
```

#### ❌ **Same Account Transfer**
```json
{
  "fromAccountId": 1,
  "toAccountId": 1,
  "amount": 100.00,
  "description": "Transfer to same account"
}
```
**Expected Response**:
```json
{
  "success": false,
  "message": "Cannot transfer to the same account"
}
```

### 5.2 Get Transfers by Account ID
**Endpoint**: `GET /api/Transfer/account/{accountId}`

**Test Cases**:

#### ✅ **Valid Account with Transfers**
- **URL**: `GET /api/Transfer/account/1`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Transfers retrieved successfully",
  "data": [
    {
      "transferId": 1,
      "transferCode": "TRF202401011400001",
      "fromAccountId": 1,
      "toAccountId": 2,
      "amount": 500.00,
      "fromCurrency": "TL",
      "toCurrency": "USD",
      "exchangeRate": 0.0308,
      "convertedAmount": 15.40,
      "status": "COMPLETED",
      "description": "Transfer to USD account",
      "transferDate": "2024-01-01T14:00:00",
      "completedDate": "2024-01-01T14:00:00"
    }
  ]
}
```

---

## 6. Exchange Rate Endpoints

### 6.1 Get Current Exchange Rates
**Endpoint**: `GET /api/ExchangeRate/current`

**Test Cases**:

#### ✅ **Get Current Rates**
- **URL**: `GET /api/ExchangeRate/current`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Exchange rates retrieved successfully",
  "data": [
    {
      "fromCurrency": "USD",
      "toCurrency": "TL",
      "rate": 32.50,
      "captureDate": "2024-01-01T00:00:00",
      "source": "INITIAL"
    },
    {
      "fromCurrency": "EUR",
      "toCurrency": "TL",
      "rate": 35.20,
      "captureDate": "2024-01-01T00:00:00",
      "source": "INITIAL"
    },
    {
      "fromCurrency": "USD",
      "toCurrency": "EUR",
      "rate": 0.92,
      "captureDate": "2024-01-01T00:00:00",
      "source": "INITIAL"
    }
  ]
}
```

### 6.2 Get Exchange Rate History
**Endpoint**: `GET /api/ExchangeRate/history/{fromCurrency}/{toCurrency}`

**Test Cases**:

#### ✅ **Valid Currency Pair**
- **URL**: `GET /api/ExchangeRate/history/USD/TL`
- **Expected Response**:
```json
{
  "success": true,
  "message": "Exchange rate history retrieved successfully",
  "data": [
    {
      "rateId": 1,
      "fromCurrency": "USD",
      "toCurrency": "TL",
      "rate": 32.50,
      "captureDate": "2024-01-01T00:00:00",
      "source": "INITIAL"
    }
  ]
}
```

#### ❌ **Invalid Currency Pair**
- **URL**: `GET /api/ExchangeRate/history/INVALID/TL`
- **Expected Response**:
```json
{
  "success": false,
  "message": "Invalid currency pair"
}
```

---

## 7. Error Handling Test Cases

### 7.1 Invalid Endpoints
- **URL**: `GET /api/InvalidEndpoint`
- **Expected Response**: 404 Not Found

### 7.2 Invalid HTTP Methods
- **URL**: `PUT /api/auth/login`
- **Expected Response**: 405 Method Not Allowed

### 7.3 Malformed JSON
```json
{
  "firstName": "Test",
  "lastName": "User",
  "tckn": "12345678901",
  "password": "123456",
  "dateOfBirth": "1995-01-15"
```
- **Expected Response**: 400 Bad Request

### 7.4 Missing Required Fields
```json
{
  "firstName": "Test",
  "lastName": "User"
}
```
- **Expected Response**: 400 Bad Request

---

## 8. Performance Test Cases

### 8.1 Concurrent Requests
- **Test**: Send 10 simultaneous login requests
- **Expected**: All requests should complete successfully

### 8.2 Large Data Sets
- **Test**: Get all customers (500+ records)
- **Expected**: Response within 2 seconds

### 8.3 Database Connection
- **Test**: Rapid successive API calls
- **Expected**: No connection pool exhaustion

---

## 9. Security Test Cases

### 9.1 SQL Injection Prevention
- **Test**: `GET /api/Customer/1'; DROP TABLE Customers; --`
- **Expected**: 400 Bad Request or 404 Not Found

### 9.2 XSS Prevention
- **Test**: Send HTML/JavaScript in input fields
- **Expected**: Input should be sanitized

### 9.3 CORS Configuration
- **Test**: Request from unauthorized origin
- **Expected**: CORS error or blocked request

---

## 10. Swagger UI Test Steps

1. **Open Swagger UI**: Navigate to `http://localhost:5115/swagger`

2. **Test Authentication**:
   - Expand `/api/auth/login`
   - Click "Try it out"
   - Enter test credentials
   - Execute and verify response

3. **Test Customer Operations**:
   - Test customer creation
   - Test customer retrieval
   - Test customer listing

4. **Test Account Operations**:
   - Test account balance retrieval
   - Test account listing by customer

5. **Test Transaction Operations**:
   - Test deposit creation
   - Test transaction history

6. **Test Transfer Operations**:
   - Test transfer creation
   - Test transfer history

7. **Test Exchange Rate Operations**:
   - Test current rates retrieval
   - Test rate history

---

## 11. Postman Collection

You can import these test cases into Postman for automated testing:

1. **Environment Variables**:
   - `baseUrl`: `http://localhost:5115`
   - `customerId`: `1`
   - `accountId`: `1`

2. **Pre-request Scripts**:
   - Set authentication headers
   - Generate dynamic test data

3. **Test Scripts**:
   - Verify response status codes
   - Validate response schemas
   - Check response times

---

## 12. Automated Testing Commands

### Using curl for API Testing:

```bash
# Test Login
curl -X POST "http://localhost:5115/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"tckn":"12345678901","password":"123456"}'

# Test Customer Creation
curl -X POST "http://localhost:5115/api/Customer" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"User","tckn":"99999999999","password":"123456","dateOfBirth":"1995-01-15"}'

# Test Account Retrieval
curl -X GET "http://localhost:5115/api/Account/customer/1"

# Test Deposit
curl -X POST "http://localhost:5115/api/Transaction/deposit" \
  -H "Content-Type: application/json" \
  -d '{"accountId":1,"amount":1000.00,"description":"Test deposit"}'
```

This comprehensive test suite covers all major functionality of the BankingApp API and ensures robust testing of the Swagger endpoints. 