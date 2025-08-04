import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CustomerService } from '../../../services/customer';

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
    dateOfBirth: ''
  };
  
  loading = false;
  success = false;
  error = '';
  customerNumber = '';

  constructor(
    private customerService: CustomerService,
    private router: Router
  ) {}

  onSubmit() {
    this.loading = true;
    this.error = '';
    this.success = false;

    // Convert date string to Date object
    const customerData = {
      ...this.customer,
      dateOfBirth: new Date(this.customer.dateOfBirth)
    };

    this.customerService.createCustomer(customerData).subscribe({
      next: (response) => {
        if (response.success) {
          this.success = true;
          this.customerNumber = response.data?.customerNumber || '';
          
          // Reset form
          this.customer = {
            firstName: '',
            lastName: '',
            tckn: '',
            dateOfBirth: ''
          };
          
          // Redirect after 3 seconds
          setTimeout(() => {
            this.router.navigate(['/customers']);
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