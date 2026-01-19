import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../auth/services/auth.service';

interface Route {
  routeId: number;
  routeName: string;
  companyId: number;
  companyName: string;
  originProvince: string;
  originDistrict: string;
  destinationProvince: string;
  destinationDistrict: string;
  distanceKm: number;
  estimatedDurationHours: number;
  basePrice: number;
  isActive: boolean;
}

interface Province {
  provinceId: number;
  provinceName: string;
}

interface District {
  districtId: number;
  districtName: string;
}

interface Ward {
  wardId: number;
  wardName: string;
}

@Component({
  selector: 'app-customer-order-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule, RouterModule],
  templateUrl: './customer-order-create.component.html',
  styleUrl: './customer-order-create.component.scss'
})
export class CustomerOrderCreateComponent implements OnInit {
  currentStep = signal(1);
  totalSteps = 5;
  isSubmitting = signal(false);
  orderCreated = signal(false);
  createdOrderCode = '';
  customerId: number | null = null;

  // Forms for each step
  routeForm!: FormGroup;  // Step 1: Chọn tuyến đường
  senderForm!: FormGroup;  // Step 2: Người gửi
  receiverForm!: FormGroup;  // Step 3: Người nhận
  goodsForm!: FormGroup;  // Step 4: Hàng hóa
  paymentForm!: FormGroup;  // Step 5: Thanh toán

  // Routes data
  allRoutes: Route[] = [];
  filteredRoutes: Route[] = [];
  selectedRoute: Route | null = null;
  loadingRoutes = signal(false);
  searchTerm = '';

  // Location data
  provinces: Province[] = [];
  senderDistricts: District[] = [];
  senderWards: Ward[] = [];
  receiverDistricts: District[] = [];
  receiverWards: Ward[] = [];

  // Parcel types
  parcelTypes = [
    { value: 'document', label: 'Tài liệu', icon: '📄' },
    { value: 'fragile', label: 'Dễ vỡ', icon: '🔮' },
    { value: 'electronics', label: 'Điện tử', icon: '📱' },
    { value: 'food', label: 'Thực phẩm', icon: '🍜' },
    { value: 'cold', label: 'Hàng lạnh', icon: '❄️' },
    { value: 'other', label: 'Khác', icon: '📦' }
  ];

  // Payment methods
  paymentMethods = [
    { value: 'cash', label: 'Tiền mặt', icon: '💵' },
    { value: 'bank_transfer', label: 'Chuyển khoản', icon: '🏦' },
    { value: 'e_wallet', label: 'Ví điện tử', icon: '📲' }
  ];

  // Calculated shipping fee
  shippingFee = signal(0);

