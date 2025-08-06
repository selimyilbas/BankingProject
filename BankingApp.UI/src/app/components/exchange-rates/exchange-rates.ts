import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExchangeRateService, ExchangeRateDisplay } from '../../services/exchange-rate.service';

@Component({
  selector: 'app-exchange-rates',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './exchange-rates.html',
  styleUrl: './exchange-rates.css'
})
export class ExchangeRatesComponent implements OnInit {
  exchangeRates: ExchangeRateDisplay[] = [];
  loading = true;
  lastUpdated: Date = new Date();

  constructor(private exchangeRateService: ExchangeRateService) {}

  ngOnInit() {
    this.loadExchangeRates();
  }

  loadExchangeRates() {
    this.loading = true;
    this.exchangeRateService.getCurrentExchangeRates()
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.exchangeRates = response.data.rates;
            this.lastUpdated = new Date(response.data.lastUpdated);
          } else {
            console.error('Failed to load exchange rates:', response.message);
            // Fallback to hardcoded rates
            this.exchangeRates = [
              { currency: 'USD', currencyName: 'Amerikan Doları', buyRate: 32.45, sellRate: 32.55 },
              { currency: 'EUR', currencyName: 'Euro', buyRate: 35.15, sellRate: 35.25 }
            ];
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading exchange rates:', error);
          // Fallback to hardcoded rates
          this.exchangeRates = [
            { currency: 'USD', currencyName: 'Amerikan Doları', buyRate: 32.45, sellRate: 32.55 },
            { currency: 'EUR', currencyName: 'Euro', buyRate: 35.15, sellRate: 35.25 }
          ];
          this.loading = false;
        }
      });
  }

  refreshRates() {
    this.loadExchangeRates();
  }
}