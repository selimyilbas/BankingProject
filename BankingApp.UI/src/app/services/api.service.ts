import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * API istemcisi: Backend'e HTTP isteklerini yöneten yardımcı servis.
 * - Temel URL: http://localhost:5115/api
 * - get/post/put/delete/patch kısayolları sağlar.
 */
@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'http://localhost:5115/api';

  constructor(private http: HttpClient) {}

  /** Belirtilen endpoint'e GET isteği gönderir. */
  get<T>(endpoint: string, params?: any): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        httpParams = httpParams.set(key, params[key]);
      });
    }
    return this.http.get<T>(`${this.baseUrl}${endpoint}`, { params: httpParams });
  }

  /** Belirtilen endpoint'e POST isteği gönderir. */
  post<T>(endpoint: string, data: any): Observable<T> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post<T>(`${this.baseUrl}${endpoint}`, data, { headers });
  }

  /** Belirtilen endpoint'e PUT isteği gönderir. */
  put<T>(endpoint: string, data: any): Observable<T> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.put<T>(`${this.baseUrl}${endpoint}`, data, { headers });
  }

  /** Belirtilen endpoint'e DELETE isteği gönderir. */
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${endpoint}`);
  }

  /** Belirtilen endpoint'e PATCH isteği gönderir. */
  patch<T>(endpoint: string, data: any): Observable<T> {
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.patch<T>(`${this.baseUrl}${endpoint}`, data, { headers });
  }
} 