import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { loadStripe, Stripe, StripeElements, StripePaymentElement } from '@stripe/stripe-js';
import { CartService } from '../../core/services/cart.service';
import { OrderService } from '../../core/services/order.service';
import { CartItem } from '../../core/models/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss']
})
export class CheckoutComponent implements OnInit {
  cartItems: CartItem[] = [];
  loading = true;
  processing = false;
  error = '';
  paymentIntentId = '';

  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;

  constructor(
    private cartService: CartService,
    private orderService: OrderService,
    private router: Router
  ) {}

  ngOnInit() {
  const state = history.state;
  if (state?.buyNow) {
    this.paymentIntentId = state.paymentIntent.paymentIntentId;

    this.initStripeWithClientSecret(state.paymentIntent.clientSecret);

    this.cartItems = [
    {
      id: 0, 
      productId: state.product.id,
      productName: state.product.name,
      price: state.product.price,
      quantity: state.quantity,
      imageUrl: state.product.imageUrl,
      subtotal: state.product.price * state.quantity,
      stock: state.product.stock
    }
  ];

    this.loading = false;
    return;
  }

  this.cartService.cartItems$.subscribe(items => this.cartItems = items);

  this.cartService.loadCart().subscribe({
    next: () => {
      this.loading = false;
      this.initStripe();
    },
    error: () => {
      this.loading = false;
      this.error = 'Failed to load cart.';
    }
  });
}

  async initStripeWithClientSecret(clientSecret: string) {
    this.stripe = await loadStripe(environment.stripePublishableKey);

    if (!this.stripe) {
      this.error = 'Failed to load Stripe.';
      return;
    }

    this.elements = this.stripe.elements({
      clientSecret: clientSecret,
      appearance: { theme: 'stripe' }
    });

    const paymentElement = this.elements.create('payment');
    paymentElement.mount('#payment-element');
  }
  
  async initStripe() {
    this.stripe = await loadStripe(environment.stripePublishableKey);
    if (!this.stripe) { this.error = 'Failed to load Stripe.'; this.loading = false; return; }

    this.orderService.createPaymentIntent().subscribe({
      next: async (pi) => {
        this.paymentIntentId = pi.paymentIntentId;

        this.elements = this.stripe!.elements({
          clientSecret: pi.clientSecret,
          appearance: { theme: 'stripe' }
        });

        const paymentElement = this.elements.create('payment');
        paymentElement.mount('#payment-element');
        this.loading = false;
      },
      error: () => { this.loading = false; this.error = 'Failed to initialize payment.'; }
    });
  }

  async placeOrder() {
    if (!this.stripe || !this.elements) return;
    this.processing = true;
    this.error = '';

    const { error: submitError } = await this.elements.submit();
    if (submitError) {
      this.error = submitError.message ?? 'Validation failed.';
      this.processing = false;
      return;
    }

    const { error, paymentIntent } = await this.stripe.confirmPayment({
      elements: this.elements,
      redirect: 'if_required' 
    });

    if (error) {
      this.error = error.message ?? 'Payment failed.';
      this.processing = false;
      return;
    }

    this.orderService.confirmOrder(paymentIntent!.id).subscribe({
      next: (res) => {
        this.cartService.clearLocalCart();
        this.router.navigate(['/order-success', res.orderId]);
      },
      error: (err) => {
        this.error = err.error?.message || 'Order confirmation failed.';
        this.processing = false;
      }
    });
  }

  get total(): number {
    return this.cartItems.reduce((sum, i) => sum + i.subtotal, 0);
  }
}