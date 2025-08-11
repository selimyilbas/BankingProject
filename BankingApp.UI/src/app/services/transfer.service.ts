import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CreateTransferDto {
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  description: string;
}

export interface CreateTransferByAccountNumberDto {
  fromAccountNumber: string;
  toAccountNumber: string;
  amount: number;
  description?: string;
}

export interface TransferDto {
  transferId: number;
  transferCode: string;
  fromAccountId: number;
  toAccountId: number;
  fromAccountNumber: string;
  toAccountNumber: string;
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

/**
 * Transfer servisi: transfer oluşturma, doğrulama ve geçmiş sorgulama işlemleri.
 */
@Injectable({
  providedIn: 'root'
})
export class TransferService {
  constructor(private api: ApiService) {}

  /** Hesap kimlikleri ile transfer oluşturur. */
  createTransfer(transferDto: CreateTransferDto): Observable<ApiResponse<TransferDto>> {
    return this.api.post<ApiResponse<TransferDto>>('/transfer', transferDto);
  }

  /** Hesap numaraları ile transfer oluşturur. */
  createTransferByAccountNumber(dto: CreateTransferByAccountNumberDto): Observable<ApiResponse<TransferDto>> {
    return this.api.post<ApiResponse<TransferDto>>('/transfer/by-account-number', dto);
  }

  /** Transfer kimliğine göre transfer getirir. */
  getTransferById(transferId: number): Observable<ApiResponse<TransferDto>> {
    return this.api.get<ApiResponse<TransferDto>>(`/Transfer/${transferId}`);
  }

  /** Hesaba ait transferleri listeler. */
  getTransfersByAccount(accountId: number): Observable<ApiResponse<TransferDto[]>> {
    return this.api.get<ApiResponse<TransferDto[]>>(`/transfer/account/${accountId}`);
  }

  /** Müşteriye ait transferleri listeler. */
  getTransfersByCustomer(customerId: number): Observable<ApiResponse<TransferDto[]>> {
    return this.api.get<ApiResponse<TransferDto[]>>(`/transfer/customer/${customerId}`);
  }

  /** Transfer kurallarını önceden doğrular. */
  validateTransfer(transferDto: CreateTransferDto): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>('/transfer/validate', transferDto);
  }

  /** Hesap numaraları ile transfer doğrulaması yapar. */
  validateTransferByAccountNumber(dto: CreateTransferByAccountNumberDto): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>('/transfer/validate/by-account-number', dto);
  }
} 