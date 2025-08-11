import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService, ApiResponse, RegisterRequest } from '../../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  customer = {
    firstName: '',
    lastName: '',
    tckn: '',
    dateOfBirth: '',
    password: ''
  };
  
  loading = false;
  success = false;
  error = '';
  customerNumber = '';

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit() {
    this.loading = true;
    this.error = '';
    this.success = false;

    // Ensure ISO date format (yyyy-MM-dd) for API
    const isoDate = this.customer.dateOfBirth
      ? new Date(this.customer.dateOfBirth).toISOString().slice(0, 10)
      : '';

    const request: RegisterRequest = {
      firstName: this.customer.firstName,
      lastName: this.customer.lastName,
      tckn: this.customer.tckn,
      password: this.customer.password,
      dateOfBirth: isoDate
    };

    this.authService.register(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.success = true;
          this.customerNumber = response.data?.customerNumber || '';
          
          // Reset form
          this.customer = {
            firstName: '',
            lastName: '',
            tckn: '',
            dateOfBirth: '',
            password: ''
          };
          
          // Redirect after 3 seconds
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 3000);
        } else {
          this.error = response.message;
        }
        this.loading = false;
      },
      error: (error) => {
        this.error = 'Bir hata oluştu. Lütfen tekrar deneyin.';
        this.loading = false;
      }
    });
  }
}