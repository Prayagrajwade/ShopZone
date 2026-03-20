import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  form: FormGroup;
  loading = false;
  error = '';

  constructor(
    private fb: FormBuilder,
    private auth: AuthService,
    private cart: CartService,
    private router: Router
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  submit() {
    if (this.form.invalid) return;

    this.loading = true;
    this.error = '';

    const { email, password } = this.form.value;

    this.auth.login(email, password).subscribe({
      next: (res) => {

        const localCart = JSON.parse(localStorage.getItem('cart') || '[]');

        if (localCart.length > 0) {
          this.cart.mergeCart(localCart).subscribe(() => {
            localStorage.removeItem('cart');
            this.cart.loadCart().subscribe();
          });
        } else {
          this.cart.loadCart().subscribe();
        }

        this.router.navigate([res.role === 'admin' ? '/admin' : '/products']);
      },
      error: (err) => {
        this.error = err.error?.message || 'Login failed';
        this.loading = false;
      }
    });
  }

  fillAdmin() {
    this.form.setValue({ email: 'admin@shop.com', password: 'admin123' });
  }
}
