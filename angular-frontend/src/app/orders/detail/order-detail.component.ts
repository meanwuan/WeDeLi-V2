import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { OrderService } from '../services/order.service';
import { Order, OrderStatus, ORDER_STATUS_CONFIG } from '../../core/models/order.model';

@Component({
    selector: 'app-order-detail',
    standalone: true,
    imports: [CommonModule, RouterModule, StatusBadgeComponent],
    templateUrl: './order-detail.component.html',
    styleUrl: './order-detail.component.scss'
})
export class OrderDetailComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private orderService = inject(OrderService);

    order = signal<Order | null>(null);
    isLoading = signal(true);
    error = signal<string | null>(null);
    showActionMenu = signal(false);
    showStatusModal = signal(false);

    ngOnInit(): void {
        const orderId = Number(this.route.snapshot.params['orderId']);
        if (orderId) {
            this.loadOrder(orderId);
        }
    }

    loadOrder(orderId: number): void {
        this.isLoading.set(true);
        this.error.set(null);

        this.orderService.getOrderById(orderId).subscribe({
            next: (response: any) => {
                const data = response.data || response;
                this.order.set(data);
                this.isLoading.set(false);
            },
            error: (err) => {
                this.error.set(err.message || 'Không thể tải thông tin đơn hàng');
                this.isLoading.set(false);
            }
        });
    }

    // Navigation
    goBack(): void {
        this.router.navigate(['/orders']);
    }

    editOrder(): void {
        if (this.order()) {
            this.router.navigate(['/orders', this.order()!.orderId, 'edit']);
        }
    }

    // Status helpers
    getStatusVariant(status: string): 'success' | 'warning' | 'danger' | 'info' | 'default' {
        switch (status) {
            case 'delivered': return 'success';
            case 'pending':
            case 'pending_pickup': return 'warning';
            case 'cancelled':
            case 'returned': return 'danger';
            case 'in_transit':
            case 'out_for_delivery':
            case 'picked_up': return 'info';
            default: return 'default';
        }
    }

    getStatusLabel(status: string): string {
        return ORDER_STATUS_CONFIG[status as OrderStatus]?.label || status;
    }

    getParcelTypeLabel(type: string): string {
        const labels: Record<string, string> = {
            'document': 'Tài liệu',
            'fragile': 'Dễ vỡ',
            'electronics': 'Điện tử',
            'food': 'Thực phẩm',
            'cold': 'Hàng lạnh',
            'other': 'Khác'
        };
        return labels[type?.toLowerCase()] || type || 'Khác';
    }

    getPaymentMethodLabel(method: string): string {
        const labels: Record<string, string> = {
            'cash': 'Tiền mặt',
            'bank_transfer': 'Chuyển khoản',
            'e_wallet': 'Ví điện tử',
            'periodic': 'Công nợ định kỳ'
        };
        return labels[method?.toLowerCase()] || method || 'Tiền mặt';
    }

    getPaymentStatusLabel(status: string): string {
        const labels: Record<string, string> = {
            'unpaid': 'Chưa thanh toán',
            'paid': 'Đã thanh toán',
            'pending': 'Đang xử lý',
            'refunded': 'Đã hoàn tiền'
        };
        return labels[status?.toLowerCase()] || status || 'Chưa thanh toán';
    }

    // Format helpers
    formatCurrency(amount: number | null): string {
        if (!amount && amount !== 0) return '0đ';
        return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
    }

    formatDateTime(dateStr: string | null): string {
        if (!dateStr) return '--';
        const date = new Date(dateStr);
        return date.toLocaleString('vi-VN', {
            day: '2-digit', month: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    }

    formatDate(dateStr: string | null): string {
        if (!dateStr) return '--';
        const date = new Date(dateStr);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit', month: '2-digit', year: 'numeric'
        });
    }

    // Actions
    toggleActionMenu(): void {
        this.showActionMenu.update(v => !v);
    }

    copyTrackingCode(): void {
        if (this.order()) {
            navigator.clipboard.writeText(this.order()!.trackingCode);
            alert('Đã sao chép mã vận đơn!');
        }
    }

    printOrder(): void {
        window.print();
    }

    updateStatus(newStatus: OrderStatus): void {
        if (!this.order()) return;

        this.orderService.updateOrderStatus(this.order()!.orderId, {
            newStatus,
            notes: `Cập nhật trạng thái thành ${this.getStatusLabel(newStatus)}`
        }).subscribe({
            next: () => {
                this.loadOrder(this.order()!.orderId);
                this.showStatusModal.set(false);
            },
            error: (err) => {
                alert('Không thể cập nhật trạng thái: ' + err.message);
            }
        });
    }

    // Get available next statuses
    getAvailableStatuses(): OrderStatus[] {
        const current = this.order()?.orderStatus;
        const statusFlow: Record<string, OrderStatus[]> = {
            'pending': ['pending_pickup', 'cancelled'],
            'pending_pickup': ['picked_up', 'cancelled'],
            'picked_up': ['in_transit'],
            'in_transit': ['out_for_delivery'],
            'out_for_delivery': ['delivered', 'returned'],
            'delivered': [],
            'returned': [],
            'cancelled': []
        };
        return statusFlow[current || ''] || [];
    }
}
