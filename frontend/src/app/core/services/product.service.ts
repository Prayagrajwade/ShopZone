import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Product, TopProduct } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  getAll(category?: string, search?: string) {
    let params = new HttpParams();
    if (category) params = params.set('category', category);
    if (search) params = params.set('search', search);
    return this.http.get<Product[]>(this.apiUrl, { params });
  }

  getById(id: number) {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  getCategories() {
    return this.http.get<string[]>(`${this.apiUrl}/categories`);
  }

  // Admin
  getAllAdmin() {
    return this.http.get<Product[]>(`${this.apiUrl}/admin/all`);
  }

  create(product: Partial<Product>) {
    return this.http.post<Product>(this.apiUrl, product);
  }

  update(id: number, product: Partial<Product>) {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, product);
  }

  delete(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getTopProducts() {
    return this.http.get<TopProduct[]>(`${this.apiUrl}/top-products`);
  }
}
