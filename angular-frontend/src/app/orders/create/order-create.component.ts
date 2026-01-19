import { Component, inject, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { OrderCreateService } from './order-create.service';
import { ApiService } from '../../core/services/api.service';

interface Customer {
    customerId?: number;
    companyCustomerId: number;
    fullName: string;
    phone: string;
    email?: string;
    totalOrders?: number;
    isVip?: boolean;
    customPrice?: number;
    discountPercent?: number;
}

interface Route {
    routeId: number;
    routeName: string;
    originProvince: string;
    destinationProvince: string;
    basePrice: number;
    estimatedDurationHours?: number;
}

@Component({
    selector: 'app-order-create',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule],
    templateUrl: './order-create.component.html',
    styleUrl: './order-create.component.scss'
})
export class OrderCreateComponent implements OnInit, OnDestroy {
    orderService = inject(OrderCreateService);
    private api = inject(ApiService);
    private router = inject(Router);
    private cdr = inject(ChangeDetectorRef);

    submitting = false;
    error: string | null = null;
    submitted = false;
    createdOrderId: number | null = null;
    confirmChecked = false;
    successMessage = '';

    // Customer selection
    isNewCustomer = true;  // true = khách vãng lai, false = chọn khách quen
    customers: Customer[] = [];
    selectedCustomerId: number = 0;
    customerSearchTerm = '';
    loadingCustomers = false;

    // Route selection
    routes: Route[] = [];
    selectedRouteId: number = 0;
    loadingRoutes = false;

    // Location data (simplified - in production would be from API)
    provinces = [
        'Hà Nội', 'Hồ Chí Minh', 'Đà Nẵng', 'Hải Phòng', 'Cần Thơ',
        'Bình Dương', 'Đồng Nai', 'Nghệ An', 'Thái Nguyên', 'Khánh Hòa'
    ];
    districts: string[] = [];
    wards: string[] = [];

    ngOnInit(): void {
        this.orderService.reset();
        this.loadDistricts();
        this.loadCustomers();
        this.loadRoutes();
    }

    ngOnDestroy(): void {
        // Don't reset on destroy to preserve state if user navigates back
    }

    loadDistricts(): void {
        // Simplified - in production would fetch from API based on province
        this.districts = ['Quận 1', 'Quận 2', 'Quận 3', 'Quận 7', 'Quận Bình Thạnh', 'Quận Gò Vấp', 'Quận Tân Bình', 'Quận Thủ Đức'];
        this.wards = ['Phường 1', 'Phường 2', 'Phường 3', 'Phường 4', 'Phường 5', 'Phường 6'];
    }

    // Load routes for dropdown
    loadRoutes(): void {
        this.loadingRoutes = true;
        // Get all routes from company 1 (or use /routes/search for specific routes)
        this.api.get<any>('/routes/company/1').subscribe({
            next: (response) => {
                this.routes = response.data || response || [];
                this.loadingRoutes = false;
            },
            error: () => {
                this.routes = [];
                this.loadingRoutes = false;
            }
        });
    }

    // Select route and trigger fee calculation
    selectRoute(routeId: number): void {
        this.selectedRouteId = routeId;
        if (routeId > 0) {
            this.orderService.updateRoute(routeId);
        }
    }

    // Load company customers for dropdown
    loadCustomers(): void {
        this.loadingCustomers = true;
        // Load company customers (khách quen của nhà xe)
        const companyId = 1; // TODO: Get from auth service
        this.api.get<any>('/company-customers', { companyId }).subscribe({
            next: (response) => {
                this.customers = response.data || response || [];
                this.loadingCustomers = false;
            },
            error: () => {
                this.customers = [];
                this.loadingCustomers = false;
            }
        });
    }

    // Toggle between new customer and existing customer
    toggleCustomerType(isNew: boolean): void {
        this.isNewCustomer = isNew;
        if (isNew) {
            this.selectedCustomerId = 0;
        }
    }

    // Select existing customer and fill sender info
    selectCustomer(customerId: number): void {
        this.selectedCustomerId = customerId;
        const customer = this.customers.find(c => c.companyCustomerId === customerId);
        if (customer) {
            this.orderService.updateSender({
                name: customer.fullName,
                phone: customer.phone
            });
            // Apply customer-specific pricing if available
            if (customer.customPrice || customer.discountPercent) {
                // Store customer pricing info for later use
                console.log('Customer pricing:', customer.customPrice, customer.discountPercent);
            }
        }
    }

