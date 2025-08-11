/**
 * Müşteri ile ilgili frontend modelleri.
 */
import { Account } from './account.model';

export interface Customer {
  customerId: number;
  customerNumber: string;
  firstName: string;
  lastName: string;
  tckn: string;
  dateOfBirth: Date;
  isActive: boolean;
  createdDate: Date;
  accounts?: Account[];
}

export interface CreateCustomer {
  firstName: string;
  lastName: string;
  tckn: string;
  dateOfBirth: Date;
}

export interface CustomerSummary {
  customerId: number;
  customerNumber: string;
  fullName: string;
  isActive: boolean;
}