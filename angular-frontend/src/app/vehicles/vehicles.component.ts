import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StatusBadgeComponent } from '../shared/components/status-badge/status-badge.component';
import { VehicleTrackingModalComponent } from './components/vehicle-tracking-modal.component';
import { VehiclesMapComponent } from './components/vehicles-map.component';
import { VehicleService, VehicleFilters, VehicleStatusCount } from './services/vehicle.service';
import { Vehicle, VehicleStatus, VEHICLE_STATUS_CONFIG, VEHICLE_TYPE_CONFIG, UpdateVehicleRequest } from '../core/models/vehicle.model';
import { AuthService } from '../auth/services/auth.service';

type VehicleTab = 'all' | 'available' | 'in_use' | 'maintenance' | 'inactive';

interface Tab {
    id: VehicleTab;
    label: string;
    status?: VehicleStatus;
}

@Component({
    selector: 'app-vehicles',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, StatusBadgeComponent, VehicleTrackingModalComponent, VehiclesMapComponent],
    templateUrl: './vehicles.component.html',
    styleUrl: './vehicles.component.scss'
})
export class VehiclesComponent implements OnInit {
    private vehicleService = inject(VehicleService);
    private authService = inject(AuthService);

    // User's company ID
    private get companyId(): number | undefined {
        return this.authService.getCurrentUser()?.companyId ?? undefined;
    }

    // State
    vehicles = signal<Vehicle[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);

    // Selected vehicle for detail panel
    selectedVehicle = signal<Vehicle | null>(null);

    // Tracking modal
    showTrackingModal = signal(false);
    trackingVehicle = signal<Vehicle | null>(null);
    showDetailPanel = signal(false);

    // Edit modal
    showEditModal = signal(false);
    editingVehicle = signal<Vehicle | null>(null);
    saving = signal(false);
    editFormData: UpdateVehicleRequest = {};

    // Pagination
    currentPage = signal(1);
    pageSize = signal(20);
    totalItems = signal(0);
    totalPages = computed(() => Math.ceil(this.totalItems() / this.pageSize()));

    // Filters
    activeTab = signal<VehicleTab>('all');
    searchTerm = signal('');
    selectedType = signal<string>('');

    // Status counts
    statusCounts = signal<VehicleStatusCount>({
        all: 0,
        available: 0,
        in_use: 0,
        maintenance: 0,
        inactive: 0
    });

    // View mode: list or map
    viewMode = signal<'list' | 'map'>('list');

    tabs: Tab[] = [
        { id: 'all', label: 'Tất cả' },
        { id: 'available', label: 'Sẵn sàng', status: 'available' },
        { id: 'in_use', label: 'Đang chạy', status: 'in_transit' },
        { id: 'maintenance', label: 'Bảo trì', status: 'maintenance' },
        { id: 'inactive', label: 'Ngừng hoạt động', status: 'inactive' }
    ];

    vehicleTypes = [
        { value: '', label: 'Tất cả loại xe' },
        { value: 'truck', label: 'Xe tải' },
        { value: 'van', label: 'Xe van' },
        { value: 'motorbike', label: 'Xe máy' }
    ];

    ngOnInit(): void {
        this.loadStatusCounts();
        this.loadVehicles();
    }

    loadStatusCounts(): void {
        this.vehicleService.getVehicleStatusCounts(this.companyId).subscribe({
            next: (counts) => this.statusCounts.set(counts),
            error: (err) => console.error('Failed to load status counts:', err)
        });
    }

    loadVehicles(): void {
        this.loading.set(true);
        this.error.set(null);

        const filters: VehicleFilters = {
            pageNumber: this.currentPage(),
            pageSize: this.pageSize(),
            searchTerm: this.searchTerm() || undefined,
            vehicleType: this.selectedType() || undefined,
            companyId: this.companyId
        };

        const tab = this.tabs.find(t => t.id === this.activeTab());
        if (tab?.status) {
            filters.status = tab.status;
        }

        this.vehicleService.getVehicles(filters).subscribe({
            next: (response) => {
                this.vehicles.set(response.items || []);
                this.totalItems.set(response.totalCount || 0);
                this.loading.set(false);
            },
            error: (err) => {
                this.error.set(err.message || 'Không thể tải danh sách xe');
                this.loading.set(false);
                this.vehicles.set([]);
            }
        });
    }

