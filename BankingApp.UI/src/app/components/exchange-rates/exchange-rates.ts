import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExchangeRateService, ExchangeRateDisplay } from '../../services/exchange-rate.service';

@Component({
  selector: 'app-exchange-rates',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './exchange-rates.html',
  styleUrl: './exchange-rates.css'
})
export class ExchangeRatesComponent implements OnInit, OnDestroy {
  exchangeRates: (ExchangeRateDisplay & { buyDelta?: number; sellDelta?: number; buyDeltaPct?: number; sellDeltaPct?: number })[] = [];
  loading = true;
  lastUpdated: Date = new Date();
  private previousMap = new Map<string, { buy: number; sell: number }>();
  // Otomatik yenileme kaldırıldı; manuel yenileme butonu ile istek atılacak
  private refreshIntervalId: any | null = null;

  constructor(private exchangeRateService: ExchangeRateService) {}

  ngOnInit() {
    this.loadExchangeRates();
  }

  loadExchangeRates(skipCache: boolean = false) {
    this.loading = true;
    this.exchangeRateService.getCurrentExchangeRates(skipCache)
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
            // Canlı veri gelmezse loading'de kalmaya devam et
            return;
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading exchange rates:', error);
          // Canlı veri gelmezse loading'de kalmaya devam et
          return;
        }
      });
  }

  refreshRates() {
    this.loadExchangeRates(true);
  }

  ngOnDestroy(): void {
    if (this.refreshIntervalId) {
      clearInterval(this.refreshIntervalId);
      this.refreshIntervalId = null;
    }
  }
}