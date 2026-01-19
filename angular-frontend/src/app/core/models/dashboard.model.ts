// Dashboard Models - matching backend DTOs
export interface DashboardStats {
    totalOrders: number;
    pendingOrders: number;
    inTransitOrders: number;
    deliveredOrders: number;
    totalRevenue: number;
    todayRevenue: number;
    activeVehicles: number;
    activeDrivers: number;
    pendingComplaints: number;
    pendingCodAmount: number;
}

// Daily revenue for chart
export interface DailyRevenue {
    date: string;
    revenue: number;
    orderCount: number;
}

// Order status summary
export interface OrderStatusSummary {
    status: string;
    count: number;
    label: string;
    color: string;
}
