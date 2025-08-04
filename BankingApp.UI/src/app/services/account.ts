import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Account, CreateAccount, AccountBalance } from '../models/account.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private api: ApiService) { }

  createAccount(account: CreateAccount): Observable<ApiResponse<Account>> {
    return this.api.post<ApiResponse<Account>>('/Account', account);
  }

  getAccountById(id: number): Observable<ApiResponse<Account>> {
    return this.api.get<ApiResponse<Account>>(`/Account/${id}`);
  }

  getAccountByNumber(accountNumber: string): Observable<ApiResponse<Account>> {
    return this.api.get<ApiResponse<Account>>(`/Account/by-number/${accountNumber}`);
  }

  getAccountsByCustomerId(customerId: number): Observable<ApiResponse<Account[]>> {
    return this.api.get<ApiResponse<Account[]>>(`/Account/customer/${customerId}`);
  }

  getAccountBalance(accountNumber: string): Observable<ApiResponse<AccountBalance>> {
    return this.api.get<ApiResponse<AccountBalance>>(`/Account/balance/${accountNumber}`);
  }

  updateAccountStatus(accountId: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.api.put<ApiResponse<boolean>>(`/Account/${accountId}/status`, { isActive });
  }
}