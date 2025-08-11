import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../models/api-response.model';

export interface ExchangeRateDisplay {
  currency: string;
  currencyName: string;
  buyRate: number;
  sellRate: number;
}

export interface ExchangeRatesResponse {
  rates: ExchangeRateDisplay[];
  lastUpdated: string;
}

/**
 * Döviz kuru servisi: anlık kur sorgulama ve güncelleme işlemleri.
 */
@Injectable({
  providedIn: 'root'
})
export class ExchangeRateService {

  constructor(private api: ApiService) {}

  /** Güncel döviz kurlarını getirir. */
  getCurrentExchangeRates(): Observable<ApiResponse<ExchangeRatesResponse>> {
    return this.api.get<ApiResponse<ExchangeRatesResponse>>('/exchangerate/current');
  }

  /** İki para birimi arasındaki kuru getirir. */
  getExchangeRate(fromCurrency: string, toCurrency: string): Observable<ApiResponse<number>> {
    return this.api.get<ApiResponse<number>>(`/exchangerate/rate?fromCurrency=${fromCurrency}&toCurrency=${toCurrency}`);
  }

  /** Kur tablolarını günceller. */
  updateExchangeRates(): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>('/exchangerate/update', {});
  }
}