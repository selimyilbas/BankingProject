import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AccountService } from '../../../services/account';
import { TransactionService } from '../../../services/transaction';
import { AuthService } from '../../../services/auth';
import { ApiResponse } from '../../../models/api-response.model';
import { Account } from '../../../models/account.model';

@Component({
  selector: 'app-deposit',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './deposit.html',
  styleUrl: './deposit.css'
})
export class DepositComponent implements OnInit {
  accounts: Account[] = [];
  selectedAccountNumber: string = '';
  amount: number | null = null;
  description: string = '';
  loading = false;
  error = '';
  success = false;

  constructor(
    private accountService: AccountService,
    private transactionService: TransactionService,
    private authService: AuthService,
    public router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }
    
    // Get accountId from query parameters
    const accountId = this.route.snapshot.queryParams['accountId'];
    
    this.accountService
      .getAccountsByCustomerId(user.customerId)
      .subscribe((res) => {
        if (res.success) {
          this.accounts = res.data ?? [];
          
          // Pre-select account if accountId was provided
          if (accountId && this.accounts.length > 0) {
            const selectedAccount = this.accounts.find(account => account.accountId.toString() === accountId);
            if (selectedAccount) {
              this.selectedAccountNumber = selectedAccount.accountNumber;
            }
          }
        }
      });
  }

  deposit() {
    if (!this.selectedAccountNumber || !this.amount || this.amount <= 0) {
      this.error = 'Lütfen hesap ve tutar seçiniz';
      return;
    }

    this.loading = true;
    this.error = '';
    this.success = false;

    this.transactionService
      .deposit({
        accountNumber: this.selectedAccountNumber,
        amount: this.amount,
        description: this.description
      })
      .subscribe({
        next: (response: ApiResponse<any>) => {
          if (response.success) {
            this.success = true;
            setTimeout(() => this.router.navigate(['/dashboard']), 1500);
          } else {
            this.error = response.message || 'İşlem başarısız';
          }
          this.loading = false;
        },
        error: (err) => {
          this.error = err?.error?.message || 'İşlem sırasında hata oluştu';
          this.loading = false;
        }
      });
  }
}