    // Filter customers by search term
    get filteredCustomers(): Customer[] {
        if (!this.customerSearchTerm) return this.customers;
        const term = this.customerSearchTerm.toLowerCase();
        return this.customers.filter(c =>
            c.fullName.toLowerCase().includes(term) ||
            c.phone.includes(term)
        );
    }

    // Check if route is selected (required for step 3)
    get isRouteRequired(): boolean {
        return this.orderService.currentStep() >= 3 && this.selectedRouteId === 0;
    }

    nextStep(): void {
        // Validate route selection before step 3
        if (this.orderService.currentStep() === 2 && this.selectedRouteId === 0) {
            this.error = 'Vui lòng chọn tuyến vận chuyển trước khi tiếp tục';
            return;
        }

        if (this.orderService.nextStep()) {
            this.error = null;
        } else {
            this.error = 'Vui lòng điền đầy đủ thông tin bắt buộc';
        }
    }

    prevStep(): void {
        this.orderService.prevStep();
        this.error = null;
    }

    goToStep(step: number): void {
        // Only allow going back, or to completed steps
        if (step < this.orderService.currentStep()) {
            this.orderService.goToStep(step);
        }
    }

    // Sender form handlers
    onSenderChange(field: string, value: string): void {
        // Format phone number to match Vietnamese format
        if (field === 'phone') {
            value = this.orderService.formatPhone(value);
        }
        this.orderService.updateSender({ [field]: value });
    }

    // Receiver form handlers
    onReceiverChange(field: string, value: string): void {
        // Format phone number to match Vietnamese format
        if (field === 'phone') {
            value = this.orderService.formatPhone(value);
        }
        this.orderService.updateReceiver({ [field]: value });
    }

    // Package form handlers
    onPackageChange(field: string, value: any): void {
        this.orderService.updatePackage({ [field]: value });
    }

    // Payment form handlers
    onPaymentChange(field: string, value: any): void {
        this.orderService.updatePayment({ [field]: value });
    }

    applyPromoCode(): void {
        const code = this.orderService.payment().promoCode;
        if (code) {
            const success = this.orderService.applyPromoCode(code);
            if (!success) {
                this.error = 'Mã khuyến mãi không hợp lệ';
            } else {
                this.error = null;
            }
        }
    }

    submitOrder(): void {
        if (!this.orderService.validateCurrentStep()) {
            this.error = 'Vui lòng xác nhận thông tin trước';
            return;
        }

        this.submitting = true;
        this.error = null;

        // Always send customerId: 0 to let backend auto-create/lookup customer by sender phone
        // CompanyCustomer IDs are from a different table than Customer table
        // Backend will find existing customer by phone or create a new one
        const payload = this.orderService.getOrderPayload(0);

        this.api.post<any>('/orders', payload).subscribe({
            next: (response) => {
                console.log('Order created successfully:', response);
                this.submitting = false;
                this.createdOrderId = response.data?.orderId || response.orderId || response.data?.OrderId;
                console.log('Created Order ID:', this.createdOrderId);
                // Show success state instead of redirecting
                this.submitted = true;
                console.log('Submitted flag set to:', this.submitted);
                // Force change detection to update the view
                this.cdr.detectChanges();
                // Scroll to top to show success card
                window.scrollTo({ top: 0, behavior: 'smooth' });
            },
            error: (err) => {
                console.error('Order creation error:', err);
                this.submitting = false;
                this.error = err.error?.message || err.message || 'Đã xảy ra lỗi khi tạo đơn hàng';
            }
        });
    }

    viewOrder(): void {
        if (this.createdOrderId) {
            this.router.navigate(['/orders', this.createdOrderId]);
        } else {
            this.router.navigate(['/orders']);
        }
    }

    createAnotherOrder(): void {
        this.orderService.reset();
        this.submitted = false;
        this.createdOrderId = null;
        this.error = null;
    }

    // Format currency
    formatCurrency(amount: number): string {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }
}
