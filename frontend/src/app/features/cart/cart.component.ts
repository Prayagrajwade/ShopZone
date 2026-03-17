import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CartItem } from '../../core/models/models';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss']
})
export class CartComponent implements OnInit {
  cartItems: CartItem[] = [];
  loading = true;

  constructor(public cartService: CartService) {}

  ngOnInit() {
    this.cartService.loadCart().subscribe({
      next: (items) => { this.cartItems = items; this.loading = false; },
      error: () => { this.loading = false; }
    });

    this.cartService.cartItems$.subscribe(items => this.cartItems = items);
  }

  updateQty(item: CartItem, qty: number) {
    this.cartService.updateQuantity(item.id, qty).subscribe();
  }

  remove(item: CartItem) {
    this.cartService.removeItem(item.id).subscribe();
  }

  get total(): number {
    return this.cartItems.reduce((sum, i) => sum + i.subtotal, 0);
  }
}
