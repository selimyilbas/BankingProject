import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TransferService } from '../../services/transfer.service';
import { AccountService } from '../../services/account';
import { AuthService } from '../../services/auth';


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

interface TransferByNumberRequest {
  fromAccountNumber: string;
  toAccountNumber: string;
  amount: number;
  description?: string;
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
  // transfer mode: 'self' = between own accounts, 'external' = to another customer's account
  transferMode: 'self' | 'external' = 'self';
  transferRequest: TransferRequest = {
    fromAccountId: 0,
    toAccountId: 0,
    amount: 0,
    description: ''
  };
  byNumberRequest: TransferByNumberRequest = {
    fromAccountNumber: '',
    toAccountNumber: '',
    amount: 0,
    description: ''
  };
  
  loading = false;
  successMessage = '';
  errorMessage = '';
  currentUser: any;
  
  // New properties for the template
  conversionLoading = false;
  conversionError = '';
  convertedAmount = 0;

  constructor(
    private transferService: TransferService,
    private accountService: AccountService,
    private authService: AuthService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getCurrentUser();
    if (this.currentUser) {
      this.loadAccounts();
    } else {
      // Kullanıcı henüz yüklenmediyse, yüklendiğinde hesapları getir
      const sub = this.authService.currentUser$.subscribe(user => {
        if (user) {
          this.currentUser = user;
          this.loadAccounts();
          sub.unsubscribe();
        }
      });
    }
  }

  async loadAccounts(): Promise<void> {
    try {
      this.loading = true;
      
      // Get fromAccountId from query parameters
      const fromAccountId = this.route.snapshot.queryParams['fromAccountId'];
      
      this.accountService.getAccountsByCustomerId(this.currentUser.customerId).subscribe({
        next: (response) => {
          if (response && response.success) {
            this.accounts = response.data || [];
            if (this.accounts.length > 0) {
              // Pre-select account if fromAccountId was provided
              if (fromAccountId) {
                const selectedAccount = this.accounts.find(account => account.accountId.toString() === fromAccountId);
                if (selectedAccount) {
                  this.transferRequest.fromAccountId = selectedAccount.accountId;
                } else {
                  this.transferRequest.fromAccountId = this.accounts[0].accountId;
                }
              } else {
                this.transferRequest.fromAccountId = this.accounts[0].accountId;
              }
            }
          } else {
            this.errorMessage = response?.message || 'Hesaplar yüklenemedi';
          }
        },
        error: (err) => {
          console.error('Error loading accounts:', err);
          this.errorMessage = 'Hesaplar yüklenirken hata oluştu';
          this.loading = false;
        },
        complete: () => {
          this.loading = false;
        }
      });
    } catch (error) {
      console.error('Error loading accounts:', error);
      this.errorMessage = 'Hesaplar yüklenirken hata oluştu';
    } finally {
      this.loading = false;
    }
  }

  createTransfer(): void {
    // Branch by selected mode
    if (this.transferMode === 'external') {
      this.createTransferByAccountNumber();
      return;
    }

    if (!this.validateTransfer()) {
      return;
    }

    this.loading = true;
    this.clearMessages();

    const payload = {
      fromAccountId: this.transferRequest.fromAccountId,
      toAccountId: this.transferRequest.toAccountId,
      amount: this.transferRequest.amount,
      description: this.transferRequest.description
    };

    this.transferService.createTransfer(payload as any).subscribe({
      next: (response) => {
        if (response && response.success) {
          this.successMessage = 'Transfer başarıyla tamamlandı!';
          this.resetForm();
          this.loadAccounts();
        } else {
          this.errorMessage = response?.message || 'Transfer işlemi başarısız';
        }
      },
      error: (error) => {
        console.error('Error creating transfer:', error);
        this.errorMessage = 'Transfer işlemi sırasında hata oluştu';
      },
      complete: () => {
        this.loading = false;
      }
    });
  }

  private createTransferByAccountNumber(): void {
    // Validate external transfer fields
    const from = this.getFromAccount();
    if (!from) {
      this.errorMessage = 'Kaynak hesap seçin';
      return;
    }
    if (!this.byNumberRequest.toAccountNumber) {
      this.errorMessage = 'Hedef hesap numarası girin';
      return;
    }
    if (!this.transferRequest.amount || this.transferRequest.amount <= 0) {
      this.errorMessage = 'Lütfen geçerli bir tutar girin';
      return;
    }

    // If source account number not manually set yet, infer from selected source
    this.byNumberRequest.fromAccountNumber = from.accountNumber;

    this.loading = true;
    this.clearMessages();

    const payload = {
      fromAccountNumber: this.byNumberRequest.fromAccountNumber,
      toAccountNumber: this.byNumberRequest.toAccountNumber,
      amount: this.transferRequest.amount,
      description: this.byNumberRequest.description || this.transferRequest.description
    };

    this.transferService.createTransferByAccountNumber(payload as any).subscribe({
      next: (response) => {
        if (response && response.success) {
          this.successMessage = 'Transfer başarıyla tamamlandı!';
          this.resetForm();
          this.loadAccounts();
        } else {
          this.errorMessage = response?.message || 'Transfer işlemi başarısız';
        }
      },
      error: (error) => {
        console.error('Error creating transfer by account number:', error);
        this.errorMessage = 'Transfer işlemi sırasında hata oluştu';
      },
      complete: () => {
        this.loading = false;
      }
    });
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

  // New methods for the template
  updatePreview(): void {
    this.clearMessages();
    
    // If currencies are different, calculate conversion (only when target is selected from list)
    const fromAccount = this.getFromAccount();
    const toAccount = this.getToAccount();
    
    if (fromAccount && toAccount && fromAccount.currency !== toAccount.currency && this.transferRequest.amount > 0) {
      this.calculateConversion();
    } else {
      this.convertedAmount = this.transferRequest.amount;
      this.conversionError = '';
    }
  }

  async calculateConversion(): Promise<void> {
    const fromAccount = this.getFromAccount();
    const toAccount = this.getToAccount();
    
    if (!fromAccount || !toAccount) return;
    
    try {
      this.conversionLoading = true;
      this.conversionError = '';
      
      // For now, use hardcoded rates (matching the backend)
      const rates: { [key: string]: { [key: string]: number } } = {
        'TL': { 'EUR': 0.029, 'USD': 0.031 },
        'EUR': { 'TL': 34.50, 'USD': 1.08 },
        'USD': { 'TL': 32.50, 'EUR': 0.93 }
      };
      
      const rate = rates[fromAccount.currency]?.[toAccount.currency] || 1;
      this.convertedAmount = this.transferRequest.amount * rate;
    } catch (error) {
      console.error('Error calculating conversion:', error);
      this.conversionError = 'Döviz çevirisi hesaplanamadı';
    } finally {
      this.conversionLoading = false;
    }
  }

  requestTransfer(): void {
    if (!this.validateTransfer()) {
      return;
    }
    // Do transfer directly without extra confirmation step
    this.confirmTransfer();
  }

  async confirmTransfer(): Promise<void> {
    
    await this.createTransfer();
  }

  cancelConfirm(): void {
    
  }
} 