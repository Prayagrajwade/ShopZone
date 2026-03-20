import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../core/services/cart.service';
import { CartItem } from '../../core/models/models';
import { ProductService } from 'src/app/core/services/product.service';

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

  constructor(private router: Router,
              public cartService: CartService,
              public productService : ProductService) {}

  ngOnInit() {
    if (this.cartService.auth.isLoggedIn) {
      this.cartService.loadCart().subscribe({
        next: (items) => {
          this.cartItems = items;
          this.loading = false;
        },
        error: () => { this.loading = false; }
      });

      this.cartService.cartItems$.subscribe(items => this.cartItems = items);

    } else {
      if (!this.cartService.auth.isLoggedIn) {
        const cart = JSON.parse(localStorage.getItem('cart') || '[]');
        const ids = cart.map((x: any) => x.productId);
        this.productService.getByIds(ids).subscribe(products => {

          this.cartItems = products.map(p => {
            const local = cart.find((x: any) => x.productId === p.id);

            return {
              id: 0,
              productId: p.id,
              productName: p.name,
              imageUrl: p.imageUrl,
              price: p.price,
              quantity: local.quantity,
              subtotal: p.price * local.quantity,
              stock: p.stock
            };
          });

          this.loading = false;
        });
      }
      this.loading = false;
    }
  }

  getLocalCartItems(): CartItem[] {
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    return cart.map((x: any) => ({
      id: 0,
      productId: x.productId,
      productName: 'Product',
      imageUrl: '',
      price: 0,
      quantity: x.quantity,
      subtotal: 0,
      stock: 0
    }));
  }

  updateQty(item: CartItem, qty: number) {
    if (this.cartService.auth.isLoggedIn) {
      this.cartService.updateQuantity(item.id, qty).subscribe();
    } else {
      this.updateLocalQty(item.productId, qty);
    }
  }

  updateLocalQty(productId: number, qty: number) {
    let cart = JSON.parse(localStorage.getItem('cart') || '[]');

    const item = cart.find((x: any) => x.productId === productId);

    if (item) {
      item.quantity = qty;
    }

    localStorage.setItem('cart', JSON.stringify(cart));

    this.cartItems = this.cartItems.map(ci => {
      if (ci.productId === productId) {
        return {
          ...ci,
          quantity: qty,
          subtotal: ci.price * qty
        };
      }
      return ci;
    });
  }

  remove(item: CartItem) {
    if (this.cartService.auth.isLoggedIn) {
      this.cartService.removeItem(item.id).subscribe();
    } else {
      this.removeLocalItem(item.productId);
    }
  }

  removeLocalItem(productId: number) {
    let cart = JSON.parse(localStorage.getItem('cart') || '[]');
    cart = cart.filter((x: any) => x.productId !== productId);

    localStorage.setItem('cart', JSON.stringify(cart));

    this.cartItems = this.cartItems.filter(ci => ci.productId !== productId);
  }

  get total(): number {
    return this.cartItems.reduce((sum, i) => sum + i.subtotal, 0);
  }

  checkout() {
    if (!this.cartService.auth.isLoggedIn) {
      this.router.navigate(['/login']);
    } else {
      this.router.navigate(['/checkout']);
    }
  }
}
