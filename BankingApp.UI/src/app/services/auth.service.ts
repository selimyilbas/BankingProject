import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface LoginRequest {
  tckn: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  tckn: string;
  password: string;
  dateOfBirth: string;
  email?: string;
  phoneNumber?: string;
}

export interface User {
  customerId: number;
  customerNumber: string;
  firstName: string;
  lastName: string;
  tckn: string;
  dateOfBirth: string;
  email?: string;
  phoneNumber?: string;
  isActive: boolean;
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
export class AuthService {
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private api: ApiService) {
    this.loadUserFromStorage();
  }

  login(request: LoginRequest): Observable<ApiResponse<User>> {
    return this.api.post<ApiResponse<User>>('/Auth/login', request);
  }

  register(request: RegisterRequest): Observable<ApiResponse<User>> {
    return this.api.post<ApiResponse<User>>('/Auth/register', request);
  }

  logout(): void {
    localStorage.removeItem('currentUser');
    this.currentUserSubject.next(null);
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isLoggedIn(): boolean {
    return this.currentUserSubject.value !== null;
  }

  private loadUserFromStorage(): void {
    const userStr = localStorage.getItem('currentUser');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);
        this.currentUserSubject.next(user);
      } catch (error) {
        console.error('Error loading user from storage:', error);
        localStorage.removeItem('currentUser');
      }
    }
  }

  setCurrentUser(user: User): void {
    localStorage.setItem('currentUser', JSON.stringify(user));
    this.currentUserSubject.next(user);
  }
} 