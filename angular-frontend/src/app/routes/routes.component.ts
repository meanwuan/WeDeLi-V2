import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../core/services/api.service';
import { AuthService } from '../auth/services/auth.service';

interface Route {
    routeId: number;
    companyId: number;
    companyName?: string;
    routeName: string;
    originProvince: string;
    originDistrict?: string;
    destinationProvince: string;
    destinationDistrict?: string;
    distanceKm?: number;
    estimatedDurationHours?: number;
    basePrice: number;
    isActive: boolean;
    createdAt: Date;
}

interface RouteStats {
    totalRoutes: number;
    activeRoutes: number;
    averagePrice: number;
    newThisMonth: number;
}

@Component({
    selector: 'app-routes',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './routes.component.html',
    styleUrl: './routes.component.scss'
})
export class RoutesComponent implements OnInit {
    private api = inject(ApiService);
    private authService = inject(AuthService);

    // State
    routes = signal<Route[]>([]);
    loading = signal(true);
    stats = signal<RouteStats>({
        totalRoutes: 0,
        activeRoutes: 0,
        averagePrice: 0,
        newThisMonth: 0
    });

    // Filters
    filterOrigin = '';
    filterDestination = '';
    filterActiveOnly = false;

    // Pagination
    currentPage = 1;
    pageSize = 10;
    totalItems = 0;

    // Modal state
    showModal = signal(false);
    showDeleteModal = signal(false);
    isEditing = signal(false);
    selectedRoute: Route | null = null;

    // Form data
    formData = {
        routeName: '',
        originProvince: '',
        originDistrict: '',
        destinationProvince: '',
        destinationDistrict: '',
        distanceKm: null as number | null,
        estimatedDurationHours: null as number | null,
        basePrice: 0,
        isActive: true
    };

    // Vietnam provinces
    provinces = [
        'An Giang', 'Bà Rịa - Vũng Tàu', 'Bạc Liêu', 'Bắc Giang', 'Bắc Kạn',
        'Bắc Ninh', 'Bến Tre', 'Bình Dương', 'Bình Định', 'Bình Phước',
        'Bình Thuận', 'Cà Mau', 'Cao Bằng', 'Cần Thơ', 'Đà Nẵng',
        'Đắk Lắk', 'Đắk Nông', 'Điện Biên', 'Đồng Nai', 'Đồng Tháp',
        'Gia Lai', 'Hà Giang', 'Hà Nam', 'Hà Nội', 'Hà Tĩnh',
        'Hải Dương', 'Hải Phòng', 'Hậu Giang', 'Hòa Bình', 'Hưng Yên',
        'Khánh Hòa', 'Kiên Giang', 'Kon Tum', 'Lai Châu', 'Lâm Đồng',
        'Lạng Sơn', 'Lào Cai', 'Long An', 'Nam Định', 'Nghệ An',
        'Ninh Bình', 'Ninh Thuận', 'Phú Thọ', 'Phú Yên', 'Quảng Bình',
        'Quảng Nam', 'Quảng Ngãi', 'Quảng Ninh', 'Quảng Trị', 'Sóc Trăng',
        'Sơn La', 'Tây Ninh', 'Thái Bình', 'Thái Nguyên', 'Thanh Hóa',
        'Thừa Thiên Huế', 'Tiền Giang', 'TP. Hồ Chí Minh', 'Trà Vinh', 'Tuyên Quang',
        'Vĩnh Long', 'Vĩnh Phúc', 'Yên Bái'
    ];

    private get companyId(): number {
        return this.authService.getCurrentUser()?.companyId ?? 0;
    }

    ngOnInit(): void {
        this.loadRoutes();
    }

    loadRoutes(): void {
        this.loading.set(true);
        this.api.get<any>(`/routes/company/${this.companyId}`).subscribe({
            next: (response) => {
                let routes = response.data || response || [];
                // Use demo data if no routes from backend
                if (routes.length === 0) {
                    routes = this.getDemoRoutes();
                }
                this.routes.set(routes);
                this.totalItems = routes.length;
                this.calculateStats(routes);
                this.loading.set(false);
            },
            error: () => {
                // On error, show demo routes for visualization
                const demoRoutes = this.getDemoRoutes();
                this.routes.set(demoRoutes);
                this.totalItems = demoRoutes.length;
                this.calculateStats(demoRoutes);
                this.loading.set(false);
            }
        });
    }

