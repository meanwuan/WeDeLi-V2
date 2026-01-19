import { Component, signal, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../core/services/api.service';

declare var google: any;

interface OrderTimeline {
    status: string;
    title: string;
    date: string;
    description: string;
    isActive: boolean;
    isCompleted: boolean;
}

interface DriverInfo {
    name: string;
    avatar: string;
    rating: number;
    reviewCount: number;
}

interface TrackingOrder {
    orderId: number;
    trackingCode: string;
    createdAt: string;
    expectedDelivery: string;
    orderStatus: string;
    serviceType: string;

    // Sender info
    senderName: string;
    senderPhone: string;
    senderAddress: string;

    // Receiver info  
    receiverName: string;
    receiverPhone: string;
    receiverAddress: string;

    // Package info
    parcelType: string;
    weightKg: number;
    specialInstructions: string;

    // Cost info
    shippingFee: number;
    codAmount: number;
    totalAmount: number;
    paymentStatus: string;

    // Driver info
    driver?: DriverInfo;

    // Timeline
    timeline: OrderTimeline[];
}

@Component({
    selector: 'app-tracking',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './tracking.component.html',
    styleUrl: './tracking.component.scss'
})
export class TrackingComponent implements AfterViewInit {
    @ViewChild('mapContainer') mapContainer!: ElementRef;

    searchCode = '';
    isSearching = signal(false);
    order = signal<TrackingOrder | null>(null);
    error = signal<string | null>(null);

    private map: any = null;
    private marker: any = null;
    private mapInitialized = false;

    // Mock vehicle location (Ho Chi Minh City center)
    private vehicleLocation = { lat: 10.7769, lng: 106.7009 };

    // Mock timeline statuses
    private statusSteps = [
        { key: 'pending_pickup', title: 'Đã lấy hàng' },
        { key: 'in_transit', title: 'Nhập kho trung chuyển' },
        { key: 'out_for_delivery', title: 'Đang vận chuyển' },
        { key: 'delivering', title: 'Đang giao hàng' },
        { key: 'delivered', title: 'Giao hàng thành công' }
    ];

    constructor(private api: ApiService) { }

    ngAfterViewInit(): void {
        // Map will be initialized when order is loaded
    }

    initMap(): void {
        if (this.mapInitialized || !this.mapContainer?.nativeElement) return;

        // Check if Google Maps is loaded
        if (typeof google === 'undefined' || !google.maps) {
            console.warn('Google Maps not loaded yet');
            setTimeout(() => this.initMap(), 500);
            return;
        }

        this.map = new google.maps.Map(this.mapContainer.nativeElement, {
            center: this.vehicleLocation,
            zoom: 13,
            mapTypeId: 'hybrid', // Satellite with labels
            mapTypeControl: false,
            streetViewControl: false,
            fullscreenControl: true,
            zoomControl: true,
            styles: [
                { featureType: 'poi', stylers: [{ visibility: 'off' }] }
            ]
        });

        // Add vehicle marker
        this.marker = new google.maps.Marker({
            position: this.vehicleLocation,
            map: this.map,
            title: 'Xe đang vận chuyển',
            icon: {
                url: 'data:image/svg+xml;charset=UTF-8,' + encodeURIComponent(`
                    <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24" fill="#f97316">
                        <path d="M18.92 6.01C18.72 5.42 18.16 5 17.5 5h-11c-.66 0-1.21.42-1.42 1.01L3 12v8c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-1h12v1c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-8l-2.08-5.99zM6.5 16c-.83 0-1.5-.67-1.5-1.5S5.67 13 6.5 13s1.5.67 1.5 1.5S7.33 16 6.5 16zm11 0c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zM5 11l1.5-4.5h11L19 11H5z"/>
                    </svg>
                `),
                scaledSize: new google.maps.Size(40, 40),
                anchor: new google.maps.Point(20, 20)
            }
        });

        // Add info window
        const infoWindow = new google.maps.InfoWindow({
            content: '<div style="padding: 8px; font-weight: 600; color: #f97316;">🚚 Xe đang di chuyển</div>'
        });

        this.marker.addListener('click', () => {
            infoWindow.open(this.map, this.marker);
        });

        this.mapInitialized = true;
    }

    searchOrder(): void {
        if (!this.searchCode.trim()) {
            this.error.set('Vui lòng nhập mã vận đơn');
            return;
        }

        this.isSearching.set(true);
        this.error.set(null);

        this.api.get<any>(`/orders/tracking/${this.searchCode.trim()}`).subscribe({
            next: (response) => {
                this.isSearching.set(false);
                if (response.data) {
                    this.order.set(this.mapOrderData(response.data));
                    // Initialize map after order is loaded
                    setTimeout(() => this.initMap(), 100);
                } else {
                    this.error.set('Không tìm thấy đơn hàng');
                }
            },
            error: (err) => {
                this.isSearching.set(false);
                this.error.set('Không tìm thấy đơn hàng với mã này');
                this.order.set(null);
            }
        });
    }

    private mapOrderData(data: any): TrackingOrder {
        const currentStatusIndex = this.statusSteps.findIndex(
            s => s.key === data.orderStatus
        );

        const timeline: OrderTimeline[] = this.statusSteps.map((step, index) => ({
            status: step.key,
            title: step.title,
            date: index <= currentStatusIndex ? this.formatDate(data.createdAt, index) : '',
            description: this.getStatusDescription(step.key, data),
            isActive: index === currentStatusIndex,
            isCompleted: index < currentStatusIndex
        }));

        return {
            orderId: data.orderId,
            trackingCode: data.trackingCode,
            createdAt: data.createdAt,
            expectedDelivery: data.expectedDelivery || this.addDays(data.createdAt, 3),
            orderStatus: data.orderStatus,
            serviceType: data.serviceType || 'Chuyển phát nhanh',

            senderName: data.senderName,
            senderPhone: this.maskPhone(data.senderPhone),
            senderAddress: data.senderAddress,

            receiverName: data.receiverName,
            receiverPhone: this.maskPhone(data.receiverPhone),
            receiverAddress: `${data.receiverAddress}, ${data.receiverDistrict}, ${data.receiverProvince}`,

            parcelType: this.getParcelTypeName(data.parcelType),
            weightKg: data.weightKg,
            specialInstructions: data.specialInstructions || 'Không có',

            shippingFee: data.shippingFee,
            codAmount: data.codAmount || 0,
            totalAmount: data.shippingFee + (data.codAmount || 0),
            paymentStatus: data.paymentStatus,

            driver: data.driver ? {
                name: data.driver.fullName,
                avatar: data.driver.avatar || '/assets/default-avatar.png',
                rating: data.driver.rating || 4.9,
                reviewCount: data.driver.reviewCount || 100
            } : undefined,

            timeline
        };
    }

    private maskPhone(phone: string): string {
        if (!phone || phone.length < 7) return phone;
        return phone.substring(0, 3) + '***' + phone.substring(phone.length - 3);
    }

    formatDate(dateStr: string, offsetDays: number = 0): string {
        const date = new Date(dateStr);
        date.setDate(date.getDate() + offsetDays);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    }

    private addDays(dateStr: string, days: number): string {
        const date = new Date(dateStr);
        date.setDate(date.getDate() + days);
        return date.toLocaleDateString('vi-VN', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });
    }

    private getStatusDescription(status: string, data: any): string {
        const descriptions: Record<string, string> = {
            'pending_pickup': `Đã nhận từ người gửi tại ${data.senderAddress}`,
            'in_transit': 'Kiện hàng đã đến kho Tân Bình',
            'out_for_delivery': 'Đang di chuyển đến kho Đà Nẵng',
            'delivering': 'Tài xế đang giao hàng',
            'delivered': 'Đã giao thành công'
        };
        return descriptions[status] || '';
    }

    private getParcelTypeName(type: string): string {
        const types: Record<string, string> = {
            'fragile': 'Dễ vỡ',
            'electronics': 'Điện tử',
            'food': 'Thực phẩm',
            'cold': 'Hàng lạnh',
            'document': 'Tài liệu',
            'other': 'Khác'
        };
        return types[type] || type;
    }

    getStatusLabel(status: string): string {
        const labels: Record<string, string> = {
            'pending_pickup': 'Chờ lấy hàng',
            'in_transit': 'Đang vận chuyển',
            'out_for_delivery': 'Đang giao',
            'delivering': 'Đang giao',
            'delivered': 'Đã giao',
            'cancelled': 'Đã hủy'
        };
        return labels[status] || status;
    }

    formatCurrency(amount: number): string {
        return new Intl.NumberFormat('vi-VN').format(amount) + 'đ';
    }
}
