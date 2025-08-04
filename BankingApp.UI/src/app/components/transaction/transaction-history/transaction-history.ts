import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-transaction-history',
  standalone: true,
  imports: [CommonModule],
  template: '<div class="container"><h2>İşlem Geçmişi</h2><p>Bu sayfa yakında eklenecek...</p></div>',
  styles: ['.container { padding: 20px; }']
})
export class TransactionHistoryComponent {}