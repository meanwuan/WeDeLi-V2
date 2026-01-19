import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';
import { ApiResponse } from '../../core/models/common.model';

// Staff DTO - matching backend WarehouseStaffDto
export interface Staff {
    staffId: number;
    userId: number;
    companyId: number;
    fullName: string;
    phone: string;
    warehouseLocation: string;
    isActive: boolean;
    createdAt: string;
}

export interface StaffFilters {
    location?: string;
    searchTerm?: string;
    companyId?: number;
}

export interface StaffStatusCount {
    total: number;
    active: number;
    inactive: number;
}

// Create Staff DTO - matching backend CreateWarehouseStaffDto
export interface CreateStaffDto {
    userId: number;
    companyId: number;
    warehouseLocation: string;
}

// Update Staff DTO - matching backend UpdateWarehouseStaffDto
export interface UpdateStaffDto {
    warehouseLocation?: string;
    companyId?: number;
    isActive?: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class StaffService {
    private api = inject(ApiService);
    private readonly endpoint = '/staff';

    // Get all staff by company (or all staff if companyId = 0)
    getStaffByCompany(companyId: number): Observable<Staff[]> {
        // If companyId is 0, call /staff (all staff for admin)
        const endpoint = companyId > 0
            ? `${this.endpoint}/company/${companyId}`
            : this.endpoint;

        return this.api.get<ApiResponse<Staff[]>>(endpoint).pipe(
            map(response => response.data || [])
        );
    }

    // Get staff with filters (client-side filtering)
    getStaff(companyId: number, filters?: StaffFilters): Observable<Staff[]> {
        return this.api.get<ApiResponse<Staff[]>>(`${this.endpoint}/company/${companyId}`).pipe(
            map(response => {
                let staffList = response.data || [];

                if (filters?.searchTerm) {
                    const term = filters.searchTerm.toLowerCase();
                    staffList = staffList.filter(s =>
                        s.fullName.toLowerCase().includes(term) ||
                        s.phone.includes(term) ||
                        s.warehouseLocation.toLowerCase().includes(term)
                    );
                }

                if (filters?.location) {
                    staffList = staffList.filter(s =>
                        s.warehouseLocation.toLowerCase().includes(filters.location!.toLowerCase())
                    );
                }

                return staffList;
            })
        );
    }

    // Get staff by ID
    getStaffById(id: number): Observable<Staff> {
        return this.api.get<ApiResponse<Staff>>(`${this.endpoint}/${id}`).pipe(
            map(response => response.data!)
        );
    }

    // Toggle staff status
    toggleStaffStatus(staffId: number, isActive: boolean): Observable<boolean> {
        return this.api.patch<ApiResponse<boolean>>(
            `${this.endpoint}/${staffId}/status`,
            { isActive }
        ).pipe(
            map(response => response.data!)
        );
    }

    // Get status counts
    getStatusCounts(companyId: number): Observable<StaffStatusCount> {
        return this.getStaffByCompany(companyId).pipe(
            map(staffList => ({
                total: staffList.length,
                active: staffList.filter(s => s.isActive).length,
                inactive: staffList.filter(s => !s.isActive).length
            }))
        );
    }

    // Get unique locations for dropdown filter
    getLocations(companyId: number): Observable<string[]> {
        return this.getStaffByCompany(companyId).pipe(
            map(staffList => {
                const locations = new Set(staffList.map(s => s.warehouseLocation).filter(Boolean));
                return Array.from(locations);
            })
        );
    }

    // Create new staff
    createStaff(dto: CreateStaffDto): Observable<Staff> {
        return this.api.post<ApiResponse<Staff>>(this.endpoint, dto).pipe(
            map(response => response.data!)
        );
    }

    // Update staff
    updateStaff(staffId: number, dto: UpdateStaffDto): Observable<Staff> {
        return this.api.put<ApiResponse<Staff>>(`${this.endpoint}/${staffId}`, dto).pipe(
            map(response => response.data!)
        );
    }

    // Delete staff
    deleteStaff(staffId: number): Observable<boolean> {
        return this.api.delete<ApiResponse<boolean>>(`${this.endpoint}/${staffId}`).pipe(
            map(response => response.data!)
        );
    }

    // Helper methods
    getStatusLabel(isActive: boolean): string {
        return isActive ? 'Active' : 'Inactive';
    }
}
