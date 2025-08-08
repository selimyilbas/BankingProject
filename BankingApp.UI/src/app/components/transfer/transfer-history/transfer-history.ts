import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransferService, TransferDto } from '../../../services/transfer.service';
import { AccountService } from '../../../services/account';
import { Account } from '../../../models/account.model';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-transfer-history',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transfer-history.html',
  styleUrls: ['./transfer-history.css']
})
export class TransferHistoryComponent implements OnInit {
  accounts: Account[] = [];
  selectedAccountId: number | null = null;
  transfers: TransferDto[] = [];
  loading = false;
  error = '';

  constructor(
    private transferService: TransferService,
    private accountService: AccountService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (!user) return;
    this.accountService.getAccountsByCustomerId(user.customerId).subscribe(res => {
      if (res?.success) {
        this.accounts = res.data || [];
        this.selectedAccountId = this.accounts.length ? this.accounts[0].accountId : null;
        if (this.selectedAccountId) this.loadTransfers();
      }
    });
  }

  loadTransfers(): void {
    if (!this.selectedAccountId) return;
    this.loading = true;
    this.error = '';
    this.transferService.getTransfersByAccount(this.selectedAccountId).subscribe({
      next: (res) => {
        if (res?.success) {
          this.transfers = res.data || [];
        } else {
          this.error = res?.message || 'Transfer geçmişi alınamadı';
        }
      },
      error: () => {
        this.error = 'Transfer geçmişi alınamadı';
      },
      complete: () => (this.loading = false)
    });
  }
}