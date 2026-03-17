import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { Product } from '../../../core/models/models';

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-management.component.html',
  styleUrls: ['./product-management.component.scss']
})
export class ProductManagementComponent implements OnInit {
  products: Product[] = [];
  loading = true;
  showForm = false;
  editingProduct: Product | null = null;
  saving = false;
  error = '';
  successMsg = '';

  form: FormGroup;

  constructor(private productService: ProductService, private fb: FormBuilder) {
    this.form = this.fb.group({
      name:        ['', Validators.required],
      description: ['', Validators.required],
      price:       [0, [Validators.required, Validators.min(0.01)]],
      stock:       [0, [Validators.required, Validators.min(0)]],
      category:    ['', Validators.required],
      imageUrl:    ['', Validators.required],
      isActive:    [true]
    });
  }

  ngOnInit() { this.loadProducts(); }

  loadProducts() {
    this.loading = true;
    this.productService.getAllAdmin().subscribe({
      next: p => { this.products = p; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  openAdd() {
    this.editingProduct = null;
    this.form.reset({ isActive: true, price: 0, stock: 0 });
    this.showForm = true;
    this.error = '';
  }

  openEdit(product: Product) {
    this.editingProduct = product;
    this.form.patchValue(product);
    this.showForm = true;
    this.error = '';
  }

  closeForm() { this.showForm = false; this.editingProduct = null; }

  save() {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';
    const data = this.form.value;

    const request = this.editingProduct
      ? this.productService.update(this.editingProduct.id, data)
      : this.productService.create(data);

    request.subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.successMsg = this.editingProduct ? 'Product updated!' : 'Product added!';
        setTimeout(() => this.successMsg = '', 3000);
        this.loadProducts();
      },
      error: () => { this.saving = false; this.error = 'Failed to save product.'; }
    });
  }

  delete(product: Product) {
    if (!confirm(`Delete "${product.name}"?`)) return;
    this.productService.delete(product.id).subscribe({
      next: () => {
        this.successMsg = 'Product deleted.';
        setTimeout(() => this.successMsg = '', 3000);
        this.loadProducts();
      }
    });
  }
}
