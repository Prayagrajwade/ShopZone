import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import { ProductService } from '../../../core/services/product.service';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { Product } from '../../../core/models/models';
import { OrderService } from 'src/app/core/services/order.service';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.scss']
})
export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  loading = true;
  quantity = 1;
  adding = false;
  added = false;
  errorMsg: string = '';

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    public cartService: CartService,
    public auth: AuthService,
    public orderService: OrderService,
    public router: Router
  ) { }

  ngOnInit() {
    this.route.params.pipe(
      switchMap(params => this.productService.getById(+params['id']))
    ).subscribe({
      next: (p) => { this.product = p; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  increment() { if (this.product && this.quantity < this.product.stock) this.quantity++; }
  decrement() { if (this.quantity > 1) this.quantity--; }

  addToCart() {
    if (!this.product) return;

    if (!this.auth.isLoggedIn) {
      this.cartService.addToLocalCart(this.product.id, this.quantity);
      this.added = true;
      setTimeout(() => this.added = false, 2500);
      return;
    }

    this.adding = true;
    this.cartService.addToCart(this.product.id, this.quantity).subscribe({
      next: () => {
        this.adding = false;
        this.added = true;
        setTimeout(() => this.added = false, 2500);
      },
      error: (err) => {
        this.errorMsg = err?.error?.message || 'Something went wrong';
        this.adding = false;
      }
    });
  }

  addToLocalCart(productId: number, quantity: number) {
    let cart = JSON.parse(localStorage.getItem('cart') || '[]');

    const existing = cart.find((x: any) => x.productId === productId);

    if (existing) {
      existing.quantity += quantity;
    } else {
      cart.push({ productId, quantity });
    }

    localStorage.setItem('cart', JSON.stringify(cart));
  }

  buyNow() {
    if (!this.product) return;

    if(!this.auth.isLoggedIn){
      this.router.navigate(['/login']);
    }
    this.adding = true;
    this.errorMsg = '';

    this.orderService.createBuyNowPaymentIntent({
      productId: this.product.id,
      quantity: this.quantity
    }).subscribe({
      next: (res) => {
        this.router.navigate(['/checkout'], {
          state: {
            buyNow: true,
            paymentIntent: res,
            product: this.product,
            quantity: this.quantity
          }
        });
      },
      error: (err) => {
        this.errorMsg = err?.error?.message || 'Something went wrong';
        this.adding = false;
      }
    });
  }
}
