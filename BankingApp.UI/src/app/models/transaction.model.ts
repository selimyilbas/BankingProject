// src/app/models/transaction.model.ts
export interface Transaction {
    transactionId: number;
    transactionCode: string;
    accountNumber: string;
    transactionType: string;
    amount: number;
    currency: string;
    exchangeRate: number;
    description?: string;
    transactionDate: Date;
  }
  
  export interface Deposit {
    accountNumber: string;
    amount: number;
    description?: string;
  }