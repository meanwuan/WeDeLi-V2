import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../auth/services/auth.service';

interface CustomerStats {
    totalOrders: number;
    pendingOrders: number;
    inTransitOrders: number;
    deliveredOrders: number;
    totalRevenue: number;
    // Map for UI display
    inProgress: number;
    delivered: number;
    cancelled: number;
}

interface RecentOrder {
    orderId: number;
    trackingCode: string;
    senderName: string;
    receiverName: string;
    receiverProvince: string;
    createdAt: string;
    orderStatus: string;
}

@Component({
    selector: 'app-customer-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './customer-dashboard.component.html',
    styleUrl: './customer-dashboard.component.scss'
})
export class CustomerDashboardComponent implements OnInit {
    userName = '';
    userId: number | null = null;

    stats = signal<CustomerStats>({
        totalOrders: 0,
        pendingOrders: 0,
        inTransitOrders: 0,
        deliveredOrders: 0,
        totalRevenue: 0,
        inProgress: 0,
        delivered: 0,
        cancelled: 0
    });
    recentOrders = signal<RecentOrder[]>([]);
    isLoading = signal(true);

    constructor(
        private api: ApiService,
        private authService: AuthService
    ) { }

    ngOnInit(): void {
        const user = this.authService.getCurrentUser();
        this.userName = user?.fullName || 'Khách hàng';
        this.userId = user?.userId || null;

        if (this.userId) {
            this.loadDashboardData();
        } else {
            this.isLoading.set(false);
        }
    }

    loadDashboardData(): void {
        if (!this.userId) return;

        // First, get customer profile by user ID to get customerId
        // API: GET /api/v1/customer/user/{userId}
        this.api.get<any>(`/customer/user/${this.userId}`).subscribe({
            next: (customerResponse) => {
                const customerId = customerResponse.customerId || customerResponse.data?.customerId;

                if (customerId) {
                    this.loadStatsAndOrders(customerId);
                } else {
                    console.error('Customer ID not found for user');
                    this.isLoading.set(false);
                }
            },
            error: (err) => {
                console.error('Error getting customer profile:', err);
                this.isLoading.set(false);
            }
        });
    }

    private loadStatsAndOrders(customerId: number): void {
        // Load customer dashboard stats from backend
        // API: GET /api/v1/dashboard/customer/{customerId}
        this.api.get<any>(`/dashboard/customer/${customerId}`).subscribe({
            next: (response) => {
                // Backend returns DashboardStatsDto directly or wrapped in data
                const data = response.data || response;
                this.stats.set({
                    totalOrders: data.totalOrders || 0,
                    pendingOrders: data.pendingOrders || 0,
                    inTransitOrders: data.inTransitOrders || 0,
                    deliveredOrders: data.deliveredOrders || 0,
                    totalRevenue: data.totalRevenue || 0,
                    // Map for UI cards
                    inProgress: data.inTransitOrders || 0,
                    delivered: data.deliveredOrders || 0,
                    cancelled: 0 // Backend doesn't track cancelled separately
                });
            },
            error: (err) => {
                console.error('Error loading customer stats:', err);
            }
        });

        // Load recent orders from backend
        // API: GET /api/v1/orders/customer/{customerId}?pageNumber=1&pageSize=5
        this.api.get<any>(`/orders/customer/${customerId}?pageNumber=1&pageSize=5`).subscribe({
            next: (response) => {
                this.isLoading.set(false);
                const orders = response.data || response || [];
                this.recentOrders.set(orders.map((order: any) => ({
                    orderId: order.orderId,
                    trackingCode: order.trackingCode,
                    senderName: order.senderName,
                    receiverName: order.receiverName,
                    receiverProvince: order.receiverProvince,
                    createdAt: this.formatDate(order.createdAt),
                    orderStatus: order.orderStatus
                })));
            },
            error: (err) => {
                this.isLoading.set(false);
                console.error('Error loading orders:', err);
            }
        });
    }

    formatDate(dateStr: string): string {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    }

    getStatusLabel(status: string): string {
        const labels: Record<string, string> = {
            'pending': 'Chờ xử lý',
            'pending_pickup': 'Chờ lấy hàng',
            'in_transit': 'Đang vận chuyển',
            'out_for_delivery': 'Đang giao',
            'delivering': 'Đang giao',
            'delivered': 'Đã giao',
            'cancelled': 'Đã hủy',
            'returned': 'Hoàn trả'
        };
        return labels[status] || status;
    }

    getStatusClass(status: string): string {
        const classes: Record<string, string> = {
            'pending': 'pending',
            'pending_pickup': 'pending',
            'in_transit': 'in-progress',
            'out_for_delivery': 'in-progress',
            'delivering': 'in-progress',
            'delivered': 'delivered',
            'cancelled': 'cancelled',
            'returned': 'cancelled'
        };
        return classes[status] || 'pending';
    }

    getStatusIcon(status: string): string {
        const icons: Record<string, string> = {
            'pending': '⏳',
            'pending_pickup': '📦',
            'in_transit': '🚚',
            'out_for_delivery': '🚚',
            'delivering': '🚚',
            'delivered': '✅',
            'cancelled': '❌',
            'returned': '↩️'
        };
        return icons[status] || '📦';
    }
}
