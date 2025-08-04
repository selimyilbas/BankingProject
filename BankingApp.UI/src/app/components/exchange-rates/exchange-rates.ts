import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-exchange-rates',
  standalone: true,
  imports: [CommonModule],
  template: '<div class="container"><h2>Döviz Kurları</h2><p>Bu sayfa yakında eklenecek...</p></div>',
  styles: ['.container { padding: 20px; }']
})
export class ExchangeRatesComponent {}