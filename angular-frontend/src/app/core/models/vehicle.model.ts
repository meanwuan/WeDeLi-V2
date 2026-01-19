// Vehicle Models - matching backend DTOs

// Vehicle Status
export type VehicleStatus = 'available' | 'in_transit' | 'maintenance' | 'inactive' | 'overloaded';

// Vehicle Type
export type VehicleType = 'truck' | 'van' | 'motorbike';

// Vehicle Response DTO
export interface Vehicle {
    vehicleId: number;
    companyId: number;
    companyName: string;
    licensePlate: string;
    vehicleType: VehicleType;
    maxWeightKg: number;
    maxVolumeM3: number | null;
    currentWeightKg: number;
    capacityPercentage: number;
    overloadThreshold: number;
    allowOverload: boolean;
    currentStatus: VehicleStatus;
    gpsEnabled: boolean;
    createdAt: string;
    // Alias properties for component compatibility
    status?: VehicleStatus;
    capacity?: number;
    driverName?: string | null;
}

// Vehicle Detail DTO
export interface VehicleDetail extends Vehicle {
    currentDriverId: number | null;
    currentDriverName: string | null;
    currentTripId: number | null;
    currentRouteName: string | null;
    currentOrderCount: number;
    availableWeightKg: number;
    availableVolumeM3: number;
    isOverloaded: boolean;
    totalTripsCompleted: number;
    totalOrdersDelivered: number;
    totalDistanceKm: number;
    lastMaintenanceDate: string | null;
    nextMaintenanceDate: string | null;
    daysUntilMaintenance: number;
    lastActiveDate: string | null;
    tripsThisMonth: number;
    tripsLastMonth: number;
}

// Vehicle Location (for tracking)
export interface VehicleLocation {
    vehicleId: number;
    licensePlate: string;
    latitude: number;
    longitude: number;
    speed: number;
    heading: number;
    timestamp: string;
    address?: string;
}

// Vehicle Capacity
export interface VehicleCapacity {
    vehicleId: number;
    licensePlate: string;
    maxWeightKg: number;
    currentWeightKg: number;
    availableWeightKg: number;
    capacityPercentage: number;
    overloadThreshold: number;
    isOverloaded: boolean;
    allowOverload: boolean;
    currentOrderCount: number;
    canAcceptMoreOrders: boolean;
}

// Create Vehicle Request
export interface CreateVehicleRequest {
    companyId: number;
    licensePlate: string;
    vehicleType: VehicleType;
    maxWeightKg: number;
    maxVolumeM3?: number;
    overloadThreshold?: number;
    gpsEnabled?: boolean;
}

// Update Vehicle Status Request
export interface UpdateVehicleStatusRequest {
    status: VehicleStatus;
    notes?: string;
}

// Update Vehicle Request (for editing vehicle details)
export interface UpdateVehicleRequest {
    licensePlate?: string;
    vehicleType?: string;
    maxWeightKg?: number;
    maxVolumeM3?: number;
    overloadThreshold?: number;
    allowOverload?: boolean;
    currentStatus?: string;
    gpsEnabled?: boolean;
}

// Vehicle status labels and colors
export const VEHICLE_STATUS_CONFIG: Record<VehicleStatus, { label: string; color: string; bgColor: string }> = {
    'available': { label: 'Sẵn sàng', color: '#22c55e', bgColor: '#dcfce7' },
    'in_transit': { label: 'Đang chạy', color: '#3b82f6', bgColor: '#dbeafe' },
    'maintenance': { label: 'Bảo trì', color: '#f59e0b', bgColor: '#fef3c7' },
    'inactive': { label: 'Không hoạt động', color: '#6b7280', bgColor: '#f3f4f6' },
    'overloaded': { label: 'Quá tải', color: '#ef4444', bgColor: '#fee2e2' }
};

// Vehicle type labels
export const VEHICLE_TYPE_CONFIG: Record<VehicleType, { label: string; icon: string }> = {
    'truck': { label: 'Xe tải', icon: '🚚' },
    'van': { label: 'Xe van', icon: '🚐' },
    'motorbike': { label: 'Xe máy', icon: '🏍️' }
};