    // Demo routes for visualization when no backend data
    private getDemoRoutes(): Route[] {
        return [
            {
                routeId: 1, companyId: 1, routeName: 'Sài Gòn - Đà Nẵng',
                originProvince: 'TP. Hồ Chí Minh', destinationProvince: 'Đà Nẵng',
                distanceKm: 960, estimatedDurationHours: 18, basePrice: 1550000,
                isActive: true, createdAt: new Date()
            },
            {
                routeId: 2, companyId: 1, routeName: 'Hà Nội - Hải Phòng',
                originProvince: 'Hà Nội', destinationProvince: 'Hải Phòng',
                distanceKm: 120, estimatedDurationHours: 2.5, basePrice: 450000,
                isActive: true, createdAt: new Date()
            },
            {
                routeId: 3, companyId: 1, routeName: 'Đà Nẵng - Quy Nhơn',
                originProvince: 'Đà Nẵng', destinationProvince: 'Bình Định',
                distanceKm: 320, estimatedDurationHours: 6, basePrice: 800000,
                isActive: true, createdAt: new Date()
            },
            {
                routeId: 4, companyId: 1, routeName: 'Sài Gòn - Cần Thơ',
                originProvince: 'TP. Hồ Chí Minh', destinationProvince: 'Cần Thơ',
                distanceKm: 170, estimatedDurationHours: 3.5, basePrice: 350000,
                isActive: true, createdAt: new Date()
            },
            {
                routeId: 5, companyId: 1, routeName: 'Hà Nội - Thanh Hóa',
                originProvince: 'Hà Nội', destinationProvince: 'Thanh Hóa',
                distanceKm: 150, estimatedDurationHours: 3, basePrice: 400000,
                isActive: false, createdAt: new Date()
            }
        ];
    }


    calculateStats(routes: Route[]): void {
        const activeRoutes = routes.filter(r => r.isActive);
        const avgPrice = routes.length > 0
            ? routes.reduce((sum, r) => sum + r.basePrice, 0) / routes.length
            : 0;
        const thisMonth = new Date();
        thisMonth.setDate(1);
        const newThisMonth = routes.filter(r => new Date(r.createdAt) >= thisMonth).length;

        this.stats.set({
            totalRoutes: routes.length,
            activeRoutes: activeRoutes.length,
            averagePrice: avgPrice,
            newThisMonth: newThisMonth
        });
    }

    get filteredRoutes(): Route[] {
        let result = this.routes();

        if (this.filterOrigin) {
            result = result.filter(r => r.originProvince === this.filterOrigin);
        }
        if (this.filterDestination) {
            result = result.filter(r => r.destinationProvince === this.filterDestination);
        }
        if (this.filterActiveOnly) {
            result = result.filter(r => r.isActive);
        }

        // Pagination
        const start = (this.currentPage - 1) * this.pageSize;
        return result.slice(start, start + this.pageSize);
    }

    get totalPages(): number {
        let count = this.routes().length;
        if (this.filterOrigin) count = this.routes().filter(r => r.originProvince === this.filterOrigin).length;
        if (this.filterDestination) count = this.routes().filter(r => r.destinationProvince === this.filterDestination).length;
        if (this.filterActiveOnly) count = this.routes().filter(r => r.isActive).length;
        return Math.ceil(count / this.pageSize);
    }

    // Modal actions
    openCreateModal(): void {
        this.isEditing.set(false);
        this.selectedRoute = null;
        this.resetForm();
        this.showModal.set(true);
    }

    openEditModal(route: Route): void {
        this.isEditing.set(true);
        this.selectedRoute = route;
        this.formData = {
            routeName: route.routeName,
            originProvince: route.originProvince,
            originDistrict: route.originDistrict || '',
            destinationProvince: route.destinationProvince,
            destinationDistrict: route.destinationDistrict || '',
            distanceKm: route.distanceKm || null,
            estimatedDurationHours: route.estimatedDurationHours || null,
            basePrice: route.basePrice,
            isActive: route.isActive
        };
        this.showModal.set(true);
    }

    openDeleteModal(route: Route): void {
        this.selectedRoute = route;
        this.showDeleteModal.set(true);
    }

    closeModal(): void {
        this.showModal.set(false);
        this.showDeleteModal.set(false);
        this.selectedRoute = null;
    }

    resetForm(): void {
        this.formData = {
            routeName: '',
            originProvince: '',
            originDistrict: '',
            destinationProvince: '',
            destinationDistrict: '',
            distanceKm: null,
            estimatedDurationHours: null,
            basePrice: 0,
            isActive: true
        };
    }

