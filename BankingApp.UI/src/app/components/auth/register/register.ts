import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

        const request = {
      firstName: this.customer.firstName,
      lastName: this.customer.lastName,
      tckn: this.customer.tckn,
      password: this.customer.password,
      dateOfBirth: this.customer.dateOfBirth
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