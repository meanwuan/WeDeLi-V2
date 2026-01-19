// Order Models - matching backend DTOs

// Order Status enum
export type OrderStatus =
    | 'pending'
    | 'pending_pickup'
    | 'picked_up'
    | 'in_transit'
    | 'out_for_delivery'
    | 'delivered'
    | 'returned'
    | 'cancelled';

// Payment Status
export type PaymentStatus = 'unpaid' | 'paid' | 'refunded';

// Payment Method
export type PaymentMethod = 'cash' | 'bank_transfer' | 'e_wallet' | 'periodic';

// Parcel Type
export type ParcelType = 'fragile' | 'electronics' | 'food' | 'cold' | 'document' | 'other';

// Order Response DTO
export interface Order {
    orderId: number;
    trackingCode: string;
    customerId: number;
    customerName: string;
    customerPhone: string;
    isRegularCustomer: boolean;
    senderName: string;
    senderPhone: string;
    senderAddress: string;
    receiverName: string;
    receiverPhone: string;
    receiverAddress: string;
    receiverProvince: string;
    receiverDistrict: string;
    parcelType: ParcelType;
    weightKg: number;
    declaredValue: number | null;
    specialInstructions: string | null;
    routeId: number | null;
    routeName: string | null;
    vehicleId: number | null;
    vehicleLicensePlate: string | null;
    driverId: number | null;
    driverName: string | null;
    driverPhone: string | null;
    shippingFee: number;
    codAmount: number;
    paymentMethod: PaymentMethod;
    paymentStatus: PaymentStatus;
    paidAt: string | null;
    orderStatus: OrderStatus;
    createdAt: string;
    pickupScheduledAt: string | null;
    pickupConfirmedAt: string | null;
    deliveredAt: string | null;
    updatedAt: string | null;
    photos?: OrderPhoto[];
    statusHistory?: OrderStatusHistory[];
}

// Order List Item (lighter for tables)
export interface OrderListItem {
    orderId: number;
    trackingCode: string;
    customerName: string;
    receiverName: string;
    receiverPhone: string;
    receiverAddress: string;
    orderStatus: OrderStatus;
    shippingFee: number;
    codAmount: number;
    paymentStatus: PaymentStatus;
    driverName: string | null;
    vehicleLicensePlate: string | null;
    createdAt: string;
    deliveredAt: string | null;
}

// Order Photo
export interface OrderPhoto {
    photoId: number;
    photoType: string;
    photoUrl: string;
    fileName: string;
    fileSizeKb: number | null;
    uploadedBy: number | null;
    uploadedByName: string | null;
    uploadedAt: string;
}

// Order Status History
export interface OrderStatusHistory {
    historyId: number;
    oldStatus: string | null;
    newStatus: string;
    updatedBy: number | null;
    updatedByName: string | null;
    location: string | null;
    notes: string | null;
    createdAt: string;
}

// Create Order Request
export interface CreateOrderRequest {
    customerId: number;
    senderName: string;
    senderPhone: string;
    senderAddress: string;
    receiverName: string;
    receiverPhone: string;
    receiverAddress: string;
    receiverProvince: string;
    receiverDistrict: string;
    receiverWard?: string;
    parcelType: ParcelType;
    weightKg: number;
    declaredValue?: number;
    specialInstructions?: string;
    routeId?: number;
    vehicleId?: number;
    driverId?: number;
    paymentMethod: PaymentMethod;
    codAmount?: number;
    pickupScheduledAt?: string;
}

// Update Order Status Request
export interface UpdateOrderStatusRequest {
    newStatus: OrderStatus;
    location?: string;
    notes?: string;
}

// Assign Order Request
export interface AssignOrderRequest {
    orderId: number;
    driverId: number;
    vehicleId: number;
    routeId?: number;
    tripId?: number;
    scheduledPickupTime?: string;
    notes?: string;
}

// Order Statistics
export interface OrderStatistics {
    totalOrders: number;
    pendingOrders: number;
    inTransitOrders: number;
    deliveredOrders: number;
    cancelledOrders: number;
    returnedOrders: number;
    totalRevenue: number;
    totalCodAmount: number;
    averageShippingFee: number;
    successRate: number;
}

// Order status labels and colors
export const ORDER_STATUS_CONFIG: Record<OrderStatus, { label: string; color: string; bgColor: string }> = {
    'pending': { label: 'Chờ xử lý', color: '#f59e0b', bgColor: '#fef3c7' },
    'pending_pickup': { label: 'Chờ lấy hàng', color: '#8b5cf6', bgColor: '#ede9fe' },
    'picked_up': { label: 'Đã lấy hàng', color: '#06b6d4', bgColor: '#cffafe' },
    'in_transit': { label: 'Đang giao', color: '#3b82f6', bgColor: '#dbeafe' },
    'out_for_delivery': { label: 'Đang giao', color: '#3b82f6', bgColor: '#dbeafe' },
    'delivered': { label: 'Hoàn thành', color: '#22c55e', bgColor: '#dcfce7' },
    'returned': { label: 'Hoàn trả', color: '#ef4444', bgColor: '#fee2e2' },
    'cancelled': { label: 'Đã hủy', color: '#6b7280', bgColor: '#f3f4f6' }
};
