import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Account {
  accountId: number;
  accountNumber: string;
  customerId: number;
  currency: string;
  balance: number;
  isActive: boolean;
  createdDate: string;
  updatedDate?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AccountService {
  constructor(private api: ApiService) {}

  getAccountsByCustomer(customerId: number): Observable<ApiResponse<Account[]>> {
    return this.api.get<ApiResponse<Account[]>>(`/Account/customer/${customerId}`);
  }

  getAccountById(accountId: number): Observable<ApiResponse<Account>> {
    return this.api.get<ApiResponse<Account>>(`/Account/${accountId}`);
  }

  getAccountByNumber(accountNumber: string): Observable<ApiResponse<Account>> {
    return this.api.get<ApiResponse<Account>>(`/Account/number/${accountNumber}`);
  }

  getAccountBalance(accountId: number): Observable<ApiResponse<{ balance: number }>> {
    return this.api.get<ApiResponse<{ balance: number }>>(`/Account/${accountId}/balance`);
  }
} 