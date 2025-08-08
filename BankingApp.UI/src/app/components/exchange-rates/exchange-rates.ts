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
  exchangeRates: (ExchangeRateDisplay & { buyDelta?: number; sellDelta?: number; buyDeltaPct?: number; sellDeltaPct?: number })[] = [];
  loading = true;
  lastUpdated: Date = new Date();
  private previousMap = new Map<string, { buy: number; sell: number }>();

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
            const incoming = response.data.rates;
            const decorated = incoming.map(r => {
              const prev = this.previousMap.get(r.currency);
              const buyDelta = prev ? +(r.buyRate - prev.buy).toFixed(4) : undefined;
              const sellDelta = prev ? +(r.sellRate - prev.sell).toFixed(4) : undefined;
              const buyDeltaPct = prev && prev.buy !== 0 ? +(((r.buyRate - prev.buy) / prev.buy) * 100).toFixed(2) : undefined;
              const sellDeltaPct = prev && prev.sell !== 0 ? +(((r.sellRate - prev.sell) / prev.sell) * 100).toFixed(2) : undefined;
              return { ...r, buyDelta, sellDelta, buyDeltaPct, sellDeltaPct };
            });
            // Update previous snapshot
            this.previousMap.clear();
            for (const r of incoming) {
              this.previousMap.set(r.currency, { buy: r.buyRate, sell: r.sellRate });
            }
            this.exchangeRates = decorated;
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