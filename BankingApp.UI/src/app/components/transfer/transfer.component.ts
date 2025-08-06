import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransferService } from '../../services/transfer.service';
import { AccountService } from '../../services/account.service';
import { AuthService } from '../../services/auth.service';

interface Account {
  accountId: number;
  accountNumber: string;
  currency: string;
  balance: number;
}

interface TransferRequest {
  fromAccountId: number;
  toAccountId: number;
  amount: number;
  description: string;
}

@Component({
  selector: 'app-transfer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transfer.component.html',
  styleUrls: ['./transfer.component.css']
})
export class TransferComponent implements OnInit {
  accounts: Account[] = [];
  transferRequest: TransferRequest = {
    fromAccountId: 0,
    toAccountId: 0,
    amount: 0,
    description: ''
  };
  
  loading = false;
  successMessage = '';
  errorMessage = '';
  currentUser: any;

  constructor(
    private transferService: TransferService,
    private accountService: AccountService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    this.loadAccounts();
  }

  async loadAccounts(): Promise<void> {
    try {
      this.loading = true;
      const response = await this.accountService.getAccountsByCustomer(this.currentUser.customerId).toPromise();
      
      if (response && response.success) {
        this.accounts = response.data;
        if (this.accounts.length > 0) {
          this.transferRequest.fromAccountId = this.accounts[0].accountId;
        }
      }
    } catch (error) {
      console.error('Error loading accounts:', error);
      this.errorMessage = 'Hesaplar yüklenirken hata oluştu';
    } finally {
      this.loading = false;
    }
  }

  async createTransfer(): Promise<void> {
    if (!this.validateTransfer()) {
      return;
    }

    try {
      this.loading = true;
      this.clearMessages();

      const response = await this.transferService.createTransfer(this.transferRequest).toPromise();
      
      if (response && response.success) {
        this.successMessage = 'Transfer başarıyla tamamlandı!';
        this.resetForm();
        this.loadAccounts(); // Refresh account balances
      } else {
        this.errorMessage = response?.message || 'Transfer işlemi başarısız';
      }
    } catch (error) {
      console.error('Error creating transfer:', error);
      this.errorMessage = 'Transfer işlemi sırasında hata oluştu';
    } finally {
      this.loading = false;
    }
  }

  validateTransfer(): boolean {
    if (!this.transferRequest.fromAccountId || !this.transferRequest.toAccountId) {
      this.errorMessage = 'Lütfen kaynak ve hedef hesabı seçin';
      return false;
    }

    if (this.transferRequest.fromAccountId === this.transferRequest.toAccountId) {
      this.errorMessage = 'Aynı hesaba transfer yapılamaz';
      return false;
    }

    if (!this.transferRequest.amount || this.transferRequest.amount <= 0) {
      this.errorMessage = 'Lütfen geçerli bir tutar girin';
      return false;
    }

    const fromAccount = this.accounts.find(a => a.accountId === this.transferRequest.fromAccountId);
    if (fromAccount && fromAccount.balance < this.transferRequest.amount) {
      this.errorMessage = 'Yetersiz bakiye';
      return false;
    }

    return true;
  }

  resetForm(): void {
    this.transferRequest = {
      fromAccountId: this.accounts.length > 0 ? this.accounts[0].accountId : 0,
      toAccountId: 0,
      amount: 0,
      description: ''
    };
  }

  clearMessages(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }

  getAccountDisplayName(account: Account): string {
    return `${account.accountNumber} (${account.currency}) - ${account.balance.toFixed(2)} ${account.currency}`;
  }

  getFromAccount(): Account | undefined {
    return this.accounts.find(a => a.accountId === this.transferRequest.fromAccountId);
  }

  getToAccount(): Account | undefined {
    return this.accounts.find(a => a.accountId === this.transferRequest.toAccountId);
  }
} 