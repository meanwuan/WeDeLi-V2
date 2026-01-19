import { Injectable, signal, computed, inject } from '@angular/core';
import { ApiService } from '../../core/services/api.service';

export interface SenderInfo {
    name: string;
    phone: string;
    address: string;
    province: string;
    district: string;
    ward: string;
}

export interface ReceiverInfo {
    name: string;
    phone: string;
    address: string;
    province: string;
    district: string;
    ward: string;
}

export interface PackageInfo {
    parcelType: 'document' | 'electronics' | 'fragile' | 'food' | 'cold' | 'other';
    weightKg: number;
    length: number;
    width: number;
    height: number;
    declaredValue: number;
    specialInstructions: string;
}

export interface PaymentInfo {
    paymentMethod: 'cash' | 'bank_transfer' | 'e_wallet';
    codAmount: number;
    promoCode: string;
    promoDiscount: number;
}

export interface OrderCost {
    baseFee: number;
    weightFee: number;
    parcelTypeFee: number;
    insurance: number;
    vat: number;
    discount: number;
    total: number;
}

export interface OrderWizardState {
    currentStep: number;
    sender: SenderInfo;
    receiver: ReceiverInfo;
    package: PackageInfo;
    payment: PaymentInfo;
    cost: OrderCost;
    routeId: number | null;
}

const initialState: OrderWizardState = {
    currentStep: 1,
    sender: {
        name: '',
        phone: '',
        address: '',
        province: '',
        district: '',
        ward: ''
    },
    receiver: {
        name: '',
        phone: '',
        address: '',
        province: '',
        district: '',
        ward: ''
    },
    package: {
        parcelType: 'other',
        weightKg: 1, // Default 1kg to pass backend validation (must be > 0)
        length: 0,
        width: 0,
        height: 0,
        declaredValue: 0,
        specialInstructions: ''
    },
    payment: {
        paymentMethod: 'cash',
        codAmount: 0,
        promoCode: '',
        promoDiscount: 0
    },
    cost: {
        baseFee: 0,
        weightFee: 0,
        parcelTypeFee: 0,
        insurance: 0,
        vat: 0,
        discount: 0,
        total: 0
    },
    routeId: null
};

@Injectable({
    providedIn: 'root'
})
export class OrderCreateService {
    private api = inject(ApiService);

    // State signals
    private _state = signal<OrderWizardState>({ ...initialState });
    private _isCalculatingFee = signal(false);

    // Computed values
    state = computed(() => this._state());
    currentStep = computed(() => this._state().currentStep);
    sender = computed(() => this._state().sender);
    receiver = computed(() => this._state().receiver);
    package = computed(() => this._state().package);
    payment = computed(() => this._state().payment);
    cost = computed(() => this._state().cost);
    routeId = computed(() => this._state().routeId);
    isCalculatingFee = computed(() => this._isCalculatingFee());

    // Step labels
    readonly stepLabels = [
        { step: 1, label: 'Người gửi', icon: 'sender' },
        { step: 2, label: 'Người nhận', icon: 'receiver' },
        { step: 3, label: 'Bưu phẩm', icon: 'package' },
        { step: 4, label: 'Thanh toán', icon: 'payment' },
        { step: 5, label: 'Xác nhận', icon: 'confirm' }
    ];

    // Parcel types - must match backend enum: fragile, electronics, food, cold, document, other
    readonly parcelTypes = [
        { value: 'document', label: 'Tài liệu', maxWeight: 0.5 },
        { value: 'electronics', label: 'Điện tử', maxWeight: 10 },
        { value: 'fragile', label: 'Hàng dễ vỡ', maxWeight: 20 },
        { value: 'food', label: 'Thực phẩm', maxWeight: 10 },
        { value: 'cold', label: 'Hàng lạnh', maxWeight: 15 },
        { value: 'other', label: 'Khác', maxWeight: 30 }
    ];

