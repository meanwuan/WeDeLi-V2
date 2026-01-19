import { Component, Input, AfterViewInit, ViewChild, ElementRef, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Vehicle } from '../../core/models/vehicle.model';

declare var google: any;

@Component({
    selector: 'app-vehicles-map',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="vehicles-map-container">
            <div #mapContainer class="map-canvas"></div>
            @if (!mapLoaded) {
                <div class="map-loading">
                    <div class="spinner"></div>
                    <p>Đang tải bản đồ vệ tinh...</p>
                </div>
            }
            <div class="map-legend">
                <div class="legend-item">
                    <span class="legend-dot available"></span>
                    <span>Sẵn sàng</span>
                </div>
                <div class="legend-item">
                    <span class="legend-dot in-use"></span>
                    <span>Đang sử dụng</span>
                </div>
                <div class="legend-item">
                    <span class="legend-dot maintenance"></span>
                    <span>Bảo trì</span>
                </div>
            </div>
            <div class="map-stats">
                <span>{{ vehicles.length }} xe trên bản đồ</span>
            </div>
        </div>
    `,
    styles: [`
        .vehicles-map-container {
            position: relative;
            width: 100%;
            height: 600px;
            border-radius: 12px;
            overflow: hidden;
            background: #1a1a2e;
        }

        .map-canvas {
            width: 100%;
            height: 100%;
        }

        .map-loading {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            background: rgba(26, 26, 46, 0.9);
            color: white;
            gap: 16px;
        }

        .spinner {
            width: 40px;
            height: 40px;
            border: 3px solid rgba(255, 255, 255, 0.3);
            border-top-color: #f97316;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            to { transform: rotate(360deg); }
        }

        .map-legend {
            position: absolute;
            bottom: 20px;
            left: 20px;
            background: rgba(0, 0, 0, 0.75);
            padding: 12px 16px;
            border-radius: 8px;
            display: flex;
            gap: 16px;
        }

        .legend-item {
            display: flex;
            align-items: center;
            gap: 8px;
            color: white;
            font-size: 12px;
        }

        .legend-dot {
            width: 10px;
            height: 10px;
            border-radius: 50%;
        }

        .legend-dot.available { background: #22c55e; }
        .legend-dot.in-use { background: #3b82f6; }
        .legend-dot.maintenance { background: #f59e0b; }

        .map-stats {
            position: absolute;
            top: 20px;
            right: 20px;
            background: rgba(0, 0, 0, 0.75);
            padding: 8px 16px;
            border-radius: 6px;
            color: white;
            font-size: 13px;
            font-weight: 500;
        }
    `]
})
export class VehiclesMapComponent implements AfterViewInit, OnDestroy, OnChanges {
    @Input() vehicles: Vehicle[] = [];
    @ViewChild('mapContainer') mapContainer!: ElementRef;

    map: any;
    markers: any[] = [];
    mapLoaded = false;

    // Default center: Ho Chi Minh City
    defaultCenter = { lat: 10.8231, lng: 106.6297 };

    ngAfterViewInit(): void {
        this.initMap();
    }

    ngOnChanges(changes: SimpleChanges): void {
        if (changes['vehicles'] && this.map) {
            this.updateMarkers();
        }
    }

    ngOnDestroy(): void {
        this.clearMarkers();
    }

    initMap(): void {
        if (typeof google === 'undefined' || !google.maps) {
            console.warn('Google Maps not loaded yet, retrying in 500ms...');
            setTimeout(() => this.initMap(), 500);
            return;
        }

        // Use satellite map type (road map violates Vietnamese law)
        this.map = new google.maps.Map(this.mapContainer.nativeElement, {
            center: this.defaultCenter,
            zoom: 12,
            mapTypeId: 'satellite', // Satellite view only
            mapTypeControl: false, // Disable map type switching
            streetViewControl: false,
            fullscreenControl: true,
            zoomControl: true,
            styles: [] // No custom styles for satellite
        });

        this.mapLoaded = true;
        this.updateMarkers();
    }

    updateMarkers(): void {
        this.clearMarkers();

        if (!this.map || !this.vehicles.length) return;

        const bounds = new google.maps.LatLngBounds();

        this.vehicles.forEach((vehicle, index) => {
            // Generate mock positions spread around Ho Chi Minh City
            // In production, use vehicle.latitude and vehicle.longitude
            const lat = this.defaultCenter.lat + (Math.random() - 0.5) * 0.1;
            const lng = this.defaultCenter.lng + (Math.random() - 0.5) * 0.1;

            const position = { lat, lng };
            bounds.extend(position);

            const markerColor = this.getMarkerColor(vehicle.currentStatus);

            const marker = new google.maps.Marker({
                position,
                map: this.map,
                title: vehicle.licensePlate,
                icon: {
                    url: this.createMarkerSvg(markerColor, vehicle.vehicleType),
                    scaledSize: new google.maps.Size(40, 40),
                    anchor: new google.maps.Point(20, 20)
                }
            });

            // Info window with vehicle details
            const infoWindow = new google.maps.InfoWindow({
                content: `
                    <div style="padding:8px;min-width:150px">
                        <div style="font-weight:600;font-size:14px;color:#111;margin-bottom:4px">
                            ${vehicle.licensePlate}
                        </div>
                        <div style="font-size:12px;color:#666;margin-bottom:4px">
                            ${this.getTypeLabel(vehicle.vehicleType)}
                        </div>
                        <div style="font-size:12px;color:#888">
                            Trạng thái: ${this.getStatusLabel(vehicle.currentStatus)}
                        </div>
                        ${vehicle.driverName ? `
                        <div style="font-size:12px;color:#888;margin-top:4px">
                            Tài xế: ${vehicle.driverName}
                        </div>
                        ` : ''}
                    </div>
                `
            });

            marker.addListener('click', () => {
                infoWindow.open(this.map, marker);
            });

            this.markers.push(marker);
        });

        // Fit map to show all markers
        if (this.markers.length > 1) {
            this.map.fitBounds(bounds);
        } else if (this.markers.length === 1) {
            this.map.setCenter(this.markers[0].getPosition());
            this.map.setZoom(15);
        }
    }

    clearMarkers(): void {
        this.markers.forEach(marker => marker.setMap(null));
        this.markers = [];
    }

    getMarkerColor(status: string): string {
        switch (status) {
            case 'available': return '#22c55e';
            case 'in_use': return '#3b82f6';
            case 'maintenance': return '#f59e0b';
            case 'inactive': return '#6b7280';
            default: return '#f97316';
        }
    }

    createMarkerSvg(color: string, vehicleType: string): string {
        const icon = this.getTypeIcon(vehicleType);
        return 'data:image/svg+xml,' + encodeURIComponent(`
            <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 40 40">
                <circle cx="20" cy="20" r="18" fill="${color}" stroke="white" stroke-width="3"/>
                <text x="20" y="26" text-anchor="middle" fill="white" font-size="14">${icon}</text>
            </svg>
        `);
    }

    getTypeIcon(type: string): string {
        switch (type) {
            case 'truck_small': return '🚚';
            case 'truck_medium': return '🚛';
            case 'truck_large': return '🚛';
            case 'van': return '🚐';
            case 'motorcycle': return '🏍️';
            default: return '🚗';
        }
    }

    getTypeLabel(type: string): string {
        const labels: Record<string, string> = {
            'truck_small': 'Xe tải nhỏ',
            'truck_medium': 'Xe tải trung',
            'truck_large': 'Xe tải lớn',
            'van': 'Xe van',
            'motorcycle': 'Xe máy'
        };
        return labels[type] || type;
    }

    getStatusLabel(status: string): string {
        const labels: Record<string, string> = {
            'available': 'Sẵn sàng',
            'in_use': 'Đang sử dụng',
            'maintenance': 'Bảo trì',
            'inactive': 'Ngừng hoạt động'
        };
        return labels[status] || status;
    }
}
