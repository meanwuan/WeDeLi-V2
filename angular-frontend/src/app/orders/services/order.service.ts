import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../auth/services/auth.service';
import {
    Order,
    OrderListItem,
    OrderStatus,
    CreateOrderRequest,
    UpdateOrderStatusRequest,
    AssignOrderRequest,
    OrderStatistics
} from '../../core/models/order.model';
import { PaginatedResponse, ApiResponse } from '../../core/models/common.model';

export interface OrderFilters {
    status?: OrderStatus;
    searchTerm?: string;
    fromDate?: string;
    toDate?: string;
    driverId?: number;
    customerId?: number;
    companyId?: number;
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
}

export interface OrderStatusCount {
    all: number;
    pending: number;
    in_transit: number;
    delivered: number;
    cancelled: number;
}

@Injectable({
    providedIn: 'root'
})
export class OrderService {
    private api = inject(ApiService);
    private authService = inject(AuthService);
    private readonly endpoint = '/orders';

    // Get current user's companyId
    private getCompanyId(): number | null {
        const user = this.authService.getCurrentUser();
        return user?.companyId ?? null;
    }

    // Get orders with filters and pagination
    // For CompanyAdmin users, automatically filters by their companyId
    getOrders(filters: OrderFilters): Observable<PaginatedResponse<OrderListItem>> {
        const params: Record<string, any> = {
            pageNumber: filters.pageNumber,
            pageSize: filters.pageSize
        };

        if (filters.searchTerm) params['searchTerm'] = filters.searchTerm;
        if (filters.fromDate) params['fromDate'] = filters.fromDate;
        if (filters.toDate) params['toDate'] = filters.toDate;
        if (filters.driverId) params['driverId'] = filters.driverId;
        if (filters.customerId) params['customerId'] = filters.customerId;
        if (filters.sortBy) params['sortBy'] = filters.sortBy;
        if (filters.sortOrder) params['sortOrder'] = filters.sortOrder;

        // Get company ID from filters or current user
        const companyId = filters.companyId ?? this.getCompanyId();

        // Determine endpoint based on filters and user role
        let endpoint: string;
        if (filters.status) {
            // Status-specific endpoint with company filter
            endpoint = `${this.endpoint}/status/${filters.status}`;
            if (companyId) params['companyId'] = companyId;
        } else if (companyId) {
            // Company-specific endpoint for transport companies
            endpoint = `${this.endpoint}/company/${companyId}`;
        } else {
            // All orders (SuperAdmin only)
            endpoint = this.endpoint;
        }

        // Backend returns ApiResponse<OrderListItem[]>, need to map to PaginatedResponse
        return this.api.get<ApiResponse<OrderListItem[]>>(endpoint, params).pipe(
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

    // Get orders by status
    getOrdersByStatus(status: OrderStatus, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<OrderListItem>> {
        return this.api.get<PaginatedResponse<OrderListItem>>(`${this.endpoint}/status/${status}`, {
            pageNumber: page,
            pageSize
        });
    }

    // Get single order by ID
    getOrderById(id: number): Observable<Order> {
        return this.api.get<Order>(`${this.endpoint}/${id}`);
    }

    // Get order by tracking code
    getOrderByTrackingCode(trackingCode: string): Observable<Order> {
        return this.api.get<Order>(`${this.endpoint}/tracking/${trackingCode}`);
    }

    // Search orders
    searchOrders(searchTerm: string, page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<OrderListItem>> {
        return this.api.get<PaginatedResponse<OrderListItem>>(`${this.endpoint}/search`, {
            searchTerm,
            pageNumber: page,
            pageSize
        });
    }

    // Create new order
    createOrder(order: CreateOrderRequest): Observable<ApiResponse<Order>> {
        return this.api.postWithResponse<Order>(this.endpoint, order);
    }

    // Update order
    updateOrder(id: number, order: any): Observable<ApiResponse<Order>> {
        return this.api.put<ApiResponse<Order>>(`${this.endpoint}/${id}`, order);
    }

    // Update order status
    updateOrderStatus(id: number, request: UpdateOrderStatusRequest): Observable<ApiResponse<null>> {
        return this.api.patch<ApiResponse<null>>(`${this.endpoint}/${id}/status`, request);
    }

    // Assign driver and vehicle
    assignOrder(request: AssignOrderRequest): Observable<ApiResponse<null>> {
        return this.api.postWithResponse<null>(`${this.endpoint}/${request.orderId}/assign`, request);
    }

    // Cancel order
    cancelOrder(id: number, reason: string): Observable<ApiResponse<null>> {
        return this.api.postWithResponse<null>(`${this.endpoint}/${id}/cancel`, {
            cancellationReason: reason,
            refundRequested: false
        });
    }

    // Get order statistics - counts orders by status (filtered by company for CompanyAdmin)
    getOrderStatusCounts(): Observable<OrderStatusCount> {
        const companyId = this.getCompanyId();
        const endpoint = companyId
            ? `${this.endpoint}/company/${companyId}`
            : this.endpoint;

        return this.api.get<ApiResponse<OrderListItem[]>>(endpoint, { pageNumber: 1, pageSize: 1000 }).pipe(
            map(response => {
                const orders = response.data || [];
                return {
                    all: orders.length,
                    pending: orders.filter(o => o.orderStatus === 'pending' || o.orderStatus === 'pending_pickup').length,
                    in_transit: orders.filter(o => o.orderStatus === 'in_transit').length,
                    delivered: orders.filter(o => o.orderStatus === 'delivered').length,
                    cancelled: orders.filter(o => o.orderStatus === 'cancelled').length
                };
            })
        );
    }

    // Get pending orders
    getPendingOrders(page: number = 1, pageSize: number = 10): Observable<PaginatedResponse<OrderListItem>> {
        return this.api.get<PaginatedResponse<OrderListItem>>(`${this.endpoint}/pending`, {
            pageNumber: page,
            pageSize
        });
    }
}
