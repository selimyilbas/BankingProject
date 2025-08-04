// src/app/models/account.model.ts
export interface Account {
    accountId: number;
    accountNumber: string;
    customerId: number;
    customerName: string;
    currency: string;
    balance: number;
    isActive: boolean;
    createdDate: Date;
  }
  
  export interface CreateAccount {
    customerId: number;
    currency: string;
  }
  
  export interface AccountBalance {
    accountNumber: string;
    balance: number;
    currency: string;
  }