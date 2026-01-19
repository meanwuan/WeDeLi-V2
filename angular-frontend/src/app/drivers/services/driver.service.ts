import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';
import { ApiResponse, PaginatedResponse } from '../../core/models/common.model';

// Driver Response DTO - matching backend DriverResponseDto
export interface Driver {
    driverId: number;
    userId: number;
    companyId: number;
    companyName: string;
    fullName: string;
    phone: string;
    email: string | null;
    driverLicense: string;
    licenseExpiry: string | null;
    totalTrips: number;
    successRate: number;
    rating: number;
    isActive: boolean;
    createdAt: string;
}

export interface DriverFilters {
    status?: 'active' | 'inactive';
    searchTerm?: string;
    companyId?: number;
    minRating?: number;
    pageNumber: number;
    pageSize: number;
}

export interface DriverStatusCount {
    all: number;
    active: number;
    inactive: number;
}

// Create Driver DTO - matching backend CreateDriverDto
export interface CreateDriverDto {
    userId: number;
    companyId: number;
    driverLicense: string;
    licenseExpiry: string; // ISO date string
}

// Update Driver DTO - matching backend UpdateDriverDto
export interface UpdateDriverDto {
    driverLicense?: string;
    licenseExpiry?: string;
    isActive?: boolean;
}

// Status configuration for UI display
export const DRIVER_STATUS_CONFIG = {
    'active': { label: 'Đang hoạt động', color: '#22c55e', bgColor: 'rgba(34, 197, 94, 0.15)' },
    'inactive': { label: 'Đang nghỉ', color: '#6b7280', bgColor: 'rgba(107, 114, 128, 0.15)' }
};

@Injectable({
    providedIn: 'root'
})
export class DriverService {
    private api = inject(ApiService);
    private readonly endpoint = '/drivers';

    // Get drivers by company
    getDriversByCompany(companyId: number): Observable<Driver[]> {
        return this.api.get<ApiResponse<Driver[]>>(`${this.endpoint}/company/${companyId}`).pipe(
            map(response => response.data || [])
        );
    }

    // Get drivers with filters
    getDrivers(filters: DriverFilters): Observable<PaginatedResponse<Driver>> {
        const params: Record<string, any> = {
            pageNumber: filters.pageNumber,
            pageSize: filters.pageSize
        };

        if (filters.searchTerm) params['searchTerm'] = filters.searchTerm;

        // If companyId is 0 or undefined, call /drivers (all drivers for admin)
        // Otherwise call /drivers/company/{companyId}
        const endpoint = filters.companyId && filters.companyId > 0
            ? `${this.endpoint}/company/${filters.companyId}`
            : this.endpoint;

        return this.api.get<ApiResponse<Driver[]>>(
            endpoint,
            params
        ).pipe(
            map(response => {
                let drivers = response.data || [];

                // Client-side filtering
                if (filters.status) {
                    const isActive = filters.status === 'active';
                    drivers = drivers.filter(d => d.isActive === isActive);
                }
                if (filters.minRating) {
                    drivers = drivers.filter(d => d.rating >= filters.minRating!);
                }
                if (filters.searchTerm) {
                    const term = filters.searchTerm.toLowerCase();
                    drivers = drivers.filter(d =>
                        d.fullName.toLowerCase().includes(term) ||
                        d.phone.includes(term) ||
                        d.driverLicense.toLowerCase().includes(term)
                    );
                }

                return {
                    items: drivers,
                    totalCount: drivers.length,
                    pageNumber: filters.pageNumber,
                    pageSize: filters.pageSize,
                    totalPages: Math.ceil(drivers.length / filters.pageSize),
                    hasPreviousPage: filters.pageNumber > 1,
                    hasNextPage: filters.pageNumber < Math.ceil(drivers.length / filters.pageSize)
                };
            })
        );
    }

    // Get driver by ID
    getDriverById(id: number): Observable<Driver> {
        return this.api.get<ApiResponse<Driver>>(`${this.endpoint}/${id}`).pipe(
            map(response => response.data!)
        );
    }

    // Get top performing drivers
    getTopPerformers(companyId: number, topN: number = 3): Observable<Driver[]> {
        return this.api.get<ApiResponse<Driver[]>>(
            `${this.endpoint}/company/${companyId}/top-performers`,
            { topN }
        ).pipe(
            map(response => response.data || [])
        );
    }

    // Get active drivers
    getActiveDrivers(companyId: number): Observable<Driver[]> {
        return this.api.get<ApiResponse<Driver[]>>(`${this.endpoint}/company/${companyId}/active`).pipe(
            map(response => response.data || [])
        );
    }

    // Toggle driver status
    toggleDriverStatus(driverId: number, isActive: boolean): Observable<boolean> {
        return this.api.patch<ApiResponse<boolean>>(
            `${this.endpoint}/${driverId}/status`,
            { isActive }
        ).pipe(
            map(response => response.data!)
        );
    }

    // Get driver status counts
    getDriverStatusCounts(companyId?: number): Observable<DriverStatusCount> {
        const endpoint = companyId
            ? `${this.endpoint}/company/${companyId}`
            : this.endpoint;

        return this.api.get<ApiResponse<Driver[]>>(endpoint).pipe(
            map(response => {
                const drivers = response.data || [];
                return {
                    all: drivers.length,
                    active: drivers.filter(d => d.isActive).length,
                    inactive: drivers.filter(d => !d.isActive).length
                };
            })
        );
    }

    // Create new driver
    createDriver(dto: CreateDriverDto): Observable<Driver> {
        return this.api.post<ApiResponse<Driver>>(this.endpoint, dto).pipe(
            map(response => response.data!)
        );
    }

    // Update driver
    updateDriver(driverId: number, dto: UpdateDriverDto): Observable<Driver> {
        return this.api.put<ApiResponse<Driver>>(`${this.endpoint}/${driverId}`, dto).pipe(
            map(response => response.data!)
        );
    }

    // Delete driver
    deleteDriver(driverId: number): Observable<boolean> {
        return this.api.delete<ApiResponse<boolean>>(`${this.endpoint}/${driverId}`).pipe(
            map(response => response.data!)
        );
    }

    // Helper methods
    getStatusLabel(isActive: boolean): string {
        return isActive ? DRIVER_STATUS_CONFIG.active.label : DRIVER_STATUS_CONFIG.inactive.label;
    }

    getStatusVariant(isActive: boolean): 'success' | 'default' {
        return isActive ? 'success' : 'default';
    }

    // Calculate days until license expires
    getDaysUntilExpiry(licenseExpiry: string | null): number | null {
        if (!licenseExpiry) return null;
        const expiry = new Date(licenseExpiry);
        const today = new Date();
        const diffTime = expiry.getTime() - today.getTime();
        return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    }

    // Check if license is expiring soon (within 30 days)
    isLicenseExpiringSoon(licenseExpiry: string | null): boolean {
        const days = this.getDaysUntilExpiry(licenseExpiry);
        return days !== null && days > 0 && days <= 30;
    }

    // Check if license is expired
    isLicenseExpired(licenseExpiry: string | null): boolean {
        const days = this.getDaysUntilExpiry(licenseExpiry);
        return days !== null && days <= 0;
    }
}
