import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ProductService } from '../../../core/services/product.service';
import { OrderService } from '../../../core/services/order.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  stats = { totalProducts: 0, totalOrders: 0, totalRevenue: 0, pendingOrders: 0 };
  recentOrders: any[] = [];
  loading = true;

  constructor(private productService: ProductService, private orderService: OrderService) {}

  ngOnInit() {
    forkJoin({
      products: this.productService.getAllAdmin(),
      orders: this.orderService.getAllOrdersAdmin()
    }).subscribe({
      next: ({ products, orders }) => {
        this.stats.totalProducts = products.length;
        this.stats.totalOrders = orders.length;
        this.stats.totalRevenue = orders.reduce((s: number, o: any) => s + o.totalAmount, 0);
        this.stats.pendingOrders = orders.filter((o: any) => o.status === 'pending').length;
        //this.recentOrders = orders.slice(0, 5);
        this.recentOrders = orders;
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = { pending: 'pending', paid: 'paid', on_the_way: 'on-way', delivered: 'delivered' };
    return map[status] ?? '';
  }
}
