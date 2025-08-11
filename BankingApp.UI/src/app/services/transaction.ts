import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Transaction, Deposit } from '../models/transaction.model';
import { ApiResponse } from '../models/api-response.model';

/**
 * İşlem servis katmanı: para yatırma ve geçmiş sorgulama işlemleri.
 */
@Injectable({
  providedIn: 'root'
})
export class TransactionService {

  constructor(private api: ApiService) { }

  /** Hesaba para yatırma işlemi yapar. */
  deposit(deposit: Deposit): Observable<ApiResponse<Transaction>> {
    return this.api.post<ApiResponse<Transaction>>('/Transaction/deposit', deposit);
  }

  /** Hesabın tüm işlemlerini listeler. */
  getTransactionsByAccountId(accountId: number): Observable<ApiResponse<Transaction[]>> {
    return this.api.get<ApiResponse<Transaction[]>>(`/Transaction/account/${accountId}`);
  }

  /** Belirtilen tarih aralığındaki işlemleri listeler. */
  getTransactionsByDateRange(accountId: number, startDate: string, endDate: string): Observable<ApiResponse<Transaction[]>> {
    return this.api.get<ApiResponse<Transaction[]>>(`/Transaction/account/${accountId}/date-range?startDate=${startDate}&endDate=${endDate}`);
  }

  /** İşlemleri sayfalı olarak listeler. */
  getTransactionsPaged(accountId: number, pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<any>> {
    return this.api.get<ApiResponse<any>>(`/Transaction/account/${accountId}/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }
}