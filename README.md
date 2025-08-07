#  VakıfBank Banking Application

A full-stack banking application developed as an internship project, featuring real-time exchange rates, multi-currency accounts, secure transactions, and modern UI/UX design.

> 📈 **Son Güncellemeler**: Para transfer sistemi optimize edildi ve bakiye güncellemeleri gerçek zamanlı hale getirildi.

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=.net&logoColor=white)
![Angular](https://img.shields.io/badge/Angular-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

## 📋 Table of Contents

- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
- [API Documentation](#-api-documentation)
- [Database Schema](#-database-schema)
- [Exchange Rates Integration](#-exchange-rates-integration)
- [Screenshots](#-screenshots)
- [Contributing](#-contributing)

## ✨ Features

### Core Banking Features
- **👤 Customer Management**
  - Customer registration with TCKN validation
  - Profile management
  - Secure authentication

- **💳 Multi-Currency Accounts**
  - Support for TL, EUR, and USD accounts
  - Automatic account number generation
  - Real-time balance tracking

- **💰 Transactions**
  - Deposit operations
  - Money transfers between accounts
  - Automatic currency conversion
  - Transaction history

- **💱 Real-Time Exchange Rates**
  - Live rates from exchangerate-api.com
  - USD/TRY and EUR/TRY rates
  - Buy/Sell spread calculations
  - Manual refresh capability

### Technical Features
- **🔐 Security**
  - JWT-based authentication
  - CORS configuration
  - Input validation

- **🏗️ Architecture**
  - Clean Architecture implementation
  - Domain-Driven Design (DDD)
  - Repository Pattern
  - Unit of Work Pattern

- **🎨 Modern UI/UX**
  - Responsive design
  - VakıfBank branding
  - Real-time updates
  - Loading states and error handling

## 🛠️ Tech Stack

### Backend
- **Framework**: .NET 8.0
- **Architecture**: Clean Architecture
- **Database**: SQL Server 2022
- **ORM**: Entity Framework Core
- **Authentication**: JWT Bearer
- **API Documentation**: Swagger/OpenAPI

### Frontend
- **Framework**: Angular 17+
- **Components**: Standalone Components (No Modules)
- **Styling**: Custom CSS with VakıfBank theme
- **HTTP Client**: Angular HttpClient
- **Routing**: Angular Router

### Infrastructure
- **Containerization**: Docker
- **Database Management**: Docker Compose
- **External APIs**: exchangerate-api.com

## 🏛️ Architecture

```
BankingProject/
├── BankingApp/                      # Backend (.NET)
│   ├── BankingApp.API/             # Web API Layer
│   │   ├── Controllers/            # API Endpoints
│   │   ├── Program.cs             # Application Entry Point
│   │   └── appsettings.json       # Configuration
│   ├── BankingApp.Application/     # Business Logic Layer
│   │   ├── DTOs/                  # Data Transfer Objects
│   │   ├── Services/              # Business Services
│   │   └── Mappings/              # AutoMapper Profiles
│   ├── BankingApp.Domain/          # Domain Layer
│   │   ├── Entities/              # Domain Entities
│   │   └── Interfaces/            # Domain Contracts
│   ├── BankingApp.Infrastructure/  # Data Access Layer
│   │   ├── Data/                  # DbContext & Configurations
│   │   └── Repositories/          # Repository Implementations
│   └── BankingApp.Common/          # Shared Utilities
├── BankingApp.UI/                  # Frontend (Angular)
│   └── src/
│       └── app/
│           ├── components/        # UI Components
│           ├── services/          # Angular Services
│           └── models/            # TypeScript Models
├── database/                       # Database Scripts
└── docker-compose.yml             # Docker Configuration
```

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ and npm
- Docker Desktop
- SQL Server Management Studio (optional)

### 1. Clone the Repository
```bash
git clone https://github.com/selimyilbas/BankingProject.git
cd BankingProject
```

### 2. Start SQL Server Database
```bash
docker-compose up -d
```

### 3. Setup Backend

```bash
# Navigate to backend directory
cd BankingApp/BankingApp.API

# Restore packages
dotnet restore

# Run the API
dotnet run
```

The API will be available at `http://localhost:5115`

### 4. Setup Frontend

```bash
# Navigate to frontend directory
cd BankingApp.UI

# Install dependencies
npm install

# Start the development server
ng serve
```

The application will be available at `http://localhost:4200`

### 5. Default Test Users

| TCKN | Password | Name |
|------|----------|------|
| 12345678901 | 123456 | Ahmet Yılmaz |
| 98765432109 | 123456 | Ayşe Kaya |
| 11111111111 | 123456 | Test User |

## 📡 API Documentation

### Authentication
```http
POST /api/auth/login
Content-Type: application/json

{
  "tckn": "12345678901",
  "password": "123456"
}
```

### Exchange Rates
```http
GET /api/exchangerate/current
```

Response:
```json
{
  "success": true,
  "message": "Operation successful",
  "data": {
    "rates": [
      {
        "currency": "USD",
        "currencyName": "Amerikan Doları",
        "buyRate": 40.4766,
        "sellRate": 40.8834
      },
      {
        "currency": "EUR",
        "currencyName": "Euro",
        "buyRate": 46.8247,
        "sellRate": 47.2953
      }
    ],
    "lastUpdated": "2025-08-06T06:46:13.094859Z"
  }
}
```

### Create Account
```http
POST /api/account
Content-Type: application/json
Authorization: Bearer {token}

{
  "customerId": 1,
  "currency": "USD",
  "accountName": "My USD Account"
}
```

### Money Transfer
```http
POST /api/transfer
Content-Type: application/json
Authorization: Bearer {token}

{
  "fromAccountId": 1,
  "toAccountId": 2,
  "amount": 100,
  "description": "Test transfer"
}
```

## 🗄️ Database Schema

### Core Tables
- **Customers**: Customer information with TCKN
- **Accounts**: Multi-currency account details
- **Transactions**: All financial transactions
- **Transfers**: Money transfer records
- **ExchangeRateHistory**: Historical exchange rates

### Account Number Format
- TL accounts: Start with 1 (e.g., 100000000001)
- EUR accounts: Start with 2 (e.g., 200000000001)
- USD accounts: Start with 3 (e.g., 300000000001)

## 💱 Exchange Rates Integration

The application integrates with [exchangerate-api.com](https://www.exchangerate-api.com/) for real-time currency rates.

### Features
- **Real-time Updates**: Fetches live rates from API
- **Fallback Mechanism**: Uses database cache if API fails
- **Spread Calculation**: Applies 0.5% spread for buy/sell rates
- **Currency Support**: USD/TRY and EUR/TRY pairs

### Implementation Details
```csharp
// Direct API call without caching
var url = $"https://api.exchangerate-api.com/v4/latest/{currency}";
var response = await _httpClient.GetAsync(url);
// Parse and apply spread calculations
```

## 📸 Screenshots

### Dashboard
- Account overview with real-time balances
- Live exchange rates display
- Quick action buttons
- Transaction summary

### Money Transfer
- Multi-currency transfer support
- Automatic currency conversion
- Real-time rate display
- Transfer confirmation

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is developed as an internship project for VakıfBank.

## 🙏 Acknowledgments

- VakıfBank for the internship opportunity
- [exchangerate-api.com](https://www.exchangerate-api.com/) for providing free exchange rate API
- Angular and .NET communities for excellent documentation

---

**Note**: This is a test/learning project. For production use, implement proper security measures including:
- Password hashing (currently using plain text for testing)
- HTTPS enforcement
- Rate limiting
- Input sanitization
- Proper error handling
- MERNIS integration for real TCKN validation