    saveRoute(): void {
        const payload = {
            companyId: this.companyId,
            ...this.formData
        };

        if (this.isEditing() && this.selectedRoute) {
            this.api.put(`/routes/${this.selectedRoute.routeId}`, payload).subscribe({
                next: () => {
                    this.closeModal();
                    this.loadRoutes();
                },
                error: (err) => console.error('Update failed:', err)
            });
        } else {
            this.api.post('/routes', payload).subscribe({
                next: () => {
                    this.closeModal();
                    this.loadRoutes();
                },
                error: (err) => console.error('Create failed:', err)
            });
        }
    }

    deleteRoute(): void {
        if (!this.selectedRoute) return;

        this.api.delete(`/routes/${this.selectedRoute.routeId}`).subscribe({
            next: () => {
                this.closeModal();
                this.loadRoutes();
            },
            error: (err) => console.error('Delete failed:', err)
        });
    }

    toggleStatus(route: Route): void {
        this.api.patch(`/routes/${route.routeId}/status`, { isActive: !route.isActive }).subscribe({
            next: () => this.loadRoutes(),
            error: (err) => console.error('Toggle failed:', err)
        });
    }

    // Pagination
    goToPage(page: number): void {
        if (page >= 1 && page <= this.totalPages) {
            this.currentPage = page;
        }
    }

    clearFilters(): void {
        this.filterOrigin = '';
        this.filterDestination = '';
        this.filterActiveOnly = false;
        this.currentPage = 1;
    }

    // Formatting
    formatCurrency(amount: number): string {
        return new Intl.NumberFormat('vi-VN', {
            style: 'decimal',
            minimumFractionDigits: 0
        }).format(amount) + 'đ';
    }

    // Province coordinates for map visualization (approximate lat/lng)
    provinceCoords: { [key: string]: { lat: number; lng: number } } = {
        'An Giang': { lat: 10.52, lng: 105.13 },
        'Bà Rịa - Vũng Tàu': { lat: 10.54, lng: 107.24 },
        'Bạc Liêu': { lat: 9.29, lng: 105.72 },
        'Bắc Giang': { lat: 21.27, lng: 106.19 },
        'Bắc Kạn': { lat: 22.15, lng: 105.83 },
        'Bắc Ninh': { lat: 21.19, lng: 106.07 },
        'Bến Tre': { lat: 10.24, lng: 106.38 },
        'Bình Dương': { lat: 11.17, lng: 106.65 },
        'Bình Định': { lat: 13.77, lng: 109.22 },
        'Bình Phước': { lat: 11.75, lng: 106.72 },
        'Bình Thuận': { lat: 11.09, lng: 108.07 },
        'Cà Mau': { lat: 9.18, lng: 105.15 },
        'Cao Bằng': { lat: 22.67, lng: 106.26 },
        'Cần Thơ': { lat: 10.03, lng: 105.79 },
        'Đà Nẵng': { lat: 16.05, lng: 108.22 },
        'Đắk Lắk': { lat: 12.71, lng: 108.24 },
        'Đắk Nông': { lat: 12.00, lng: 107.69 },
        'Điện Biên': { lat: 21.39, lng: 103.02 },
        'Đồng Nai': { lat: 10.95, lng: 106.82 },
        'Đồng Tháp': { lat: 10.49, lng: 105.69 },
        'Gia Lai': { lat: 13.98, lng: 108.00 },
        'Hà Giang': { lat: 22.82, lng: 104.98 },
        'Hà Nam': { lat: 20.54, lng: 105.91 },
        'Hà Nội': { lat: 21.03, lng: 105.85 },
        'Hà Tĩnh': { lat: 18.34, lng: 105.91 },
        'Hải Dương': { lat: 20.94, lng: 106.33 },
        'Hải Phòng': { lat: 20.86, lng: 106.68 },
        'Hậu Giang': { lat: 9.78, lng: 105.47 },
        'Hòa Bình': { lat: 20.81, lng: 105.34 },
        'Hưng Yên': { lat: 20.65, lng: 106.05 },
        'Khánh Hòa': { lat: 12.24, lng: 109.20 },
        'Kiên Giang': { lat: 10.01, lng: 105.08 },
        'Kon Tum': { lat: 14.35, lng: 108.00 },
        'Lai Châu': { lat: 22.39, lng: 103.46 },
        'Lâm Đồng': { lat: 11.94, lng: 108.44 },
        'Lạng Sơn': { lat: 21.85, lng: 106.76 },
        'Lào Cai': { lat: 22.49, lng: 103.97 },
        'Long An': { lat: 10.54, lng: 106.41 },
        'Nam Định': { lat: 20.42, lng: 106.17 },
        'Nghệ An': { lat: 19.23, lng: 104.92 },
        'Ninh Bình': { lat: 20.25, lng: 105.97 },
        'Ninh Thuận': { lat: 11.57, lng: 108.99 },
        'Phú Thọ': { lat: 21.32, lng: 105.40 },
        'Phú Yên': { lat: 13.09, lng: 109.31 },
        'Quảng Bình': { lat: 17.47, lng: 106.62 },
        'Quảng Nam': { lat: 15.54, lng: 108.02 },
        'Quảng Ngãi': { lat: 15.12, lng: 108.80 },
        'Quảng Ninh': { lat: 21.01, lng: 107.29 },
        'Quảng Trị': { lat: 16.74, lng: 107.19 },
        'Sóc Trăng': { lat: 9.60, lng: 105.97 },
        'Sơn La': { lat: 21.33, lng: 103.91 },
        'Tây Ninh': { lat: 11.31, lng: 106.10 },
        'Thái Bình': { lat: 20.45, lng: 106.34 },
        'Thái Nguyên': { lat: 21.59, lng: 105.84 },
        'Thanh Hóa': { lat: 19.81, lng: 105.78 },
        'Thừa Thiên Huế': { lat: 16.46, lng: 107.59 },
        'Tiền Giang': { lat: 10.36, lng: 106.36 },
        'TP. Hồ Chí Minh': { lat: 10.82, lng: 106.63 },
        'Trà Vinh': { lat: 9.94, lng: 106.34 },
        'Tuyên Quang': { lat: 21.82, lng: 105.21 },
        'Vĩnh Long': { lat: 10.25, lng: 105.96 },
        'Vĩnh Phúc': { lat: 21.31, lng: 105.60 },
        'Yên Bái': { lat: 21.70, lng: 104.87 }
    };

