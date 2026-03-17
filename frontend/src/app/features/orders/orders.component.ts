import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { OrderService } from '../../core/services/order.service';
import { Order } from '../../core/models/models';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.scss']
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  loading = true;
  expandedOrderId: number | null = null;

  constructor(private orderService: OrderService) {}

  ngOnInit() {
    this.orderService.getOrders().subscribe({
      next: (orders) => { this.orders = orders; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  toggleOrder(id: number) {
    this.expandedOrderId = this.expandedOrderId === id ? null : id;
  }

  getStatusIcon(status: string): string {
    const map: Record<string, string> = {
      pending: '⏳', paid: '✅', on_the_way: '🚚', delivered: '📦'
    };
    return map[status] ?? '📋';
  }

  getStatusLabel(status: string): string {
    const map: Record<string, string> = {
      pending: 'Pending', paid: 'Paid', on_the_way: 'On the Way', delivered: 'Delivered'
    };
    return map[status] ?? status;
  }
}
