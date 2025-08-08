import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Transfer, CreateTransfer } from '../models/transfer.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class TransferService {

  constructor(private api: ApiService) { }

  createTransfer(transfer: CreateTransfer): Observable<ApiResponse<Transfer>> {
    return this.api.post<ApiResponse<Transfer>>('/Transfer', transfer);
  }

  getTransferById(id: number): Observable<ApiResponse<Transfer>> {
    return this.api.get<ApiResponse<Transfer>>(`/Transfer/${id}`);
  }

  getTransfersByAccountId(accountId: number): Observable<ApiResponse<Transfer[]>> {
    return this.api.get<ApiResponse<Transfer[]>>(`/Transfer/account/${accountId}`);
  }

  getTransfersPaged(accountId: number, pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<any>> {
    return this.api.get<ApiResponse<any>>(`/Transfer/account/${accountId}/paged?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  // New endpoints for cross-customer transfers by account number
  createTransferByAccountNumber(transfer: CreateTransfer): Observable<ApiResponse<Transfer>> {
    return this.api.post<ApiResponse<Transfer>>('/transfer/by-account-number', transfer);
  }

  validateTransferByAccountNumber(transfer: CreateTransfer): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>('/transfer/validate/by-account-number', transfer);
  }
}