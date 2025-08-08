import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth';
import { CustomerService } from '../../services/customer';
import { ApiResponse } from '../../models/api-response.model';
import { Customer } from '../../models/customer.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent {
  user: any;
  profileForm = {
    firstName: '',
    lastName: '',
    email: '',
    phoneNumber: '',
    dateOfBirth: ''
  };
  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
  };
  loading = true;
  message = '';
  error = '';

  constructor(
    private auth: AuthService,
    private customerService: CustomerService,
    private router: Router
  ) {
    this.user = this.auth.getCurrentUser();
    if (!this.user) {
      this.router.navigate(['/login']);
      return;
    }
    this.loadProfile();
  }

  loadProfile() {
    this.loading = true;
    this.customerService.getCustomerById(this.user.customerId).subscribe({
      next: (res: ApiResponse<Customer>) => {
        if (res.success && res.data) {
          const c = res.data;
          this.profileForm.firstName = c.firstName;
          this.profileForm.lastName = c.lastName;
          this.profileForm.email = (c as any).email || '';
          this.profileForm.phoneNumber = (c as any).phoneNumber || '';
          this.profileForm.dateOfBirth = new Date(c.dateOfBirth).toISOString().substring(0,10);
        }
        this.loading = false;
      },
      error: () => { this.loading = false; this.error = 'Profil yüklenemedi'; }
    });
  }

  saveProfile() {
    this.message = '';
    this.error = '';
    this.customerService.updateCustomer(this.user.customerId, {
      ...this.profileForm,
      dateOfBirth: this.profileForm.dateOfBirth
    }).subscribe({
      next: (res: ApiResponse<Customer>) => {
        if (res.success && res.data) {
          this.message = 'Profil güncellendi';
          // local storage güncelle
          const updated = {
            ...this.user,
            firstName: res.data.firstName,
            lastName: res.data.lastName,
            name: `${res.data.firstName} ${res.data.lastName}`
          };
          localStorage.setItem('currentUser', JSON.stringify(updated));
        } else {
          this.error = res.message || 'Güncelleme başarısız';
        }
      },
      error: () => this.error = 'Güncelleme sırasında hata'
    });
  }

  changePassword() {
    this.message = '';
    this.error = '';
    if (this.passwordForm.newPassword !== this.passwordForm.confirmNewPassword) {
      this.error = 'Yeni şifreler eşleşmiyor';
      return;
    }
    this.customerService.changePassword(
      this.user.customerId,
      this.passwordForm.currentPassword,
      this.passwordForm.newPassword
    ).subscribe({
      next: (res: ApiResponse<boolean>) => {
        if (res.success && res.data) {
          this.message = 'Şifre değiştirildi';
          this.passwordForm = { currentPassword: '', newPassword: '', confirmNewPassword: '' };
        } else {
          this.error = res.message || 'Şifre değiştirilemedi';
        }
      },
      error: () => this.error = 'Şifre değiştirirken hata'
    });
  }
}


