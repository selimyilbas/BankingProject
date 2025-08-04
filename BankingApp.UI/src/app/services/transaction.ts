import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Transaction, Deposit } from '../models/transaction.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {

  constructor(private api: ApiService) { }

  deposit(deposit: Deposit): Observable<ApiResponse<Transaction>> {
    return this.api.post<ApiResponse<Transaction>>('/Transaction/deposit', deposit);
  }

  getTransactionsByAccountId(accountId: number): Observable<ApiResponse<Transaction[]>> {
    return this.api.get<ApiResponse<Transaction[]>>(`/Transaction/account/${accountId}`);
  }

  getTransactionsByDateRange(accountId: number, startDate: string, endDate: string): Observable<ApiResponse<Transaction[]>> {
    return this.api.get<ApiResponse<Transaction[]>>(`/Transaction/account/${accountId}/date-range?startDate=${startDate}&endDate=${endDate}`);
  }

  getTransactionsPaged(accountId: number, pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<any>> {
    return this.api.get<ApiResponse<any>>(`/Transaction/account/${accountId}/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }
}