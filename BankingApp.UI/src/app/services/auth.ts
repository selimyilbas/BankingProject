import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { ApiService } from './api';

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

  logout(): void {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  getCurrentUser(): any {
    return this.currentUserSubject.value;
  }

  // Helper method to get user display name
  getUserDisplayName(): string {
    const user = this.getCurrentUser();
    return user ? user.name : '';
  }

  // Helper method to get customer number
  getCustomerNumber(): string {
    const user = this.getCurrentUser();
    return user ? user.customerNumber : '';
  }

  // Helper method to get customer ID
  getCustomerId(): number {
    const user = this.getCurrentUser();
    return user ? user.customerId : 0;
  }
}