    // Map bounds for the satellite view (adjusted to match actual embed view)
    // The satellite embed shows: roughly lat 5-25, lng 95-125
    private mapBounds = {
        minLat: 5.0,
        maxLat: 25.0,
        minLng: 95.0,
        maxLng: 125.0
    };

    // Convert lat/lng to SVG coordinates (percentage)
    latLngToXY(lat: number, lng: number): { x: number; y: number } {
        const x = ((lng - this.mapBounds.minLng) / (this.mapBounds.maxLng - this.mapBounds.minLng)) * 100;
        const y = ((this.mapBounds.maxLat - lat) / (this.mapBounds.maxLat - this.mapBounds.minLat)) * 100;
        return { x, y };
    }

    // Get unique provinces used in routes (for markers)
    get routeMarkers(): { name: string; x: number; y: number; isOrigin: boolean; isDestination: boolean }[] {
        const provinces = new Map<string, { isOrigin: boolean; isDestination: boolean }>();

        this.routes().forEach(route => {
            if (!provinces.has(route.originProvince)) {
                provinces.set(route.originProvince, { isOrigin: true, isDestination: false });
            } else {
                provinces.get(route.originProvince)!.isOrigin = true;
            }

            if (!provinces.has(route.destinationProvince)) {
                provinces.set(route.destinationProvince, { isOrigin: false, isDestination: true });
            } else {
                provinces.get(route.destinationProvince)!.isDestination = true;
            }
        });

        return Array.from(provinces.entries())
            .filter(([name]) => this.provinceCoords[name])
            .map(([name, flags]) => {
                const coords = this.provinceCoords[name];
                const xy = this.latLngToXY(coords.lat, coords.lng);
                return { name, x: xy.x, y: xy.y, ...flags };
            });
    }

    // Get route lines for SVG
    get routeLines(): { x1: number; y1: number; x2: number; y2: number; name: string; isActive: boolean }[] {
        return this.routes()
            .filter(route =>
                this.provinceCoords[route.originProvince] &&
                this.provinceCoords[route.destinationProvince]
            )
            .map(route => {
                const origin = this.provinceCoords[route.originProvince];
                const dest = this.provinceCoords[route.destinationProvince];
                const from = this.latLngToXY(origin.lat, origin.lng);
                const to = this.latLngToXY(dest.lat, dest.lng);
                return {
                    x1: from.x,
                    y1: from.y,
                    x2: to.x,
                    y2: to.y,
                    name: route.routeName,
                    isActive: route.isActive
                };
            });
    }
}
