import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../auth/services/auth.service';
import { DashboardStats, DailyRevenue, OrderStatusSummary } from '../../core/models/dashboard.model';
import { OrderListItem, ORDER_STATUS_CONFIG } from '../../core/models/order.model';
import { ApiResponse } from '../../core/models/common.model';

@Injectable({
    providedIn: 'root'
})
export class DashboardService {
    private api = inject(ApiService);
    private authService = inject(AuthService);

    // Get current user's companyId
    private getCompanyId(): number | null {
        const user = this.authService.getCurrentUser();
        return user?.companyId ?? null;
    }

    // Get dashboard summary - uses company endpoint for CompanyAdmin
    getDashboardSummary(): Observable<DashboardStats> {
        const companyId = this.getCompanyId();
        const endpoint = companyId
            ? `/dashboard/company/${companyId}`
            : '/dashboard/summary';

        return this.api.get<DashboardStats>(endpoint).pipe(
            catchError(err => {
                console.error('Dashboard summary error:', err);
                return of(this.getDefaultStats());
            })
        );
    }

    // Get recent orders - filtered by company for CompanyAdmin
    getRecentOrders(limit: number = 10): Observable<OrderListItem[]> {
        const companyId = this.getCompanyId();
        const endpoint = companyId
            ? `/orders/company/${companyId}`
            : '/orders';

        return this.api.get<ApiResponse<OrderListItem[]>>(endpoint, { pageSize: limit }).pipe(
            map(response => response.data || []),
            catchError(err => {
                console.error('Recent orders error:', err);
                return of([]);
            })
        );
    }

    // Mock: Get weekly revenue data for chart
    getWeeklyRevenue(): Observable<DailyRevenue[]> {
        const mockData: DailyRevenue[] = [
            { date: 'T2', revenue: 12500000, orderCount: 45 },
            { date: 'T3', revenue: 14200000, orderCount: 52 },
            { date: 'T4', revenue: 13800000, orderCount: 48 },
            { date: 'T5', revenue: 15600000, orderCount: 58 },
            { date: 'T6', revenue: 17200000, orderCount: 65 },
            { date: 'T7', revenue: 16800000, orderCount: 62 },
            { date: 'CN', revenue: 18500000, orderCount: 70 }
        ];
        return of(mockData);
    }

    // Get order status summary from real orders data - filtered by company
    getOrderStatusSummary(): Observable<OrderStatusSummary[]> {
        const companyId = this.getCompanyId();
        const endpoint = companyId
            ? `/orders/company/${companyId}`
            : '/orders';

        return this.api.get<ApiResponse<OrderListItem[]>>(endpoint, { pageSize: 100 }).pipe(
            map(response => {
                const orders = response.data || [];
                const statusCounts = new Map<string, number>();

                // Count orders by status
                orders.forEach(order => {
                    const status = order.orderStatus || 'unknown';
                    statusCounts.set(status, (statusCounts.get(status) || 0) + 1);
                });

                // Convert to summary array with labels and colors
                const summaryData: OrderStatusSummary[] = [];
                statusCounts.forEach((count, status) => {
                    const config = ORDER_STATUS_CONFIG[status as keyof typeof ORDER_STATUS_CONFIG];
                    summaryData.push({
                        status,
                        count,
                        label: config?.label || status,
                        color: config?.color || '#6b7280'
                    });
                });

                // Sort by count descending
                return summaryData.sort((a, b) => b.count - a.count);
            }),
            catchError(err => {
                console.error('Order status summary error:', err);
                return of([]);
            })
        );
    }

    private getDefaultStats(): DashboardStats {
        return {
            totalOrders: 0,
            pendingOrders: 0,
            inTransitOrders: 0,
            deliveredOrders: 0,
            totalRevenue: 0,
            todayRevenue: 0,
            activeVehicles: 0,
            activeDrivers: 0,
            pendingComplaints: 0,
            pendingCodAmount: 0
        };
    }
}
