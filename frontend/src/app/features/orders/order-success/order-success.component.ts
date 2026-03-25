import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models/models';

@Component({
  selector: 'app-order-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-success.component.html',
  styleUrls: ['./order-success.component.scss']
})
export class OrderSuccessComponent implements OnInit {
  order: Order | null = null;
  loading = true;

  constructor(private route: ActivatedRoute, private orderService: OrderService) {}

  ngOnInit() {
    const id = +this.route.snapshot.params['id'];
    this.orderService.getOrder(id).subscribe({
      next: (order) => { this.order = order; this.loading = false; },
      error: () => { this.loading = false; }
    });
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
