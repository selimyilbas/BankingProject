import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AccountService, Account as ServiceAccount } from '../../services/account.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-accounts',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './accounts.html',
  styleUrl: './accounts.css'
})
export class AccountsComponent implements OnInit {
  accounts: ServiceAccount[] = [];
  loading = true;
  error = '';
  totalBalances = {
    TL: 0,
    USD: 0,
    EUR: 0
  };

  constructor(
    private accountService: AccountService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadAccounts();
  }

  loadAccounts(): void {
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser) {
      this.error = 'Kullanıcı bilgisi bulunamadı';
      this.loading = false;
      return;
    }
    
    const customerId = currentUser.customerId;
    
    this.accountService.getAccountsByCustomer(customerId).subscribe({
      next: (response) => {
        if (response.success) {
          this.accounts = response.data;
          this.calculateTotalBalances();
        } else {
          this.error = response.message || 'Hesaplar yüklenirken hata oluştu';
        }
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error loading accounts:', err);
        this.error = 'Hesaplar yüklenirken hata oluştu';
        this.loading = false;
      }
    });
  }

  calculateTotalBalances(): void {
    this.totalBalances = {
      TL: 0,
      USD: 0,
      EUR: 0
    };

    this.accounts.forEach(account => {
      if (account.currency === 'TL') {
        this.totalBalances.TL += account.balance;
      } else if (account.currency === 'USD') {
        this.totalBalances.USD += account.balance;
      } else if (account.currency === 'EUR') {
        this.totalBalances.EUR += account.balance;
      }
    });
  }

  getCurrencyIcon(currency: string): string {
    switch (currency) {
      case 'TL':
        return '₺';
      case 'USD':
        return '$';
      case 'EUR':
        return '€';
      default:
        return currency;
    }
  }

  getCurrencyClass(currency: string): string {
    switch (currency) {
      case 'TL':
        return 'currency-tl';
      case 'USD':
        return 'currency-usd';
      case 'EUR':
        return 'currency-eur';
      default:
        return '';
    }
  }

  getAccountTypeText(currency: string): string {
    switch (currency) {
      case 'TL':
        return 'TL Hesabı';
      case 'USD':
        return 'Döviz Hesabı (USD)';
      case 'EUR':
        return 'Döviz Hesabı (EUR)';
      default:
        return 'Hesap';
    }
  }

  formatCurrency(amount: number, currency: string): string {
    return new Intl.NumberFormat('tr-TR', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount);
  }

  refreshAccounts(): void {
    this.loading = true;
    this.error = '';
    this.loadAccounts();
  }

  // Navigation methods
  navigateToDeposit(accountId: number): void {
    this.router.navigate(['/deposit'], { queryParams: { accountId: accountId } });
  }

  navigateToTransfer(accountId: number): void {
    this.router.navigate(['/transfer'], { queryParams: { fromAccountId: accountId } });
  }

  navigateToTransactions(accountId: number): void {
    this.router.navigate(['/transaction-history'], { queryParams: { accountId: accountId } });
  }
}