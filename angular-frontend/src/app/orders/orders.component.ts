import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StatusBadgeComponent } from '../shared/components/status-badge/status-badge.component';
import { OrderService, OrderFilters, OrderStatusCount } from './services/order.service';
import { OrderListItem, OrderStatus, ORDER_STATUS_CONFIG } from '../core/models/order.model';

type OrderTab = 'all' | 'pending' | 'in_transit' | 'delivered' | 'cancelled';

interface Tab {
    id: OrderTab;
    label: string;
    status?: OrderStatus;
}

@Component({
    selector: 'app-orders',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, StatusBadgeComponent],
    templateUrl: './orders.component.html',
    styleUrl: './orders.component.scss'
})
export class OrdersComponent implements OnInit {
    private orderService = inject(OrderService);

    // State
    orders = signal<OrderListItem[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);

    // Pagination
    currentPage = signal(1);
    pageSize = signal(10);
    totalItems = signal(0);
    totalPages = computed(() => Math.ceil(this.totalItems() / this.pageSize()));

    // Filters
    activeTab = signal<OrderTab>('all');
    searchTerm = signal('');
    dateFrom = signal('');
    dateTo = signal('');

    // Status counts
    statusCounts = signal<OrderStatusCount>({
        all: 0,
        pending: 0,
        in_transit: 0,
        delivered: 0,
        cancelled: 0
    });

    // Selected orders for bulk actions
    selectedOrders = signal<number[]>([]);
    selectAll = signal(false);

    tabs: Tab[] = [
        { id: 'all', label: 'Tất cả' },
        { id: 'pending', label: 'Chờ lấy hàng', status: 'pending_pickup' },
        { id: 'in_transit', label: 'Đang giao', status: 'in_transit' },
        { id: 'delivered', label: 'Hoàn thành', status: 'delivered' },
        { id: 'cancelled', label: 'Đã hủy', status: 'cancelled' }
    ];

    ngOnInit(): void {
        this.loadStatusCounts();
        this.loadOrders();
    }

    loadStatusCounts(): void {
        this.orderService.getOrderStatusCounts().subscribe({
            next: (counts) => this.statusCounts.set(counts),
            error: (err) => console.error('Failed to load status counts:', err)
        });
    }

    loadOrders(): void {
        this.loading.set(true);
        this.error.set(null);

        const filters: OrderFilters = {
            pageNumber: this.currentPage(),
            pageSize: this.pageSize(),
            searchTerm: this.searchTerm() || undefined,
            fromDate: this.dateFrom() || undefined,
            toDate: this.dateTo() || undefined
        };

        // Add status filter if not "all"
        const tab = this.tabs.find(t => t.id === this.activeTab());
        if (tab?.status) {
            filters.status = tab.status;
        }

        this.orderService.getOrders(filters).subscribe({
            next: (response) => {
                this.orders.set(response.items || []);
                this.totalItems.set(response.totalCount || 0);
                this.loading.set(false);
            },
            error: (err) => {
                this.error.set(err.message || 'Không thể tải danh sách đơn hàng');
                this.loading.set(false);
                // Set empty orders on error
                this.orders.set([]);
            }
        });
    }

    onTabChange(tabId: OrderTab): void {
        this.activeTab.set(tabId);
        this.currentPage.set(1);
        this.selectedOrders.set([]);
        this.selectAll.set(false);
        this.loadOrders();
    }

    onSearch(): void {
        this.currentPage.set(1);
        this.loadOrders();
    }

    onDateFilterChange(): void {
        this.currentPage.set(1);
        this.loadOrders();
    }

    onPageChange(page: number): void {
        if (page >= 1 && page <= this.totalPages()) {
            this.currentPage.set(page);
            this.loadOrders();
        }
    }

    onPageSizeChange(size: number): void {
        this.pageSize.set(size);
        this.currentPage.set(1);
        this.loadOrders();
    }

    toggleSelectAll(): void {
        if (this.selectAll()) {
            this.selectedOrders.set([]);
            this.selectAll.set(false);
        } else {
            this.selectedOrders.set(this.orders().map(o => o.orderId));
            this.selectAll.set(true);
        }
    }

    toggleOrderSelection(orderId: number): void {
        const current = this.selectedOrders();
        if (current.includes(orderId)) {
            this.selectedOrders.set(current.filter(id => id !== orderId));
        } else {
            this.selectedOrders.set([...current, orderId]);
        }
        // Update selectAll state
        this.selectAll.set(this.selectedOrders().length === this.orders().length);
    }

    isSelected(orderId: number): boolean {
        return this.selectedOrders().includes(orderId);
    }

    // Get status badge variant
    getStatusVariant(status: string): 'success' | 'warning' | 'danger' | 'info' | 'default' {
        switch (status) {
            case 'delivered': return 'success';
            case 'pending':
            case 'pending_pickup': return 'warning';
            case 'cancelled':
            case 'returned': return 'danger';
            case 'in_transit':
            case 'out_for_delivery': return 'info';
            default: return 'default';
        }
    }

    getStatusLabel(status: string): string {
        return ORDER_STATUS_CONFIG[status as keyof typeof ORDER_STATUS_CONFIG]?.label || status;
    }

    getTabCount(tabId: OrderTab): number {
        return this.statusCounts()[tabId] || 0;
    }

    // Format currency
    formatCurrency(amount: number): string {
        if (!amount && amount !== 0) return '0đ';
        return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
    }

    // Get customer initials
    getInitials(name: string | null): string {
        if (!name) return 'NA';
        const parts = name.trim().split(' ');
        if (parts.length >= 2) {
            return parts[0].charAt(0).toUpperCase() + parts[parts.length - 1].charAt(0).toUpperCase();
        }
        return name.substring(0, 2).toUpperCase();
    }

    // Actions
    assignDriver(): void {
        if (this.selectedOrders().length === 0) {
            alert('Vui lòng chọn ít nhất một đơn hàng');
            return;
        }
        // TODO: Open assign driver modal
        console.log('Assign driver to orders:', this.selectedOrders());
    }

    exportExcel(): void {
        // TODO: Implement Excel export
        console.log('Export to Excel');
    }

    printOrders(): void {
        if (this.selectedOrders().length === 0) {
            alert('Vui lòng chọn ít nhất một đơn hàng');
            return;
        }
        // TODO: Implement print functionality
        console.log('Print orders:', this.selectedOrders());
    }

    // Pagination helpers
    getVisiblePages(): number[] {
        const total = this.totalPages();
        const current = this.currentPage();
        const pages: number[] = [];

        if (total <= 5) {
            for (let i = 1; i <= total; i++) pages.push(i);
        } else {
            if (current <= 3) {
                pages.push(1, 2, 3, -1, total);
            } else if (current >= total - 2) {
                pages.push(1, -1, total - 2, total - 1, total);
            } else {
                pages.push(1, -1, current, -1, total);
            }
        }
        return pages;
    }
}
