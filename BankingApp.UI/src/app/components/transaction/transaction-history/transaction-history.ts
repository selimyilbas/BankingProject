import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransactionService } from '../../../services/transaction';
import { AccountService } from '../../../services/account';
import { AuthService } from '../../../services/auth';
import { Transaction } from '../../../models/transaction.model';
import { Account } from '../../../models/account.model';

@Component({
  selector: 'app-transaction-history',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transaction-history.html',
  styleUrl: './transaction-history.css'
})
export class TransactionHistoryComponent implements OnInit {
  transactions: Transaction[] = [];
  accounts: Account[] = [];
  selectedAccountId: number | null = null;
  loading = false;
  error = '';
  
  // Pagination
  currentPage = 1;
  pageSize = 10;
  totalTransactions = 0;
  
  // Date filtering
  startDate = '';
  endDate = '';

  constructor(
    private transactionService: TransactionService,
    private accountService: AccountService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadUserAccounts();
  }

  loadUserAccounts() {
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser?.customerId) {
      this.error = 'Kullanıcı bilgisi bulunamadı';
      return;
    }

    this.loading = true;
    this.accountService.getAccountsByCustomerId(currentUser.customerId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.accounts = response.data;
          if (this.accounts.length > 0) {
            this.selectedAccountId = this.accounts[0].accountId;
            this.loadTransactions();
          }
        } else {
          this.error = 'Hesap bilgileri yüklenirken hata oluştu';
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading accounts:', error);
        this.error = 'Hesap bilgileri yüklenirken hata oluştu';
        this.loading = false;
      }
    });
  }

  loadTransactions() {
    if (!this.selectedAccountId) return;

    this.loading = true;
    this.error = '';

    // Use date range filter if both dates are provided
    if (this.startDate && this.endDate) {
      this.transactionService.getTransactionsByDateRange(
        this.selectedAccountId, 
        this.startDate, 
        this.endDate
      ).subscribe({
        next: (response) => this.handleTransactionResponse(response),
        error: (error) => this.handleTransactionError(error)
      });
    } else {
      // Use paged transactions
      this.transactionService.getTransactionsPaged(
        this.selectedAccountId, 
        this.currentPage, 
        this.pageSize
      ).subscribe({
        next: (response) => this.handleTransactionResponse(response),
        error: (error) => this.handleTransactionError(error)
      });
    }
  }

  private handleTransactionResponse(response: any) {
    if (response.success) {
      if (Array.isArray(response.data)) {
        this.transactions = response.data;
      } else if (response.data && response.data.items) {
        // Paged response
        this.transactions = response.data.items;
        this.totalTransactions = response.data.totalCount;
      } else {
        this.transactions = [];
      }
    } else {
      this.error = response.message || 'İşlem geçmişi yüklenirken hata oluştu';
      this.transactions = [];
    }
    this.loading = false;
  }

  private handleTransactionError(error: any) {
    console.error('Error loading transactions:', error);
    this.error = 'İşlem geçmişi yüklenirken hata oluştu';
    this.transactions = [];
    this.loading = false;
  }

  onAccountChange() {
    this.currentPage = 1;
    this.loadTransactions();
  }

  onDateFilter() {
    if (this.startDate && this.endDate) {
      this.currentPage = 1;
      this.loadTransactions();
    }
  }

  clearDateFilter() {
    this.startDate = '';
    this.endDate = '';
    this.currentPage = 1;
    this.loadTransactions();
  }

  onPageChange(page: number) {
    this.currentPage = page;
    this.loadTransactions();
  }

  getAccountDisplayName(account: Account): string {
    return `${account.accountNumber} (${account.currency})`;
  }

  formatCurrency(amount: number, currency: string): string {
    const currencySymbols: { [key: string]: string } = {
      'TL': '₺',
      'USD': '$',
      'EUR': '€'
    };
    
    const symbol = currencySymbols[currency] || currency;
    return `${amount.toLocaleString('tr-TR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${symbol}`;
  }

  formatDate(date: Date | string): string {
    const d = new Date(date);
    return d.toLocaleString('tr-TR');
  }

  getTransactionTypeClass(type: string): string {
    switch (type?.toLowerCase()) {
      case 'deposit':
      case 'transfer_in':
        return 'transaction-positive';
      case 'transfer_out':
      case 'withdrawal':
        return 'transaction-negative';
      default:
        return 'transaction-neutral';
    }
  }

  getTransactionTypeText(type: string): string {
    switch (type?.toLowerCase()) {
      case 'deposit':
        return 'Para Yatırma';
      case 'transfer_in':
        return 'Gelen Transfer';
      case 'transfer_out':
        return 'Giden Transfer';
      case 'withdrawal':
        return 'Para Çekme';
      default:
        return type;
    }
  }

  get totalPages(): number {
    return Math.ceil(this.totalTransactions / this.pageSize);
  }

  get startIndex(): number {
    return (this.currentPage - 1) * this.pageSize + 1;
  }

  get endIndex(): number {
    return Math.min(this.currentPage * this.pageSize, this.totalTransactions);
  }
}