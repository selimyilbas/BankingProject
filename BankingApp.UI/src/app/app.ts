import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/layout/header/header';
import { SidebarComponent } from './components/layout/sidebar/sidebar';
import { AuthService } from './services/auth';

/**
 * Kök Uygulama Bileşeni: header, sidebar ve yönlendirme çıkışını barındırır.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, HeaderComponent, SidebarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  title = 'BankingApp.UI';
  
  constructor(public authService: AuthService) {}
}