  steps = [
    { number: 1, label: 'Chọn tuyến' },
    { number: 2, label: 'Người gửi' },
    { number: 3, label: 'Người nhận' },
    { number: 4, label: 'Hàng hóa' },
    { number: 5, label: 'Thanh toán' }
  ];

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private authService: AuthService,
    private router: Router
  ) {
    this.initForms();
  }

  ngOnInit(): void {
    this.loadRoutes();
    this.loadProvinces();
    this.loadUserInfo();
  }

  private initForms(): void {
    // Step 1: Chọn tuyến đường
    this.routeForm = this.fb.group({
      routeId: ['', Validators.required]
    });

    // Step 2: Người gửi
    this.senderForm = this.fb.group({
      senderName: ['', [Validators.required, Validators.minLength(2)]],
      senderPhone: ['', [Validators.required, Validators.pattern(/^0[0-9]{9}$/)]],
      senderAddress: ['', Validators.required],
      saveToAddressBook: [false]
    });

    // Step 3: Người nhận
    this.receiverForm = this.fb.group({
      receiverName: ['', [Validators.required, Validators.minLength(2)]],
      receiverPhone: ['', [Validators.required, Validators.pattern(/^0[0-9]{9}$/)]],
      receiverAddress: ['', Validators.required],
      courierNote: [''],
      saveToAddressBook: [false]
    });

    // Step 4: Hàng hóa
    this.goodsForm = this.fb.group({
      parcelType: ['document', Validators.required],
      weightKg: [1, [Validators.required, Validators.min(0.1), Validators.max(100)]],
      declaredValue: [0],
      specialInstructions: ['']
    });

    // Step 5: Thanh toán
    this.paymentForm = this.fb.group({
      paymentMethod: ['cash', Validators.required],
      codAmount: [0, [Validators.min(0)]]
    });
  }

  private loadUserInfo(): void {
    const user = this.authService.getCurrentUser();
    if (user) {
      // Pre-fill sender info
      this.senderForm.patchValue({
        senderName: user.fullName,
        senderPhone: user.phone
      });

      // Get customerId
      this.api.get<any>(`/customer/user/${user.userId}`).subscribe({
        next: (res) => {
          this.customerId = res.customerId || res.data?.customerId;
        },
        error: () => {
          this.customerId = null;
        }
      });
    }
  }

  // Load all available routes
  private loadRoutes(): void {
    this.loadingRoutes.set(true);
    this.api.get<any>('/routes').subscribe({
      next: (response) => {
        const routes = response.data || response || [];
        this.allRoutes = routes.filter((r: Route) => r.isActive);
        this.filteredRoutes = [...this.allRoutes];
        this.loadingRoutes.set(false);
      },
      error: () => {
        this.allRoutes = [];
        this.filteredRoutes = [];
        this.loadingRoutes.set(false);
      }
    });
  }

  // Filter routes by search term
  filterRoutes(): void {
    if (!this.searchTerm.trim()) {
      this.filteredRoutes = [...this.allRoutes];
      return;
    }
    const term = this.searchTerm.toLowerCase();
    this.filteredRoutes = this.allRoutes.filter(r =>
      r.routeName.toLowerCase().includes(term) ||
      r.companyName?.toLowerCase().includes(term) ||
      r.originProvince.toLowerCase().includes(term) ||
      r.destinationProvince.toLowerCase().includes(term)
    );
  }

  // Select a route
  selectRoute(route: Route): void {
    this.selectedRoute = route;
    this.routeForm.patchValue({ routeId: route.routeId });
    this.shippingFee.set(route.basePrice);
  }

  private loadProvinces(): void {
    this.provinces = [
      { provinceId: 1, provinceName: 'TP. Hồ Chí Minh' },
      { provinceId: 2, provinceName: 'Hà Nội' },
      { provinceId: 3, provinceName: 'Đà Nẵng' },
      { provinceId: 4, provinceName: 'Cần Thơ' },
      { provinceId: 5, provinceName: 'Lâm Đồng' },
      { provinceId: 6, provinceName: 'Khánh Hòa' },
      { provinceId: 7, provinceName: 'Bà Rịa - Vũng Tàu' },
      { provinceId: 8, provinceName: 'Bình Dương' },
      { provinceId: 9, provinceName: 'Đồng Nai' },
      { provinceId: 10, provinceName: 'Long An' }
    ];
  }

  calculateShippingFee(): void {
    if (this.selectedRoute) {
      const weight = this.goodsForm.get('weightKg')?.value || 1;
      const baseFee = this.selectedRoute.basePrice;
      const perKgFee = 5000;
      const fee = baseFee + ((weight - 1) * perKgFee);
      this.shippingFee.set(Math.max(fee, baseFee));
    }
  }

  isStepValid(step: number): boolean {
    switch (step) {
      case 1: return this.routeForm.valid && this.selectedRoute !== null;
      case 2: return this.senderForm.valid;
      case 3: return this.receiverForm.valid;
      case 4: return this.goodsForm.valid;
      case 5: return this.paymentForm.valid;
      default: return true;
    }
  }

  nextStep(): void {
    if (this.isStepValid(this.currentStep())) {
      if (this.currentStep() < this.totalSteps) {
        this.currentStep.update(s => s + 1);
      }
    }
  }

  prevStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  goToStep(step: number): void {
    if (step < this.currentStep()) {
      this.currentStep.set(step);
    }
  }

  getParcelTypeLabel(value: string): string {
    return this.parcelTypes.find(t => t.value === value)?.label || value;
  }

  getPaymentMethodLabel(value: string): string {
    return this.paymentMethods.find(m => m.value === value)?.label || value;
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
  }

  submitOrder(): void {
    if (!this.selectedRoute) {
      return;
    }

    for (let i = 1; i <= 5; i++) {
      if (!this.isStepValid(i)) {
        return;
      }
    }

    this.isSubmitting.set(true);

    const orderData = {
      customerId: this.customerId || 0,
      // Route & Company
      routeId: this.selectedRoute.routeId,
      // Sender
      senderName: this.senderForm.value.senderName,
      senderPhone: this.senderForm.value.senderPhone,
      senderAddress: `${this.senderForm.value.senderAddress}, ${this.selectedRoute.originDistrict}, ${this.selectedRoute.originProvince}`,
      // Receiver
      receiverName: this.receiverForm.value.receiverName,
      receiverPhone: this.receiverForm.value.receiverPhone,
      receiverAddress: this.receiverForm.value.receiverAddress,
      receiverProvince: this.selectedRoute.destinationProvince,
      receiverDistrict: this.selectedRoute.destinationDistrict,
      receiverWard: '',
      // Parcel
      parcelType: this.goodsForm.value.parcelType,
      weightKg: this.goodsForm.value.weightKg,
      declaredValue: this.goodsForm.value.declaredValue || 0,
      specialInstructions: this.goodsForm.value.specialInstructions || this.receiverForm.value.courierNote || '',
      // Payment
      paymentMethod: this.paymentForm.value.paymentMethod,
      codAmount: this.paymentForm.value.codAmount || 0
    };

    this.api.post<any>('/orders', orderData).subscribe({
      next: (response) => {
        this.isSubmitting.set(false);
        this.orderCreated.set(true);
        this.createdOrderCode = response.data?.trackingCode || response.trackingCode || 'WDL-XXXXXX';
      },
      error: (err) => {
        this.isSubmitting.set(false);
        console.error('Error creating order:', err);
        alert('Có lỗi xảy ra khi tạo đơn hàng. Vui lòng thử lại.');
      }
    });
  }

  createNewOrder(): void {
    this.orderCreated.set(false);
    this.currentStep.set(1);
    this.selectedRoute = null;
    this.routeForm.reset();
    this.senderForm.reset();
    this.receiverForm.reset();
    this.goodsForm.reset({ parcelType: 'document', weightKg: 1 });
    this.paymentForm.reset({ paymentMethod: 'cash', codAmount: 0 });
    this.loadUserInfo();
  }

  trackOrder(): void {
    this.router.navigate(['/tracking'], { queryParams: { code: this.createdOrderCode } });
  }

  goHome(): void {
    this.router.navigate(['/customer']);
  }
}
