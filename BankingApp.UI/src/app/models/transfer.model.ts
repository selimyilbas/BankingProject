/**
 * Transfer ile ilgili frontend modelleri.
 */
export interface Transfer {
    transferId: number;
    transferCode: string;
    fromAccountNumber: string;
    toAccountNumber: string;
    amount: number;
    fromCurrency: string;
    toCurrency: string;
    exchangeRate?: number;
    convertedAmount: number;
    status: string;
    description?: string;
    transferDate: Date;
  }
  
  export interface CreateTransfer {
    fromAccountNumber: string;
    toAccountNumber: string;
    amount: number;
    description?: string;
  }