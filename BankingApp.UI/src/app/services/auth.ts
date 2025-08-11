import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { ApiService } from './api';

/**
 * Kimlik doğrulama servisi: giriş, kayıt, oturum yönetimi ve kullanıcı durumu.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private api: ApiService) {
    const user = localStorage.getItem('currentUser');
    if (user) {
      this.currentUserSubject.next(JSON.parse(user));
    }
  }

  /** TCKN ve şifre ile giriş yapar. */
  login(tckn: string, password: string): Observable<boolean> {
    console.log('AuthService.login called with:', { tckn, password });
    
    return this.api.post<any>('/auth/login', { tckn, password }).pipe(
      map(response => {
        console.log('API Response:', response);
        if (response && response.success && response.data) {
          // Store user details
          const user = {
            ...response.data,
            name: `${response.data.firstName} ${response.data.lastName}`
          };
          localStorage.setItem('currentUser', JSON.stringify(user));
          this.currentUserSubject.next(user);
          return true;
        }
        return false;
      }),
      catchError(error => {
        console.error('API Error:', error);
        return of(false);
      })
    );
  }

  /** Yeni kullanıcı kaydı oluşturur. */
  register(userData: {
    firstName: string;
    lastName: string;
    tckn: string;
    password: string;
    dateOfBirth: string;
    email?: string;
    phoneNumber?: string;
  }): Observable<boolean> {
    return this.api.post<any>('/auth/register', userData).pipe(
      map(response => {
        if (response.success && response.data) {
          // Store user details after successful registration
          const user = {
            ...response.data,
            name: `${response.data.firstName} ${response.data.lastName}`
          };
          localStorage.setItem('currentUser', JSON.stringify(user));
          this.currentUserSubject.next(user);
          return true;
        }
        return false;
      }),
      catchError(error => {
        console.error('Registration error:', error);
        return of(false);
      })
    );
  }

  /** Oturumu kapatır ve yerel depolamayı temizler. */
  logout(): void {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  /** Kullanıcının giriş yapıp yapmadığını döner. */
  isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  /** Mevcut kullanıcıyı döner. */
  getCurrentUser(): any {
    return this.currentUserSubject.value;
  }

  /** Kullanıcı görünen adını döner. */
  getUserDisplayName(): string {
    const user = this.getCurrentUser();
    return user ? user.name : '';
  }

  /** Kullanıcının müşteri numarasını döner. */
  getCustomerNumber(): string {
    const user = this.getCurrentUser();
    return user ? user.customerNumber : '';
  }

  /** Kullanıcının müşteri kimliğini döner. */
  getCustomerId(): number {
    const user = this.getCurrentUser();
    return user ? user.customerId : 0;
  }
}