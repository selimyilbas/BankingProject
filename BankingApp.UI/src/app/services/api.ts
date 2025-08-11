import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

/**
 * Basit HTTP istemcisi: Backend API çağrıları için yardımcı servis.
 */
@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private baseUrl = 'http://localhost:5115/api';

  constructor(private http: HttpClient) { }

  /** Varsayılan JSON başlıklarını döner. */
  private getHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    });
  }

  /** GET isteği gönderir. */
  get<T>(endpoint: string): Observable<T> {
    return this.http.get<T>(`${this.baseUrl}${endpoint}`, { headers: this.getHeaders() });
  }

  /** POST isteği gönderir. */
  post<T>(endpoint: string, data: any): Observable<T> {
    return this.http.post<T>(`${this.baseUrl}${endpoint}`, data, { headers: this.getHeaders() });
  }

  /** PUT isteği gönderir. */
  put<T>(endpoint: string, data: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}${endpoint}`, data, { headers: this.getHeaders() });
  }

  /** DELETE isteği gönderir. */
  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(`${this.baseUrl}${endpoint}`, { headers: this.getHeaders() });
  }
}