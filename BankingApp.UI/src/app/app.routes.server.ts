import { Routes } from '@angular/router';

export const routes: Routes = [
  { 
    path: '', 
    redirectTo: '/dashboard', 
    pathMatch: 'full' 
  },
  { 
    path: 'dashboard', 
    loadComponent: () => import('./components/dashboard/dashboard').then(m => m.DashboardComponent)
  },
  { 
    path: 'customers', 
    loadComponent: () => import('./components/customer/customer-list/customer-list').then(m => m.CustomerListComponent)
  },
  { 
    path: 'customer/:id', 
    loadComponent: () => import('./components/customer/customer-detail/customer-detail').then(m => m.CustomerDetailComponent)
  },
  { 
    path: 'register', 
    loadComponent: () => import('./components/auth/register/register').then(m => m.RegisterComponent)
  },
  { 
    path: 'accounts', 
    loadComponent: () => import('./components/account/account-list/account-list').then(m => m.AccountListComponent)
  },
  { 
    path: 'account/create', 
    loadComponent: () => import('./components/account/account-create/account-create').then(m => m.AccountCreateComponent)
  },
  { 
    path: 'deposit', 
    loadComponent: () => import('./components/transaction/deposit/deposit').then(m => m.DepositComponent)
  },
  { 
    path: 'transactions', 
    loadComponent: () => import('./components/transaction/transaction-history/transaction-history').then(m => m.TransactionHistoryComponent)
  },
  { 
    path: 'transfer', 
    loadComponent: () => import('./components/transfer/transfer-create/transfer-create').then(m => m.TransferCreateComponent)
  },
  { 
    path: 'transfers', 
    loadComponent: () => import('./components/transfer/transfer-history/transfer-history').then(m => m.TransferHistoryComponent)
  },
  { 
    path: 'exchange-rates', 
    loadComponent: () => import('./components/exchange-rates/exchange-rates').then(m => m.ExchangeRatesComponent)
  }
];