    // Navigation
    nextStep(): boolean {
        const current = this._state().currentStep;
        if (current < 5 && this.validateCurrentStep()) {
            this._state.update(s => ({ ...s, currentStep: current + 1 }));
            return true;
        }
        return false;
    }

    prevStep(): void {
        const current = this._state().currentStep;
        if (current > 1) {
            this._state.update(s => ({ ...s, currentStep: current - 1 }));
        }
    }

    goToStep(step: number): void {
        if (step >= 1 && step <= 5) {
            this._state.update(s => ({ ...s, currentStep: step }));
        }
    }

    // Update sender
    updateSender(sender: Partial<SenderInfo>): void {
        this._state.update(s => ({
            ...s,
            sender: { ...s.sender, ...sender }
        }));
    }

    // Update receiver
    updateReceiver(receiver: Partial<ReceiverInfo>): void {
        this._state.update(s => ({
            ...s,
            receiver: { ...s.receiver, ...receiver }
        }));
    }

    // Update package
    updatePackage(pkg: Partial<PackageInfo>): void {
        this._state.update(s => ({
            ...s,
            package: { ...s.package, ...pkg }
        }));
        this.calculateCost();
    }

    // Update payment
    updatePayment(payment: Partial<PaymentInfo>): void {
        this._state.update(s => ({
            ...s,
            payment: { ...s.payment, ...payment }
        }));
        this.calculateCost();
    }

    // Apply promo code
    applyPromoCode(code: string): boolean {
        // Simple promo code logic - in production this would call an API
        let discount = 0;
        if (code.toUpperCase() === 'WEDELI10') {
            discount = 10; // 10% off
        } else if (code.toUpperCase() === 'FREESHIP') {
            discount = 100; // Free shipping (100% off base fee)
        } else if (code.toUpperCase() === 'NEWUSER') {
            discount = 20; // 20% off
        }

        if (discount > 0) {
            this._state.update(s => ({
                ...s,
                payment: { ...s.payment, promoCode: code, promoDiscount: discount }
            }));
            this.calculateCost();
            return true;
        }
        return false;
    }

    // Update route
    updateRoute(routeId: number): void {
        this._state.update(s => ({
            ...s,
            routeId: routeId
        }));
        this.calculateCostFromApi();
    }

    // Calculate cost from API
    calculateCostFromApi(): void {
        const state = this._state();
        const payment = state.payment;

        // If no routeId, use local calculation
        if (!state.routeId || state.routeId <= 0) {
            this.calculateCostLocal();
            return;
        }

        // If no weight, skip
        if (state.package.weightKg <= 0) {
            return;
        }

        this._isCalculatingFee.set(true);

        const payload = {
            routeId: state.routeId,
            weightKg: state.package.weightKg,
            parcelType: state.package.parcelType,
            declaredValue: state.package.declaredValue || 0
        };

        this.api.post<any>('/routes/calculate-fee', payload).subscribe({
            next: (response) => {
                const data = response.data || response;

                // Calculate VAT and discount
                const subtotal = data.totalFee;
                const vat = subtotal * 0.1;
                const discount = payment.promoDiscount > 0 ? (data.basePrice * payment.promoDiscount / 100) : 0;
                const total = subtotal + vat - discount;

                this._state.update(s => ({
                    ...s,
                    cost: {
                        baseFee: Math.round(data.basePrice),
                        weightFee: Math.round(data.weightFee),
                        parcelTypeFee: Math.round(data.parcelTypeFee),
                        insurance: Math.round(data.insuranceFee),
                        vat: Math.round(vat),
                        discount: Math.round(discount),
                        total: Math.round(total)
                    }
                }));
                this._isCalculatingFee.set(false);
            },
            error: () => {
                // Fallback to local calculation
                this.calculateCostLocal();
                this._isCalculatingFee.set(false);
            }
        });
    }

