import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StatusBadgeComponent } from '../shared/components/status-badge/status-badge.component';
import { DriverService, Driver, DriverFilters, DriverStatusCount } from './services/driver.service';
import { AuthService } from '../auth/services/auth.service';

type StatusFilter = 'all' | 'active' | 'inactive';
type RatingFilter = 'all' | '4plus';

@Component({
    selector: 'app-drivers',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, StatusBadgeComponent],
    templateUrl: './drivers.component.html',
    styleUrl: './drivers.component.scss'
})
export class DriversComponent implements OnInit {
    private driverService = inject(DriverService);
    private authService = inject(AuthService);

    private get companyId(): number {
        const user = this.authService.getCurrentUser();
        console.log('DriversComponent - User:', user);
        return user?.companyId ?? 0;
    }

    // State
    drivers = signal<Driver[]>([]);
    topPerformers = signal<Driver[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);

    // Selected driver for detail
    selectedDriver = signal<Driver | null>(null);
    showDetailPanel = signal(false);

    // Pagination
    currentPage = signal(1);
    pageSize = signal(20);
    totalItems = signal(0);
    totalPages = computed(() => Math.ceil(this.totalItems() / this.pageSize()));

    // Filters
    statusFilter = signal<StatusFilter>('all');
    ratingFilter = signal<RatingFilter>('all');
    searchTerm = signal('');

    // Status counts
    statusCounts = signal<DriverStatusCount>({
        all: 0,
        active: 0,
        inactive: 0
    });

    ngOnInit(): void {
        this.loadStatusCounts();
        this.loadTopPerformers();
        this.loadDrivers();
    }

    loadStatusCounts(): void {
        this.driverService.getDriverStatusCounts(this.companyId).subscribe({
            next: (counts) => this.statusCounts.set(counts),
            error: (err) => console.error('Failed to load status counts:', err)
        });
    }

    loadTopPerformers(): void {
        this.driverService.getTopPerformers(this.companyId, 3).subscribe({
            next: (performers) => this.topPerformers.set(performers),
            error: (err) => console.error('Failed to load top performers:', err)
        });
    }

    loadDrivers(): void {
        this.loading.set(true);
        this.error.set(null);

        const filters: DriverFilters = {
            pageNumber: this.currentPage(),
            pageSize: this.pageSize(),
            searchTerm: this.searchTerm() || undefined,
            companyId: this.companyId
        };

        // Apply status filter
        if (this.statusFilter() !== 'all') {
            filters.status = this.statusFilter() as 'active' | 'inactive';
        }

        // Apply rating filter
        if (this.ratingFilter() === '4plus') {
            filters.minRating = 4.0;
        }

        this.driverService.getDrivers(filters).subscribe({
            next: (response) => {
                console.log('DriversComponent - Response:', response);
                this.drivers.set(response.items || []);
                this.totalItems.set(response.totalCount || 0);
                this.loading.set(false);
            },
            error: (err) => {
                console.error('DriversComponent - Error:', err);
                this.error.set(err.message || 'Không thể tải danh sách tài xế');
                this.loading.set(false);
                this.drivers.set([]);
            }
        });
    }

    onStatusFilterChange(status: StatusFilter): void {
        this.statusFilter.set(status);
        this.currentPage.set(1);
        this.loadDrivers();
    }

    onRatingFilterChange(rating: RatingFilter): void {
        this.ratingFilter.set(rating);
        this.currentPage.set(1);
        this.loadDrivers();
    }

    onSearch(): void {
        this.currentPage.set(1);
        this.loadDrivers();
    }

    onPageChange(page: number): void {
        if (page >= 1 && page <= this.totalPages()) {
            this.currentPage.set(page);
            this.loadDrivers();
        }
    }

    selectDriver(driver: Driver): void {
        this.selectedDriver.set(driver);
        this.showDetailPanel.set(true);
    }

    closeDetailPanel(): void {
        this.showDetailPanel.set(false);
        this.selectedDriver.set(null);
    }

    toggleDriverStatus(driver: Driver, event: Event): void {
        event.stopPropagation();
        const newStatus = !driver.isActive;

        this.driverService.toggleDriverStatus(driver.driverId, newStatus).subscribe({
            next: () => {
                // Update local state
                const updatedDrivers = this.drivers().map(d =>
                    d.driverId === driver.driverId ? { ...d, isActive: newStatus } : d
                );
                this.drivers.set(updatedDrivers);
                this.loadStatusCounts();
            },
            error: (err) => console.error('Failed to toggle status:', err)
        });
    }

    // Status helpers
    getStatusVariant(isActive: boolean): 'success' | 'warning' | 'danger' | 'info' | 'default' {
        return isActive ? 'success' : 'default';
    }

    getStatusLabel(isActive: boolean): string {
        return this.driverService.getStatusLabel(isActive);
    }

    // License expiry helpers
    getDaysUntilExpiry(licenseExpiry: string | null): number | null {
        return this.driverService.getDaysUntilExpiry(licenseExpiry);
    }

    isLicenseExpiringSoon(licenseExpiry: string | null): boolean {
        return this.driverService.isLicenseExpiringSoon(licenseExpiry);
    }

    isLicenseExpired(licenseExpiry: string | null): boolean {
        return this.driverService.isLicenseExpired(licenseExpiry);
    }

    getExpiryBadgeText(licenseExpiry: string | null): string {
        const days = this.getDaysUntilExpiry(licenseExpiry);
        if (days === null) return '';
        if (days <= 0) return 'Đã hết hạn';
        if (days <= 7) return `Hết hạn trong ${days} ngày`;
        if (days <= 30) return `Hết hạn trong ${days} ngày`;
        return '';
    }

    // UI helpers
    getInitials(name: string): string {
        return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    }

    formatRating(rating: number): string {
        return rating?.toFixed(1) || '0.0';
    }

    formatPhone(phone: string): string {
        if (!phone) return '';
        // Format as 0xxx.xxx.xxx
        return phone.replace(/(\d{4})(\d{3})(\d{3})/, '$1.$2.$3');
    }

    formatSuccessRate(rate: number): string {
        return `${Math.round(rate || 0)}%`;
    }
}
