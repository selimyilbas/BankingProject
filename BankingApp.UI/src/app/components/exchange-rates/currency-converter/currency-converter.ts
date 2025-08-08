import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ExchangeRateService } from '../../../services/exchange-rate.service';

@Component({
  selector: 'app-currency-converter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './currency-converter.html',
  styleUrls: ['./currency-converter.css']
})
export class CurrencyConverterComponent {
  amount = 100;
  fromCurrency = 'TL';
  toCurrency = 'USD';
  result: number | null = null;
  rate: number | null = null;
  loading = false;
  error = '';

  readonly currencies = [
    { code: 'TL', name: 'Türk Lirası' },
    { code: 'USD', name: 'Amerikan Doları' },
    { code: 'EUR', name: 'Euro' }
  ];

  constructor(private exchangeRateService: ExchangeRateService) {}

  convert(): void {
    this.error = '';
    if (!this.amount || this.amount <= 0) {
      this.result = null;
      this.rate = null;
      return;
    }
    this.loading = true;
    this.exchangeRateService.getExchangeRate(this.fromCurrency, this.toCurrency).subscribe({
      next: (res) => {
        if (res?.success) {
          const r = (res as any).data as number;
          this.rate = r ?? 1;
          this.result = +(this.amount * (this.rate ?? 1)).toFixed(2);
        } else {
          this.error = res?.message || 'Kur alınamadı';
          this.result = null;
          this.rate = null;
        }
      },
      error: () => {
        this.error = 'Kur alınamadı';
        this.result = null;
        this.rate = null;
      },
      complete: () => (this.loading = false)
    });
  }

  swap(): void {
    const tmp = this.fromCurrency;
    this.fromCurrency = this.toCurrency;
    this.toCurrency = tmp;
    this.convert();
  }
}


