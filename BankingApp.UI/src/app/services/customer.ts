import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api';
import { Customer, CreateCustomer } from '../models/customer.model';
import { ApiResponse } from '../models/api-response.model';

/**
 * Müşteri servis katmanı: müşteri oluşturma, sorgulama ve güncelleme işlemleri.
 */
@Injectable({
  providedIn: 'root'
})
export class CustomerService {

  constructor(private api: ApiService) { }

  /** Yeni müşteri oluşturur. */
  createCustomer(customer: CreateCustomer): Observable<ApiResponse<Customer>> {
    return this.api.post<ApiResponse<Customer>>('/Customer', customer);
  }

  /** Müşteriyi kimliğine göre getirir. */
  getCustomerById(id: number): Observable<ApiResponse<Customer>> {
    return this.api.get<ApiResponse<Customer>>(`/Customer/${id}`);
  }

  /** Müşteriyi müşteri numarasına göre getirir. */
  getCustomerByNumber(customerNumber: string): Observable<ApiResponse<Customer>> {
    return this.api.get<ApiResponse<Customer>>(`/Customer/by-number/${customerNumber}`);
  }

  /** Müşteriyi TCKN'ye göre getirir. */
  getCustomerByTCKN(tckn: string): Observable<ApiResponse<Customer>> {
    return this.api.get<ApiResponse<Customer>>(`/Customer/by-tckn/${tckn}`);
  }

  /** Müşteriyi hesaplarıyla birlikte getirir. */
  getCustomerWithAccounts(id: number): Observable<ApiResponse<Customer>> {
    return this.api.get<ApiResponse<Customer>>(`/Customer/${id}/with-accounts`);
  }

  /** Müşteri listesini sayfalı olarak döner. */
  getAllCustomers(pageNumber: number = 1, pageSize: number = 10): Observable<ApiResponse<any>> {
    return this.api.get<ApiResponse<any>>(`/Customer?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  /** Müşteri bilgilerini günceller. */
  updateCustomer(id: number, dto: {
    firstName: string;
    lastName: string;
    email?: string;
    phoneNumber?: string;
    dateOfBirth: string | Date;
    isActive?: boolean;
  }): Observable<ApiResponse<Customer>> {
    return this.api.put<ApiResponse<Customer>>(`/Customer/${id}`, dto);
  }

  /** Müşteri şifresini değiştirir. */
  changePassword(id: number, currentPassword: string, newPassword: string): Observable<ApiResponse<boolean>> {
    return this.api.post<ApiResponse<boolean>>(`/Customer/${id}/change-password`, { currentPassword, newPassword });
  }
}