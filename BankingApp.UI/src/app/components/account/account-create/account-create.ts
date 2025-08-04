import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-account-create',
  standalone: true,
  imports: [CommonModule],
  template: '<div class="container"><h2>Yeni Hesap Aç</h2><p>Bu sayfa yakında eklenecek...</p></div>',
  styles: ['.container { padding: 20px; }']
})
export class AccountCreateComponent {}