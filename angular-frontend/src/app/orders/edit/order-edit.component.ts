import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { OrderService } from '../services/order.service';
import { Order, OrderStatus, ParcelType, PaymentMethod } from '../../core/models/order.model';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../auth/services/auth.service';

interface DriverOption {
    driverId: number;
    fullName: string;
    phone: string;
}

interface VehicleOption {
    vehicleId: number;
    licensePlate: string;
    vehicleType: string;
}

interface RouteOption {
    routeId: number;
    routeName: string;
}

@Component({
    selector: 'app-order-edit',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule],
    templateUrl: './order-edit.component.html',
    styleUrl: './order-edit.component.scss'
})
export class OrderEditComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private orderService = inject(OrderService);
    private api = inject(ApiService);
    private authService = inject(AuthService);

    private get companyId(): number {
        return this.authService.getCurrentUser()?.companyId ?? 1;
    }

    order = signal<Order | null>(null);
    isLoading = signal(true);
    isSaving = signal(false);
    error = signal<string | null>(null);

    // Form data
    formData = signal({
        senderName: '',
        senderPhone: '',
        senderAddress: '',
        receiverName: '',
        receiverPhone: '',
        receiverAddress: '',
        receiverProvince: '',
        receiverDistrict: '',
        parcelType: 'other' as ParcelType,
        weightKg: 0,
        declaredValue: 0,
        specialInstructions: '',
        routeId: null as number | null,
        vehicleId: null as number | null,
        driverId: null as number | null,
        codAmount: 0,
        pickupScheduledAt: '',
        orderStatus: '' as OrderStatus,
        paymentStatus: ''
    });

    // Dropdown options
    drivers = signal<DriverOption[]>([]);
    vehicles = signal<VehicleOption[]>([]);
    routes = signal<RouteOption[]>([]);

    parcelTypes: { value: ParcelType; label: string }[] = [
        { value: 'document', label: 'Tài liệu' },
        { value: 'fragile', label: 'Dễ vỡ' },
        { value: 'electronics', label: 'Điện tử' },
        { value: 'food', label: 'Thực phẩm' },
        { value: 'cold', label: 'Hàng lạnh' },
        { value: 'other', label: 'Khác' }
    ];

    orderStatuses: { value: string; label: string }[] = [
        { value: 'pending_pickup', label: 'Chờ lấy hàng' },
        { value: 'picked_up', label: 'Đã lấy hàng' },
        { value: 'in_transit', label: 'Đang vận chuyển' },
        { value: 'out_for_delivery', label: 'Đang giao hàng' },
        { value: 'delivered', label: 'Đã giao hàng' },
        { value: 'cancelled', label: 'Đã hủy' },
        { value: 'returned', label: 'Hoàn trả' }
    ];

    paymentStatuses: { value: string; label: string }[] = [
        { value: 'unpaid', label: 'Chưa thanh toán' },
        { value: 'paid', label: 'Đã thanh toán' },
        { value: 'pending', label: 'Đang xử lý' }
    ];

    ngOnInit(): void {
        const orderId = Number(this.route.snapshot.params['orderId']);
        if (orderId) {
            this.loadOrder(orderId);
            this.loadDropdownData();
        }
    }

    loadOrder(orderId: number): void {
        this.isLoading.set(true);
        this.error.set(null);

        this.orderService.getOrderById(orderId).subscribe({
            next: (response: any) => {
                const data = response.data || response;
                this.order.set(data);
                this.populateForm(data);
                this.isLoading.set(false);
            },
            error: (err) => {
                this.error.set(err.message || 'Không thể tải thông tin đơn hàng');
                this.isLoading.set(false);
            }
        });
    }

    populateForm(order: Order): void {
        this.formData.set({
            senderName: order.senderName || '',
            senderPhone: order.senderPhone || '',
            senderAddress: order.senderAddress || '',
            receiverName: order.receiverName || '',
            receiverPhone: order.receiverPhone || '',
            receiverAddress: order.receiverAddress || '',
            receiverProvince: order.receiverProvince || '',
            receiverDistrict: order.receiverDistrict || '',
            parcelType: order.parcelType || 'other',
            weightKg: order.weightKg || 0,
            declaredValue: order.declaredValue || 0,
            specialInstructions: order.specialInstructions || '',
            routeId: order.routeId,
            vehicleId: order.vehicleId,
            driverId: order.driverId,
            codAmount: order.codAmount || 0,
            pickupScheduledAt: order.pickupScheduledAt ? this.formatDateTimeLocal(order.pickupScheduledAt) : '',
            orderStatus: order.orderStatus || 'pending',
            paymentStatus: order.paymentStatus || 'unpaid'
        });
    }

    loadDropdownData(): void {
        const companyId = this.companyId;

        // Load drivers by company
        this.api.get<any>(`/drivers/company/${companyId}`).subscribe({
            next: (res) => {
                const data = res.data || res || [];
                this.drivers.set(data);
            },
            error: () => this.drivers.set([])
        });

        // Load vehicles by company
        this.api.get<any>('/vehicles', { companyId }).subscribe({
            next: (res) => {
                const data = res.data || res || [];
                this.vehicles.set(data);
            },
            error: () => this.vehicles.set([])
        });

        // Load routes by company
        this.api.get<any>(`/routes/company/${companyId}`).subscribe({
            next: (res) => {
                const data = res.data || res || [];
                this.routes.set(data);
            },
            error: () => this.routes.set([])
        });
    }

    updateFormField(field: string, value: any): void {
        this.formData.update(form => ({
            ...form,
            [field]: value
        }));
    }

    goBack(): void {
        if (this.order()) {
            this.router.navigate(['/orders', this.order()!.orderId]);
        } else {
            this.router.navigate(['/orders']);
        }
    }

    saveOrder(): void {
        if (!this.order()) return;

        this.isSaving.set(true);
        const form = this.formData();

        const updateData = {
            senderName: form.senderName,
            senderPhone: form.senderPhone,
            senderAddress: form.senderAddress,
            receiverName: form.receiverName,
            receiverPhone: form.receiverPhone,
            receiverAddress: form.receiverAddress,
            receiverProvince: form.receiverProvince,
            receiverDistrict: form.receiverDistrict,
            parcelType: form.parcelType,
            weightKg: form.weightKg,
            declaredValue: form.declaredValue || null,
            specialInstructions: form.specialInstructions || null,
            routeId: form.routeId,
            vehicleId: form.vehicleId,
            driverId: form.driverId,
            codAmount: form.codAmount,
            pickupScheduledAt: form.pickupScheduledAt ? new Date(form.pickupScheduledAt).toISOString() : null,
            orderStatus: form.orderStatus,
            paymentStatus: form.paymentStatus
        };

        this.orderService.updateOrder(this.order()!.orderId, updateData).subscribe({
            next: () => {
                this.isSaving.set(false);
                this.router.navigate(['/orders', this.order()!.orderId]);
            },
            error: (err) => {
                this.isSaving.set(false);
                alert('Không thể lưu thay đổi: ' + (err.message || 'Đã xảy ra lỗi'));
            }
        });
    }

    // Helpers
    formatDateTimeLocal(dateStr: string): string {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        return date.toISOString().slice(0, 16);
    }
}
