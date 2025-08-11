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
  getCurrentExchangeRates(skipCache: boolean = false): Observable<ApiResponse<ExchangeRatesResponse>> {
    // Backend route token is [controller]=ExchangeRate → '/api/ExchangeRate/current'
    return this.api.get<ApiResponse<ExchangeRatesResponse>>('/ExchangeRate/current', { skipCache });
  }

  /** İki para birimi arasındaki kuru getirir. */
  getExchangeRate(fromCurrency: string, toCurrency: string, skipCache: boolean = false): Observable<ApiResponse<number>> {
    return this.api.get<ApiResponse<number>>(`/ExchangeRate/rate`, { fromCurrency, toCurrency, skipCache });
  }

  /** Kur tablolarını günceller. */
  updateExchangeRates(): Observable<ApiResponse<any>> {
    return this.api.post<ApiResponse<any>>('/ExchangeRate/update', {});
  }
}