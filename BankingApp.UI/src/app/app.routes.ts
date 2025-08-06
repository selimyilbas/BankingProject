// src/app/app.routes.ts
import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login';
import { RegisterComponent } from './components/auth/register/register';
import { DashboardComponent } from './components/dashboard/dashboard';
import { TransferComponent } from './components/transfer/transfer.component';
import { AccountCreateComponent } from './components/account/account-create/account-create';
import { DepositComponent } from './components/transaction/deposit/deposit';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'dashboard', component: DashboardComponent },
  { path: 'account/create', component: AccountCreateComponent },
  { path: 'deposit', component: DepositComponent },
  { path: 'transfer', component: TransferComponent },
  { path: '**', redirectTo: '/login' }
];