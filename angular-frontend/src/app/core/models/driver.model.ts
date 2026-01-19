// Driver Models - matching backend DTOs

// Driver Response DTO
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

// Driver Detail DTO
export interface DriverDetail extends Driver {
    currentTripId: number | null;
    currentVehicleId: number | null;
    currentVehicleLicensePlate: string | null;
    currentTripStatus: string | null;
    totalDeliveries: number;
    successfulDeliveries: number;
    failedDeliveries: number;
    pendingDeliveries: number;
    totalCodCollected: number;
    pendingCodAmount: number;
    totalEarnings: number;
    averageDeliveryTime: number;
    totalRatings: number;
    averageRating: number;
    lastActiveDate: string | null;
    tripsThisMonth: number;
    tripsLastMonth: number;
}

// Driver Assignment (for dropdown)
export interface DriverAssignment {
    driverId: number;
    driverName: string;
    currentVehicleId: number;
    vehicleLicensePlate: string;
    currentOrderCount: number;
    currentLoadWeight: number;
    isAvailable: boolean;
    lastDeliveryTime: string | null;
}

// Driver Statistics
export interface DriverStatistics {
    driverId: number;
    driverName: string;
    totalTrips: number;
    completedTrips: number;
    inProgressTrips: number;
    cancelledTrips: number;
    totalDeliveries: number;
    successfulDeliveries: number;
    failedDeliveries: number;
    successRate: number;
    averageDeliveryTime: number;
    onTimeDeliveries: number;
    lateDeliveries: number;
    onTimeRate: number;
    totalCodCollected: number;
    totalCodSubmitted: number;
    pendingCodAmount: number;
    totalRatings: number;
    averageRating: number;
    fromDate: string;
    toDate: string;
}

// Create Driver Request
export interface CreateDriverRequest {
    userId: number;
    companyId: number;
    driverLicense: string;
    licenseExpiry: string;
}

// Update Driver Request
export interface UpdateDriverRequest {
    driverLicense?: string;
    licenseExpiry?: string;
    isActive?: boolean;
}
