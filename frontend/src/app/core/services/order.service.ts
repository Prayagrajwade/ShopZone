import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Order, PaymentIntent } from '../models/models';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private apiUrl = `${environment.apiUrl}/orders`;

  constructor(private http: HttpClient) {}

  createPaymentIntent() {
    return this.http.post<PaymentIntent>(`${this.apiUrl}/create-payment-intent`, {});
  }

  confirmOrder(paymentIntentId: string) {
    return this.http.post<{ orderId: number; message: string }>(`${this.apiUrl}/confirm`, JSON.stringify(paymentIntentId), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  getOrders() {
    return this.http.get<Order[]>(this.apiUrl);
  }

  getOrder(id: number) {
    return this.http.get<Order>(`${this.apiUrl}/${id}`);
  }

  getAllOrdersAdmin() {
    return this.http.get<any[]>(`${this.apiUrl}/admin/all`);
  }

  createBuyNowPaymentIntent(data: { productId: number, quantity: number }) {
    return this.http.post<PaymentIntent>(
      `${environment.apiUrl}/orders/buy-now`,
      data
    );
  }
}
