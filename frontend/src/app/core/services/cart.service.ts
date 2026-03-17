import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CartItem } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CartService {
  private apiUrl = `${environment.apiUrl}/cart`;
  private cartItemsSubject = new BehaviorSubject<CartItem[]>([]);
  cartItems$ = this.cartItemsSubject.asObservable();

  constructor(private http: HttpClient) {}

  get cartCount(): number {
    return this.cartItemsSubject.value.reduce((sum, item) => sum + item.quantity, 0);
  }

  get cartTotal(): number {
    return this.cartItemsSubject.value.reduce((sum, item) => sum + item.subtotal, 0);
  }

  loadCart() {
    return this.http.get<CartItem[]>(this.apiUrl).pipe(
      tap(items => this.cartItemsSubject.next(items))
    );
  }

  addToCart(productId: number, quantity: number = 1) {
    return this.http.post(this.apiUrl, { productId, quantity }).pipe(
      tap(() => this.loadCart().subscribe())
    );
  }

  updateQuantity(id: number, quantity: number) {
    return this.http.put(`${this.apiUrl}/${id}`, quantity).pipe(
      tap(() => this.loadCart().subscribe())
    );
  }

  removeItem(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.loadCart().subscribe())
    );
  }

  clearCart() {
    return this.http.delete(this.apiUrl).pipe(
      tap(() => this.cartItemsSubject.next([]))
    );
  }

  clearLocalCart() {
    this.cartItemsSubject.next([]);
  }
}
