import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';
import {
    Vehicle,
    VehicleDetail,
    VehicleLocation,
    VehicleStatus,
    CreateVehicleRequest,
    UpdateVehicleRequest,
    UpdateVehicleStatusRequest,
    VEHICLE_STATUS_CONFIG,
    VEHICLE_TYPE_CONFIG
} from '../../core/models/vehicle.model';
import { ApiResponse, PaginatedResponse } from '../../core/models/common.model';

export interface VehicleFilters {
    status?: VehicleStatus;
    searchTerm?: string;
    vehicleType?: string;
    companyId?: number;
    pageNumber: number;
    pageSize: number;
}

export interface VehicleStatusCount {
    all: number;
    available: number;
    in_use: number;
    maintenance: number;
    inactive: number;
}

@Injectable({
    providedIn: 'root'
})
export class VehicleService {
    private api = inject(ApiService);
    private readonly endpoint = '/vehicles';

    // Get all vehicles with filters
    getVehicles(filters: VehicleFilters): Observable<PaginatedResponse<Vehicle>> {
        const params: Record<string, any> = {
            pageNumber: filters.pageNumber,
            pageSize: filters.pageSize
        };

        if (filters.searchTerm) params['searchTerm'] = filters.searchTerm;
        if (filters.vehicleType) params['vehicleType'] = filters.vehicleType;
        if (filters.companyId) params['companyId'] = filters.companyId;
        if (filters.status) params['status'] = filters.status;

        return this.api.get<ApiResponse<Vehicle[]>>(this.endpoint, params).pipe(
            map(response => ({
                items: response.data || [],
                totalCount: response.data?.length || 0,
                pageNumber: filters.pageNumber,
                pageSize: filters.pageSize,
                totalPages: 1,
                hasPreviousPage: false,
                hasNextPage: false
            }))
        );
    }

    // Get vehicle by ID
    getVehicleById(id: number): Observable<VehicleDetail> {
        return this.api.get<ApiResponse<VehicleDetail>>(`${this.endpoint}/${id}`).pipe(
            map(response => response.data!)
        );
    }

    // Get vehicle locations for map
    getVehicleLocations(companyId?: number): Observable<VehicleLocation[]> {
        const params = companyId ? { companyId } : {};
        return this.api.get<ApiResponse<VehicleLocation[]>>(`${this.endpoint}/locations`, params).pipe(
            map(response => response.data || [])
        );
    }

    // Get active vehicles (for tracking)
    getActiveVehicles(companyId?: number): Observable<Vehicle[]> {
        const params = companyId ? { companyId } : {};
        return this.api.get<ApiResponse<Vehicle[]>>(`${this.endpoint}/active`, params).pipe(
            map(response => response.data || [])
        );
    }

    // Create new vehicle
    createVehicle(vehicle: CreateVehicleRequest): Observable<ApiResponse<Vehicle>> {
        return this.api.postWithResponse<Vehicle>(this.endpoint, vehicle);
    }

    // Update vehicle status
    updateVehicleStatus(id: number, request: UpdateVehicleStatusRequest): Observable<ApiResponse<null>> {
        return this.api.patch<ApiResponse<null>>(`${this.endpoint}/${id}/status`, request);
    }

    // Update vehicle details
    updateVehicle(id: number, request: UpdateVehicleRequest): Observable<ApiResponse<Vehicle>> {
        return this.api.put<ApiResponse<Vehicle>>(`${this.endpoint}/${id}`, request);
    }

    // Get vehicle status counts
    getVehicleStatusCounts(companyId?: number): Observable<VehicleStatusCount> {
        const params = companyId ? { companyId, pageNumber: 1, pageSize: 1000 } : { pageNumber: 1, pageSize: 1000 };
        return this.api.get<ApiResponse<Vehicle[]>>(this.endpoint, params).pipe(
            map(response => {
                const vehicles = response.data || [];
                return {
                    all: vehicles.length,
                    available: vehicles.filter(v => v.currentStatus === 'available').length,
                    in_use: vehicles.filter(v => v.currentStatus === 'in_transit').length,
                    maintenance: vehicles.filter(v => v.currentStatus === 'maintenance').length,
                    inactive: vehicles.filter(v => v.currentStatus === 'inactive').length
                };
            })
        );
    }

    // Get status config
    getStatusConfig(status: string) {
        return VEHICLE_STATUS_CONFIG[status as keyof typeof VEHICLE_STATUS_CONFIG];
    }

    // Get type config
    getTypeConfig(type: string) {
        return VEHICLE_TYPE_CONFIG[type as keyof typeof VEHICLE_TYPE_CONFIG];
    }
}
