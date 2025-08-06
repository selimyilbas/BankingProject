import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AccountService } from '../../../services/account';
import { AuthService } from '../../../services/auth';
import { ApiResponse } from '../../../models/api-response.model';

@Component({
  selector: 'app-account-create',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './account-create.html',
  styleUrl: './account-create.css'
})
export class AccountCreateComponent {
  currencies = [
    { code: 'TRY', name: 'Türk Lirası' },
    { code: 'USD', name: 'Amerikan Doları' },
    { code: 'EUR', name: 'Euro' }
  ];

  selectedCurrency = 'TRY';
  loading = false;
  error = '';
  success = false;

  constructor(
    private accountService: AccountService,
    private authService: AuthService,
    public router: Router
  ) {}

  createAccount() {
    this.error = '';
    this.success = false;
    const user = this.authService.getCurrentUser();
    if (!user) {
      this.error = 'Oturum açmanız gerekiyor.';
      return;
    }

    this.loading = true;
    this.accountService
      .createAccount({ customerId: user.customerId, currency: this.selectedCurrency })
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success) {
            this.success = true;
            // Redirect back to dashboard after short delay
            setTimeout(() => this.router.navigate(['/dashboard']), 1500);
          } else {
            this.error = response.message || 'Hesap oluşturulamadı';
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err?.error?.message || 'Hesap oluşturulurken bir hata oluştu';
          this.loading = false;
        }
      });
  }
}