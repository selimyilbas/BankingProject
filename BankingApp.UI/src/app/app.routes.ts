// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard';
import { CustomerListComponent } from './components/customer/customer-list/customer-list';
import { CustomerDetailComponent } from './components/customer/customer-detail/customer-detail';
import { AccountListComponent } from './components/account/account-list/account-list';
import { AccountCreateComponent } from './components/account/account-create/account-create';
import { DepositComponent } from './components/transaction/deposit/deposit';
import { TransactionHistoryComponent } from './components/transaction/transaction-history/transaction-history';
import { TransferCreateComponent } from './components/transfer/transfer-create/transfer-create';
import { TransferHistoryComponent } from './components/transfer/transfer-history/transfer-history';
import { ExchangeRatesComponent } from './components/exchange-rates/exchange-rates';
import { RegisterComponent } from './components/auth/register/register';
import { LoginComponent } from './components/auth/login/login';

export const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'customers', component: CustomerListComponent },
  { path: 'customer/:id', component: CustomerDetailComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'accounts', component: AccountListComponent },
  { path: 'account/create', component: AccountCreateComponent },
  { path: 'deposit', component: DepositComponent },
  { path: 'transactions', component: TransactionHistoryComponent },
  { path: 'transfer', component: TransferCreateComponent },
  { path: 'transfers', component: TransferHistoryComponent },
  { path: 'exchange-rates', component: ExchangeRatesComponent }
];