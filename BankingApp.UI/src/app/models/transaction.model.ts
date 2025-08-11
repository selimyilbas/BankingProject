/**
 * İşlem (transaction) ile ilgili frontend modelleri.
 */
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