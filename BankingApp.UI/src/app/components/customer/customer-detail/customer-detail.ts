import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [CommonModule],
  template: '<div class="container"><h2>Müşteri Detayı</h2><p>Bu sayfa yakında eklenecek...</p></div>',
  styles: ['.container { padding: 20px; }']
})
export class CustomerDetailComponent {}