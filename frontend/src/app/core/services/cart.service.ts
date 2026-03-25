import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CartItem } from '../models/models';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class CartService {
  private apiUrl = `${environment.apiUrl}/cart`;
  private cartItemsSubject = new BehaviorSubject<CartItem[]>([]);
  cartItems$ = this.cartItemsSubject.asObservable();
  private _cartItems: CartItem[] = [];

  constructor(private http: HttpClient, public auth: AuthService) {}

  get cartCount(): number {
    if (this.auth.isLoggedIn) {
      return this._cartItems.reduce((sum, x) => sum + x.quantity, 0);
    } else {
      return this.getLocalCartCount();
    }
  }

  get cartTotal(): number {
    return this.cartItemsSubject.value.reduce((sum, item) => sum + item.subtotal, 0);
  }

  loadCart() {
    return this.http.get<CartItem[]>(this.apiUrl).pipe(
      tap(items =>{this._cartItems = items;  this.cartItemsSubject.next(items)})
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
      tap(() => {this._cartItems = [];  this.cartItemsSubject.next([])})
    );
  }

  clearLocalCart() {
    this.clearCart();
    localStorage.removeItem('cart');
  }

  mergeCart(items: any[]) {
    return this.http.post(`${this.apiUrl}/merge`, items);
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
    this.cartItemsSubject.next([]);
  }

  getLocalCartCount(): number {
    const cart = JSON.parse(localStorage.getItem('cart') || '[]');
    return cart.reduce((sum: number, x: any) => sum + x.quantity, 0);
  }
}
