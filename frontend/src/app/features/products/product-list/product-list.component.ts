import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, switchMap, startWith, combineLatest } from 'rxjs';
import { ProductService } from '../../../core/services/product.service';
import { CartService } from '../../../core/services/cart.service';
import { AuthService } from '../../../core/services/auth.service';
import { Product } from '../../../core/models/models';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss']
})
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  categories: string[] = [];
  selectedCategory = '';
  loading = false;
  addingToCart: { [key: number]: boolean } = {};
  successMsg: { [key: number]: boolean } = {};

  searchControl = new FormControl('');

  constructor(
    public productService: ProductService,
    public cartService: CartService,
    public auth: AuthService
  ) {}

  ngOnInit() {
    this.loadCategories();

    this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(() => this.loadProducts());
  }

  loadCategories() {
    this.productService.getCategories().subscribe(cats => this.categories = cats);
  }

  loadProducts() {
    this.loading = true;
    const search = this.searchControl.value || '';
    this.productService.getAll(this.selectedCategory || undefined, search || undefined).subscribe({
      next: (products) => { this.products = products; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  selectCategory(cat: string) {
    this.selectedCategory = cat;
    this.loadProducts();
  }

  addToCart(product: Product) {
    if (!this.auth.isLoggedIn) return;
    this.addingToCart[product.id] = true;
    this.cartService.addToCart(product.id, 1).subscribe({
      next: () => {
        this.addingToCart[product.id] = false;
        this.successMsg[product.id] = true;
        setTimeout(() => delete this.successMsg[product.id], 2000);
      },
      error: () => { this.addingToCart[product.id] = false; }
    });
  }
}
