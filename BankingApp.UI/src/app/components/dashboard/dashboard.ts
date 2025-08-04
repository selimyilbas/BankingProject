import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AccountService } from '../../services/account';
import { AuthService } from '../../services/auth';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  currentUser: any;
  accounts: any[] = [];
  totalBalance = {
    TL: 0,
    EUR: 0,
    USD: 0
  };
  loading = true;
  today = new Date();

  exchangeRates = {
    USD: { buy: 32.45, sell: 32.55 },
    EUR: { buy: 35.15, sell: 35.25 }
  };

  constructor(
    private authService: AuthService,
    private accountService: AccountService
  ) {
    this.currentUser = this.authService.getCurrentUser();
  }

  ngOnInit() {
    this.loadAccounts();
  }

  loadAccounts() {
    if (this.currentUser && this.currentUser.customerId) {
      this.accountService.getAccountsByCustomerId(this.currentUser.customerId)
        .subscribe({
          next: (response) => {
            if (response.success) {
              this.accounts = response.data || [];
              this.calculateTotalBalance();
            }
            this.loading = false;
          },
          error: (error) => {
            console.error('Error loading accounts:', error);
            this.loading = false;
          }
        });
    }
  }

  calculateTotalBalance() {
    this.totalBalance = { TL: 0, EUR: 0, USD: 0 };
    this.accounts.forEach(account => {
      this.totalBalance[account.currency as keyof typeof this.totalBalance] += account.balance;
    });
  }

  getCurrencyIcon(currency: string): string {
    const icons: { [key: string]: string } = {
      'TL': '₺',
      'EUR': '€',
      'USD': '$'
    };
    return icons[currency] || currency;
  }
}