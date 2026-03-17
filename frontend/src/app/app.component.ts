import { Component, OnInit } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';
import { CartService } from './core/services/cart.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  constructor(public auth: AuthService, public cartService: CartService) {}

  ngOnInit() {
    if (this.auth.isLoggedIn) {
      this.cartService.loadCart().subscribe();
    }
  }

  logout() {
    this.cartService.clearLocalCart();
    this.auth.logout();
  }
}
