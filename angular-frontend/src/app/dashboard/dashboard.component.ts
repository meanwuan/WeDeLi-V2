import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { StatCardComponent } from '../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../shared/components/status-badge/status-badge.component';
import { DashboardService } from './services/dashboard.service';
import { DashboardStats, DailyRevenue, OrderStatusSummary } from '../core/models/dashboard.model';
import { OrderListItem, ORDER_STATUS_CONFIG } from '../core/models/order.model';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, RouterModule, StatCardComponent, StatusBadgeComponent],
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
    private dashboardService = inject(DashboardService);

    // Data signals
    stats = signal<DashboardStats | null>(null);
    revenueData = signal<DailyRevenue[]>([]);
    orderStatusData = signal<OrderStatusSummary[]>([]);
    recentOrders = signal<OrderListItem[]>([]);

    // Loading states
    loading = signal(true);
    error = signal<string | null>(null);

    // Chart dimensions
    chartWidth = 560;
    chartHeight = 200;

    ngOnInit(): void {
        this.loadDashboardData();
    }

    loadDashboardData(): void {
        this.loading.set(true);
        this.error.set(null);

        // Load dashboard stats
        this.dashboardService.getDashboardSummary().subscribe({
            next: (data) => this.stats.set(data),
            error: (err) => this.handleError(err)
        });

        // Load weekly revenue
        this.dashboardService.getWeeklyRevenue().subscribe({
            next: (data) => this.revenueData.set(data),
            error: (err) => console.error('Failed to load revenue data:', err)
        });

        // Load order status summary
        this.dashboardService.getOrderStatusSummary().subscribe({
            next: (data) => this.orderStatusData.set(data)
        });

        // Load recent orders
        this.dashboardService.getRecentOrders(5).subscribe({
            next: (data) => {
                this.recentOrders.set(data);
                this.loading.set(false);
            },
            error: (err) => {
                this.handleError(err);
                this.loading.set(false);
            }
        });
    }

    private handleError(err: Error): void {
        this.error.set(err.message || 'Đã xảy ra lỗi');
        console.error('Dashboard error:', err);
    }

    // Format currency
    formatCurrency(amount: number): string {
        return new Intl.NumberFormat('vi-VN', {
            minimumFractionDigits: 0,
            maximumFractionDigits: 0
        }).format(amount);
    }

    // Format short currency for Y-axis (e.g., 1.5M, 500K)
    formatShortCurrency(amount: number): string {
        if (amount >= 1000000) {
            return (amount / 1000000).toFixed(1).replace('.0', '') + 'M';
        }
        if (amount >= 1000) {
            return (amount / 1000).toFixed(0) + 'K';
        }
        return amount.toString();
    }

    // Get max revenue for Y-axis scale
    getMaxRevenue(): number {
        const data = this.revenueData();
        if (data.length === 0) return 100000;
        return Math.max(...data.map(d => d.revenue), 1);
    }

    // Get status badge variant
    getStatusVariant(status: string): 'success' | 'warning' | 'danger' | 'info' | 'default' {
        const config = ORDER_STATUS_CONFIG[status as keyof typeof ORDER_STATUS_CONFIG];
        if (!config) return 'default';

        if (config.color.includes('22c55e')) return 'success';
        if (config.color.includes('f59e0b')) return 'warning';
        if (config.color.includes('ef4444')) return 'danger';
        if (config.color.includes('3b82f6')) return 'info';
        return 'default';
    }

    getStatusLabel(status: string): string {
        return ORDER_STATUS_CONFIG[status as keyof typeof ORDER_STATUS_CONFIG]?.label || status;
    }

    // Calculate chart path for revenue line
    getRevenuePath(): string {
        const data = this.revenueData();
        if (data.length === 0) return '';

        const maxRevenue = Math.max(...data.map(d => d.revenue));
        const padding = 20;
        const chartW = this.chartWidth - padding * 2;
        const chartH = this.chartHeight - padding * 2;

        const points = data.map((d, i) => {
            const x = padding + (i / (data.length - 1)) * chartW;
            const y = padding + chartH - (d.revenue / maxRevenue) * chartH;
            return `${x},${y}`;
        });

        return `M ${points.join(' L ')}`;
    }

    // Get max bar height for status chart
    getMaxStatusCount(): number {
        return Math.max(...this.orderStatusData().map(d => d.count), 1);
    }

    getBarHeight(count: number): number {
        return (count / this.getMaxStatusCount()) * 150;
    }

    // Get customer initials for avatar
    getCustomerInitials(name: string | null): string {
        if (!name) return 'NA';
        const parts = name.trim().split(' ');
        if (parts.length >= 2) {
            return parts[0].charAt(0) + parts[parts.length - 1].charAt(0);
        }
        return name.charAt(0) + (name.charAt(1) || '');
    }

    // New percentage-based chart methods (viewBox 0 0 100 40)
    getRevenuePathNew(): string {
        const data = this.revenueData();
        if (data.length === 0) return '';

        const maxRevenue = Math.max(...data.map(d => d.revenue), 1);

        const points = data.map((d, i) => {
            const x = this.getPointXNew(i);
            const y = this.getPointYNew(d.revenue);
            return `${x},${y}`;
        });

        return `M ${points.join(' L ')}`;
    }

    getPointXNew(index: number): number {
        const data = this.revenueData();
        if (data.length <= 1) return 50;
        return 2 + (index / (data.length - 1)) * 96; // 2% padding on each side
    }

    getPointYNew(revenue: number): number {
        const data = this.revenueData();
        const maxRevenue = Math.max(...data.map(d => d.revenue), 1);
        // Y ranges from 5 (top) to 35 (bottom)
        return 35 - (revenue / maxRevenue) * 30;
    }

    // Order count line path
    getOrdersPathNew(): string {
        const data = this.revenueData();
        if (data.length === 0) return '';

        const points = data.map((d, i) => {
            const x = this.getPointXNew(i);
            const y = this.getOrderPointY(d.orderCount || 0);
            return `${x},${y}`;
        });

        return `M ${points.join(' L ')}`;
    }

    getOrderPointY(orderCount: number): number {
        const data = this.revenueData();
        const maxOrders = Math.max(...data.map(d => d.orderCount || 0), 1);
        // Y ranges from 5 (top) to 35 (bottom)
        return 35 - (orderCount / maxOrders) * 30;
    }
}
