// Common Models - shared DTOs

// API Response wrapper
export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T | null;
    errors?: string[];
}

// Paginated response
export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
}

// Pagination request
export interface PaginationRequest {
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortOrder?: 'asc' | 'desc';
}

// Search request
export interface SearchRequest extends PaginationRequest {
    searchTerm?: string;
}

// Date range filter
export interface DateRange {
    fromDate: string;
    toDate: string;
}

// Key-Value for dropdowns
export interface KeyValue<TKey = number> {
    key: TKey;
    value: string;
}

// Notification
export interface Notification {
    notificationId: number;
    userId: number;
    orderId: number | null;
    notificationType: string;
    title: string;
    message: string;
    isRead: boolean;
    sentVia: string;
    createdAt: string;
}

// Export index
export * from './dashboard.model';
export * from './order.model';
export * from './vehicle.model';
export * from './driver.model';