    // Local cost calculation (fallback when no route selected)
    calculateCostLocal(): void {
        const pkg = this._state().package;
        const payment = this._state().payment;

        // Base fee calculation (simplified)
        let baseFee = 15000; // Base fee in VND
        const weightFee = pkg.weightKg > 1 ? (pkg.weightKg - 1) * 5000 : 0;
        const parcelTypeFee = 0; // No surcharge without route
        baseFee += weightFee;

        // Insurance (0.5% of declared value)
        const insurance = pkg.declaredValue > 0 ? pkg.declaredValue * 0.005 : 0;

        // VAT (10%)
        const subtotal = baseFee + insurance;
        const vat = subtotal * 0.1;

        // Discount
        const discount = payment.promoDiscount > 0 ? (baseFee * payment.promoDiscount / 100) : 0;

        // Total
        const total = subtotal + vat - discount;

        this._state.update(s => ({
            ...s,
            cost: {
                baseFee: Math.round(baseFee),
                weightFee: Math.round(weightFee),
                parcelTypeFee: Math.round(parcelTypeFee),
                insurance: Math.round(insurance),
                vat: Math.round(vat),
                discount: Math.round(discount),
                total: Math.round(total)
            }
        }));
    }

    // Calculate cost (calls API or local based on routeId)
    calculateCost(): void {
        this.calculateCostFromApi();
    }

    // Validation
    validateCurrentStep(): boolean {
        const state = this._state();
        switch (state.currentStep) {
            case 1:
                return this.validateSender();
            case 2:
                return this.validateReceiver();
            case 3:
                return this.validatePackage();
            case 4:
                return this.validatePayment();
            case 5:
                return true;
            default:
                return false;
        }
    }

    validateSender(): boolean {
        const s = this._state().sender;
        return !!(s.name && s.phone && s.address && s.province && s.district);
    }

    validateReceiver(): boolean {
        const r = this._state().receiver;
        return !!(r.name && r.phone && r.address && r.province && r.district);
    }

    validatePackage(): boolean {
        const p = this._state().package;
        return p.weightKg > 0;
    }

    validatePayment(): boolean {
        return true; // Payment method always has default
    }

    // Reset state
    reset(): void {
        this._state.set({ ...initialState });
    }

    // Format phone number to match Vietnamese format (starts with 0 or +84)
    formatPhone(phone: string): string {
        if (!phone) return '';
        // Remove all non-digit characters except +
        let cleaned = phone.replace(/[^\d+]/g, '');
        // If starts with +84, keep it
        if (cleaned.startsWith('+84')) {
            return cleaned;
        }
        // If doesn't start with 0, prepend 0
        if (!cleaned.startsWith('0') && !cleaned.startsWith('+')) {
            cleaned = '0' + cleaned;
        }
        return cleaned;
    }

    // Get order data for submission - matches backend CreateOrderDto
    // customerId: 0 = new/walk-in customer (backend will auto-create), >0 = existing customer
    getOrderPayload(customerId: number = 0): object {
        const state = this._state();
        return {
            // CustomerId: 0 = auto-create from sender info, >0 = existing customer
            customerId: customerId,

            // Route Assignment
            routeId: state.routeId || null,

            // Sender Information
            senderName: state.sender.name,
            senderPhone: state.sender.phone,
            senderAddress: `${state.sender.address}${state.sender.ward ? ', ' + state.sender.ward : ''}, ${state.sender.district}, ${state.sender.province}`,

            // Receiver Information
            receiverName: state.receiver.name,
            receiverPhone: state.receiver.phone,
            receiverAddress: state.receiver.address,
            receiverProvince: state.receiver.province,
            receiverDistrict: state.receiver.district,
            receiverWard: state.receiver.ward || '',

            // Parcel Information
            parcelType: state.package.parcelType,
            weightKg: state.package.weightKg,
            declaredValue: state.package.declaredValue || 0,
            specialInstructions: state.package.specialInstructions || '',

            // Payment
            paymentMethod: state.payment.paymentMethod,
            codAmount: state.payment.codAmount || 0
        };
    }
}
