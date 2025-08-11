import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-crypto-rates',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './crypto-rates.html',
  styleUrl: './crypto-rates.css'
})
export class CryptoRatesComponent implements OnInit {
  loading = true;
  topCrypto: { symbol: string; lastPrice: number; priceChangePercent: number }[] = [];

  async ngOnInit() {
    await this.loadTopCrypto();
  }

  async loadTopCrypto() {
    try {
      this.loading = true;
      const symbols = [
        'BTCUSDT','ETHUSDT','BNBUSDT','XRPUSDT','SOLUSDT','ADAUSDT','DOGEUSDT','TRXUSDT','AVAXUSDT','DOTUSDT','MATICUSDT','LTCUSDT','LINKUSDT','BCHUSDT','ATOMUSDT'
      ];
      const url = 'https://api.binance.com/api/v3/ticker/24hr?symbols=' + encodeURIComponent(JSON.stringify(symbols)) + '&type=MINI';
      const resp = await fetch(url);
      if (!resp.ok) throw new Error('Binance 24hr fetch failed');
      const json = await resp.json() as any[];
      this.topCrypto = json.map(j => ({
        symbol: j.symbol,
        lastPrice: parseFloat(j.lastPrice),
        priceChangePercent: parseFloat(j.priceChangePercent)
      }));
      this.loading = false;
    } catch (e) {
      console.error('Error loading top crypto:', e);
      this.topCrypto = [];
      this.loading = false;
    }
  }
}


