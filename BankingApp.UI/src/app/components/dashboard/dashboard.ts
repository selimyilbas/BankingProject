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
  // Otomatik yenileme kaldırıldı; manuel butonla yenilenecek
  private refreshIntervalId: any | null = null;

  // Kripto (test amaçlı, yüksek oynaklık): Binance public endpointlerinden çekilecek
  volatileSymbol: string = 'PEPEUSDT';
  volatilePrice: number | null = null;
  volatileChangePercent: number | null = null;
  volatileLoading: boolean = true;
  volatileError: string | null = null;

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
    // Otomatik periyodik istek kaldırıldı
    this.loadVolatileCoin();
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

  loadExchangeRates(skipCache: boolean = false) {
    this.exchangeRatesLoading = true;
    this.exchangeRateService.getCurrentExchangeRates(skipCache)
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.exchangeRates = response.data.rates;
            this.exchangeRatesLastUpdated = new Date(response.data.lastUpdated);
          } else {
            console.error('Failed to load exchange rates:', response.message);
            // Canlı veri gelmezse loading'de kalmaya devam et
            return;
          }
          this.exchangeRatesLoading = false;
        },
        error: (error) => {
          console.error('Error loading exchange rates:', error);
          // Canlı veri gelmezse loading'de kalmaya devam et
          return;
        }
      });
  }

  refreshExchangeRates() {
    this.loadExchangeRates(true);
  }

  // Kripto test datası: Binance'ten PEPEUSDT fiyat ve 24s değişim
  loadVolatileCoin() {
    this.volatileLoading = true;
    this.volatileError = null;
    const priceUrl = `https://api.binance.com/api/v3/ticker/price?symbol=${this.volatileSymbol}`;
    const statsUrl = `https://api.binance.com/api/v3/ticker/24hr?symbol=${this.volatileSymbol}&type=MINI`;

    Promise.all([
      fetch(priceUrl).then(r => r.ok ? r.json() : Promise.reject(r.statusText)),
      fetch(statsUrl).then(r => r.ok ? r.json() : Promise.reject(r.statusText))
    ]).then(([priceJson, statsJson]) => {
      const priceStr = priceJson?.price as string | undefined;
      const changePctStr = (statsJson?.priceChangePercent ?? statsJson?.priceChangePercent) as string | undefined;
      const parsedPrice = priceStr ? parseFloat(priceStr) : NaN;
      const parsedChange = changePctStr ? parseFloat(changePctStr) : NaN;
      this.volatilePrice = Number.isFinite(parsedPrice) ? parsedPrice : null;
      this.volatileChangePercent = Number.isFinite(parsedChange) ? parsedChange : null;
      this.volatileLoading = false;
    }).catch(err => {
      console.error('Binance volatile fetch error:', err);
      this.volatileError = 'Kripto verisi alınamadı';
      this.volatileLoading = false;
    });
  }

  refreshVolatile() {
    this.loadVolatileCoin();
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