    onTabChange(tabId: VehicleTab): void {
        this.activeTab.set(tabId);
        this.currentPage.set(1);
        this.loadVehicles();
    }

    onSearch(): void {
        this.currentPage.set(1);
        this.loadVehicles();
    }

    onTypeFilterChange(): void {
        this.currentPage.set(1);
        this.loadVehicles();
    }

    onPageChange(page: number): void {
        if (page >= 1 && page <= this.totalPages()) {
            this.currentPage.set(page);
            this.loadVehicles();
        }
    }

    toggleViewMode(): void {
        this.viewMode.set(this.viewMode() === 'list' ? 'map' : 'list');
    }

    selectVehicle(vehicle: Vehicle): void {
        this.selectedVehicle.set(vehicle);
        this.showDetailPanel.set(true);
    }

    closeDetailPanel(): void {
        this.showDetailPanel.set(false);
        this.selectedVehicle.set(null);
    }

    // Status helpers
    getStatusVariant(status: string): 'success' | 'warning' | 'danger' | 'info' | 'default' {
        switch (status) {
            case 'available': return 'success';
            case 'in_transit': return 'info';
            case 'maintenance': return 'warning';
            case 'inactive': return 'danger';
            default: return 'default';
        }
    }

    getStatusLabel(status: string): string {
        return VEHICLE_STATUS_CONFIG[status as keyof typeof VEHICLE_STATUS_CONFIG]?.label || status;
    }

    getTypeLabel(type: string): string {
        return VEHICLE_TYPE_CONFIG[type as keyof typeof VEHICLE_TYPE_CONFIG]?.label || type;
    }

    getTypeIcon(type: string): string {
        return VEHICLE_TYPE_CONFIG[type as keyof typeof VEHICLE_TYPE_CONFIG]?.icon || '🚗';
    }

    getTabCount(tabId: VehicleTab): number {
        return this.statusCounts()[tabId] || 0;
    }

    // Format capacity
    formatCapacity(capacity: number): string {
        if (capacity >= 1000) {
            return (capacity / 1000).toFixed(1) + ' tấn';
        }
        return capacity + ' kg';
    }

    // Open tracking modal
    openTracking(vehicle: Vehicle): void {
        this.trackingVehicle.set(vehicle);
        this.showTrackingModal.set(true);
    }

    // Close tracking modal
    closeTracking(): void {
        this.showTrackingModal.set(false);
        this.trackingVehicle.set(null);
    }

    // Open edit modal
    openEditModal(vehicle: Vehicle): void {
        this.editingVehicle.set(vehicle);
        this.editFormData = {
            licensePlate: vehicle.licensePlate,
            vehicleType: vehicle.vehicleType,
            maxWeightKg: vehicle.maxWeightKg,
            maxVolumeM3: vehicle.maxVolumeM3 ?? undefined,
            overloadThreshold: vehicle.overloadThreshold,
            allowOverload: vehicle.allowOverload,
            currentStatus: vehicle.currentStatus,
            gpsEnabled: vehicle.gpsEnabled
        };
        this.showEditModal.set(true);
    }

    // Close edit modal
    closeEditModal(): void {
        this.showEditModal.set(false);
        this.editingVehicle.set(null);
        this.editFormData = {};
    }

    // Save vehicle changes
    saveVehicle(): void {
        const vehicle = this.editingVehicle();
        if (!vehicle) return;

        this.saving.set(true);
        this.vehicleService.updateVehicle(vehicle.vehicleId, this.editFormData).subscribe({
            next: () => {
                this.saving.set(false);
                this.closeEditModal();
                this.loadVehicles();
                this.loadStatusCounts();
            },
            error: (err) => {
                this.saving.set(false);
                console.error('Failed to update vehicle:', err);
                this.error.set('Không thể cập nhật xe');
            }
        });
    }

    // Status options for dropdown
    statusOptions = [
        { value: 'available', label: 'Sẵn sàng' },
        { value: 'in_transit', label: 'Đang chạy' },
        { value: 'maintenance', label: 'Bảo trì' },
        { value: 'inactive', label: 'Không hoạt động' }
    ];
}
