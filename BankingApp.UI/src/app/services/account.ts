import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Account, CreateAccount, AccountBalance } from '../models/account.model';
import { ApiResponse } from '../models/api-response.model';

/**
 * Hesap servis katmanı: hesap oluşturma, sorgulama ve durum güncelleme işlemleri.
 */
@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private api: ApiService) { }

  /** Yeni hesap oluşturur. */
  createAccount(account: CreateAccount): Observable<ApiResponse<Account>> {
    return this.api.post<ApiResponse<Account>>('/Account', account);
  }

  /** Hesabı kimliğine göre getirir. */
  getAccountById(id: number): Observable<ApiResponse<Account>> {
    return this.api.get<ApiResponse<Account>>(`/Account/${id}`);
  }

  /** Hesabı numarasına göre getirir. */
  getAccountByNumber(accountNumber: string): Observable<ApiResponse<Account>> {
    return this.api.get<ApiResponse<Account>>(`/Account/by-number/${accountNumber}`);
  }

  /** Müşteriye ait hesapları listeler. */
  getAccountsByCustomerId(customerId: number): Observable<ApiResponse<Account[]>> {
    return this.api.get<ApiResponse<Account[]>>(`/Account/customer/${customerId}`);
  }

  /** Hesap bakiyesini getirir. */
  getAccountBalance(accountNumber: string): Observable<ApiResponse<AccountBalance>> {
    return this.api.get<ApiResponse<AccountBalance>>(`/Account/balance/${accountNumber}`);
  }

  /** Hesap aktiflik durumunu günceller. */
  updateAccountStatus(accountId: number, isActive: boolean): Observable<ApiResponse<boolean>> {
    return this.api.put<ApiResponse<boolean>>(`/Account/${accountId}/status`, { isActive });
  }
}