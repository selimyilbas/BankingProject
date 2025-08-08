import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AccountService } from '../../services/account';
import { AuthService } from '../../services/auth';
import { ExchangeRateService, ExchangeRateDisplay } from '../../services/exchange-rate.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit, OnDestroy {
  currentUser: any;
  accounts: any[] = [];
  totalBalance = {
    TL: 0,
    EUR: 0,
    USD: 0
  };
  loading = true;
  exchangeRatesLoading = true;
  today = new Date();

  exchangeRates: ExchangeRateDisplay[] = [];
  exchangeRatesLastUpdated: Date = new Date();
  private refreshIntervalId: any | null = null;

  constructor(
    private authService: AuthService,
    private accountService: AccountService,
    private exchangeRateService: ExchangeRateService
  ) {
    this.currentUser = this.authService.getCurrentUser();
  }

  ngOnInit() {
    this.loadAccounts();
    this.loadExchangeRates();
    this.refreshIntervalId = setInterval(() => this.loadExchangeRates(), 5000);
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

  loadExchangeRates() {
    this.exchangeRatesLoading = true;
    this.exchangeRateService.getCurrentExchangeRates()
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.exchangeRates = response.data.rates;
            this.exchangeRatesLastUpdated = new Date(response.data.lastUpdated);
          } else {
            console.error('Failed to load exchange rates:', response.message);
            // Fallback to hardcoded rates
            this.exchangeRates = [
              { currency: 'USD', currencyName: 'Amerikan Doları', buyRate: 32.45, sellRate: 32.55 },
              { currency: 'EUR', currencyName: 'Euro', buyRate: 35.15, sellRate: 35.25 }
            ];
          }
          this.exchangeRatesLoading = false;
        },
        error: (error) => {
          console.error('Error loading exchange rates:', error);
          // Fallback to hardcoded rates
          this.exchangeRates = [
            { currency: 'USD', currencyName: 'Amerikan Doları', buyRate: 32.45, sellRate: 32.55 },
            { currency: 'EUR', currencyName: 'Euro', buyRate: 35.15, sellRate: 35.25 }
          ];
          this.exchangeRatesLoading = false;
        }
      });
  }

  refreshExchangeRates() {
    this.loadExchangeRates();
  }

  ngOnDestroy(): void {
    if (this.refreshIntervalId) {
      clearInterval(this.refreshIntervalId);
      this.refreshIntervalId = null;
    }
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