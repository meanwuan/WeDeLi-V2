import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';
import { ApiResponse } from '../../core/models/common.model';

// Company Customer Response DTO
export interface CompanyCustomer {
    companyCustomerId: number;
    companyId: number;
    customerId: number | null;
    fullName: string;
    phone: string;
    email: string | null;
    customPrice: number | null;
    discountPercent: number | null;
    isVip: boolean;
    notes: string | null;
    totalOrders: number;
    totalRevenue: number;
    createdAt: string | null;
    updatedAt: string | null;
    pricingDescription: string;
}

// Create/Update DTO
export interface CompanyCustomerRequest {
    customerId?: number | null;
    fullName: string;
    phone: string;
    email?: string | null;
    customPrice?: number | null;
    discountPercent?: number | null;
    isVip?: boolean;
    notes?: string | null;
}

// Pricing update DTO
export interface PricingUpdate {
    customPrice: number | null;
    discountPercent: number | null;
}

// VIP status update
export interface VipUpdate {
    isVip: boolean;
}

// Price calculation
export interface PriceCalculation {
    basePrice: number;
    finalPrice: number;
    discountAmount: number;
    pricingType: string;
    isCustomerFound: boolean;
}

@Injectable({
    providedIn: 'root'
})
export class CompanyCustomerService {
    private api = inject(ApiService);
    private readonly endpoint = '/company-customers';

    // Get all customers for a company
    getCompanyCustomers(companyId: number): Observable<CompanyCustomer[]> {
        return this.api.get<ApiResponse<CompanyCustomer[]>>(
            this.endpoint,
            { companyId }
        ).pipe(
            map(response => response.data || [])
        );
    }

    // Get VIP customers
    getVipCustomers(companyId: number): Observable<CompanyCustomer[]> {
        return this.api.get<ApiResponse<CompanyCustomer[]>>(
            `${this.endpoint}/vip`,
            { companyId }
        ).pipe(
            map(response => response.data || [])
        );
    }

    // Get customer by ID
    getCustomerById(id: number): Observable<CompanyCustomer> {
        return this.api.get<ApiResponse<CompanyCustomer>>(
            `${this.endpoint}/${id}`
        ).pipe(
            map(response => response.data!)
        );
    }

    // Create or update customer
    createOrUpdate(companyId: number, request: CompanyCustomerRequest): Observable<CompanyCustomer> {
        return this.api.post<ApiResponse<CompanyCustomer>>(
            `${this.endpoint}?companyId=${companyId}`,
            request
        ).pipe(
            map(response => response.data!)
        );
    }

    // Set pricing
    setPricing(id: number, pricing: PricingUpdate): Observable<CompanyCustomer> {
        return this.api.put<ApiResponse<CompanyCustomer>>(
            `${this.endpoint}/${id}/pricing`,
            pricing
        ).pipe(
            map(response => response.data!)
        );
    }

    // Set VIP status
    setVipStatus(id: number, isVip: boolean): Observable<CompanyCustomer> {
        return this.api.put<ApiResponse<CompanyCustomer>>(
            `${this.endpoint}/${id}/vip`,
            { isVip }
        ).pipe(
            map(response => response.data!)
        );
    }

    // Calculate price
    calculatePrice(companyId: number, phone: string, basePrice: number): Observable<PriceCalculation> {
        return this.api.post<ApiResponse<PriceCalculation>>(
            `${this.endpoint}/calculate-price`,
            { companyId, phone, basePrice }
        ).pipe(
            map(response => response.data!)
        );
    }

    // Delete customer
    deleteCustomer(id: number): Observable<boolean> {
        return this.api.delete<ApiResponse<boolean>>(
            `${this.endpoint}/${id}`
        ).pipe(
            map(response => response.data!)
        );
    }

    // Helper: Format price
    formatPrice(price: number | null): string {
        if (price === null) return 'Giá mặc định';
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(price);
    }

    // Helper: Format discount
    formatDiscount(percent: number | null): string {
        if (percent === null) return '-';
        return `${percent}%`;
    }
}
