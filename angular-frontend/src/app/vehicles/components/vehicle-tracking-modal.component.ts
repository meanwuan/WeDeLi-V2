import { Component, Input, Output, EventEmitter, signal, OnInit, AfterViewInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { Vehicle, VEHICLE_STATUS_CONFIG } from '../../core/models/vehicle.model';

declare var google: any;

@Component({
    selector: 'app-vehicle-tracking-modal',
    standalone: true,
    imports: [CommonModule, StatusBadgeComponent],
    templateUrl: './vehicle-tracking-modal.component.html',
    styleUrl: './vehicle-tracking-modal.component.scss'
})
export class VehicleTrackingModalComponent implements OnInit, AfterViewInit, OnDestroy {
    @Input() vehicle!: Vehicle;
    @Output() close = new EventEmitter<void>();
    @ViewChild('mapContainer') mapContainer!: ElementRef;

    map: any;
    marker: any;
    updateInterval: any;

    // Mock tracking data - simulating Ho Chi Minh City area
    trackingData = signal({
        speed: 45,
        fuelLevel: 65,
        totalDistance: 124,
        estimatedArrival: '15:30',
        driverName: 'Nguyễn Văn A',
        driverPhone: '0909 123 456',
        currentLocation: 'Đường Trường Chinh, Q. Tân Bình',
        pickupLocation: 'Kho A - KCN Tân Bình',
        deliveryLocation: 'Siêu thị Big C, Q. Gò Vấp',
        lat: 10.8231,
        lng: 106.6297
    });

    activeTab = signal<'info' | 'orders' | 'history'>('info');

    currentOrders = signal([
        { id: 'WDL-2024-001', destination: 'Q. Tân Bình', status: 'in_transit' },
        { id: 'WDL-2024-002', destination: 'Q. Gò Vấp', status: 'pending' }
    ]);

    ngOnInit(): void {
        // Simulate real-time position updates
        this.updateInterval = setInterval(() => {
            const current = this.trackingData();
            const newLat = current.lat + (Math.random() - 0.5) * 0.002;
            const newLng = current.lng + (Math.random() - 0.5) * 0.002;

            this.trackingData.set({
                ...current,
                speed: 35 + Math.floor(Math.random() * 25),
                lat: newLat,
                lng: newLng
            });

            // Update marker position
            if (this.marker) {
                this.marker.setPosition({ lat: newLat, lng: newLng });
            }
        }, 3000);
    }

    ngAfterViewInit(): void {
        this.initMap();
    }

    ngOnDestroy(): void {
        if (this.updateInterval) {
            clearInterval(this.updateInterval);
        }
    }

    initMap(): void {
        // Retry if Google Maps not loaded yet
        if (typeof google === 'undefined' || !google.maps) {
            console.warn('Google Maps not loaded yet, retrying in 500ms...');
            setTimeout(() => this.initMap(), 500);
            return;
        }

        const initialPosition = {
            lat: this.trackingData().lat,
            lng: this.trackingData().lng
        };

        // Use satellite map type (road map violates Vietnamese law)
        this.map = new google.maps.Map(this.mapContainer.nativeElement, {
            center: initialPosition,
            zoom: 15,
            mapTypeId: 'satellite', // Use satellite view instead of roadmap
            mapTypeControl: true,
            streetViewControl: false,
            fullscreenControl: true,
            zoomControl: true
        });

        this.marker = new google.maps.Marker({
            position: initialPosition,
            map: this.map,
            title: this.vehicle.licensePlate,
            icon: {
                url: 'data:image/svg+xml,' + encodeURIComponent(`
                    <svg xmlns="http://www.w3.org/2000/svg" width="50" height="50" viewBox="0 0 50 50">
                        <circle cx="25" cy="25" r="20" fill="#FF6B35" stroke="white" stroke-width="3"/>
                        <text x="25" y="30" text-anchor="middle" fill="white" font-size="16">🚛</text>
                    </svg>
                `),
                scaledSize: new google.maps.Size(50, 50),
                anchor: new google.maps.Point(25, 25)
            }
        });

        // Add info window
        const infoWindow = new google.maps.InfoWindow({
            content: `<div style="padding:8px;font-weight:600;color:#111">${this.vehicle.licensePlate} • ${this.trackingData().speed}km/h</div>`
        });

        this.marker.addListener('click', () => {
            infoWindow.open(this.map, this.marker);
        });
    }

    onClose(): void {
        this.close.emit();
    }

    getStatusLabel(status: string): string {
        return VEHICLE_STATUS_CONFIG[status as keyof typeof VEHICLE_STATUS_CONFIG]?.label || status;
    }

    getStatusVariant(status: string): 'success' | 'warning' | 'danger' | 'info' | 'default' {
        switch (status) {
            case 'available': return 'success';
            case 'in_use': return 'info';
            case 'maintenance': return 'warning';
            case 'inactive': return 'danger';
            default: return 'default';
        }
    }
}
