import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CreateTransferDto {
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  description: string;
}

export interface TransferDto {
  transferId: number;
  transferCode: string;
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  fromCurrency: string;
  toCurrency: string;
  exchangeRate: number;
  convertedAmount: number;
  status: string;
  description: string;
  transferDate: string;
  completedDate?: string;
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
export class TransferService {
  constructor(private api: ApiService) {}

  createTransfer(transferDto: CreateTransferDto): Observable<ApiResponse<TransferDto>> {
    return this.api.post<ApiResponse<TransferDto>>('/Transfer', transferDto);
  }

  getTransferById(transferId: number): Observable<ApiResponse<TransferDto>> {
    return this.api.get<ApiResponse<TransferDto>>(`/Transfer/${transferId}`);
  }

  getTransfersByAccount(accountId: number): Observable<ApiResponse<TransferDto[]>> {
    return this.api.get<ApiResponse<TransferDto[]>>(`/Transfer/account/${accountId}`);
  }

  getTransfersByCustomer(customerId: number): Observable<ApiResponse<TransferDto[]>> {
    return this.api.get<ApiResponse<TransferDto[]>>(`/Transfer/customer/${customerId}`);
  }

  validateTransfer(transferDto: CreateTransferDto): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>('/Transfer/validate', transferDto);
  }
} 