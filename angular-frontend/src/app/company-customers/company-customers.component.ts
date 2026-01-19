import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CompanyCustomerService, CompanyCustomer, CompanyCustomerRequest, PricingUpdate } from './services/company-customer.service';
import { AuthService } from '../auth/services/auth.service';

@Component({
    selector: 'app-company-customers',
    standalone: true,
    imports: [CommonModule, FormsModule, ReactiveFormsModule],
    templateUrl: './company-customers.component.html',
    styleUrl: './company-customers.component.scss'
})
export class CompanyCustomersComponent implements OnInit {
    private service = inject(CompanyCustomerService);
    private authService = inject(AuthService);
    private fb = inject(FormBuilder);

    // State
    customers = signal<CompanyCustomer[]>([]);
    filteredCustomers = signal<CompanyCustomer[]>([]);
    isLoading = signal(true);
    searchTerm = signal('');
    filterVipOnly = signal(false);

    // Modal state
    showAddModal = signal(false);
    showPricingModal = signal(false);
    selectedCustomer = signal<CompanyCustomer | null>(null);
    isSaving = signal(false);

    // Forms
    customerForm!: FormGroup;
    pricingForm!: FormGroup;

    // Company ID from auth
    get companyId(): number {
        return this.authService.getCurrentUser()?.companyId || 0;
    }

    ngOnInit(): void {
        this.initForms();
        this.loadCustomers();
    }

    initForms(): void {
        this.customerForm = this.fb.group({
            fullName: ['', [Validators.required, Validators.maxLength(200)]],
            phone: ['', [Validators.required, Validators.pattern(/^[0-9]{10,11}$/)]],
            email: ['', [Validators.email]],
            customPrice: [null],
            discountPercent: [null, [Validators.min(0), Validators.max(100)]],
            isVip: [false],
            notes: ['']
        });

        this.pricingForm = this.fb.group({
            customPrice: [null],
            discountPercent: [null, [Validators.min(0), Validators.max(100)]]
        });
    }

    loadCustomers(): void {
        this.isLoading.set(true);
        this.service.getCompanyCustomers(this.companyId).subscribe({
            next: (data) => {
                this.customers.set(data);
                this.applyFilters();
                this.isLoading.set(false);
            },
            error: (err) => {
                console.error('Failed to load customers:', err);
                this.isLoading.set(false);
            }
        });
    }

    applyFilters(): void {
        let result = [...this.customers()];

        // Search filter
        const term = this.searchTerm().toLowerCase();
        if (term) {
            result = result.filter(c =>
                c.fullName.toLowerCase().includes(term) ||
                c.phone.includes(term) ||
                (c.email && c.email.toLowerCase().includes(term))
            );
        }

        // VIP filter
        if (this.filterVipOnly()) {
            result = result.filter(c => c.isVip);
        }

        this.filteredCustomers.set(result);
    }

    onSearch(event: Event): void {
        const value = (event.target as HTMLInputElement).value;
        this.searchTerm.set(value);
        this.applyFilters();
    }

    toggleVipFilter(): void {
        this.filterVipOnly.update(v => !v);
        this.applyFilters();
    }

    // Add Customer Modal
    openAddModal(): void {
        this.customerForm.reset({ isVip: false });
        this.selectedCustomer.set(null);
        this.showAddModal.set(true);
    }

    editCustomer(customer: CompanyCustomer): void {
        this.selectedCustomer.set(customer);
        this.customerForm.patchValue({
            fullName: customer.fullName,
            phone: customer.phone,
            email: customer.email,
            customPrice: customer.customPrice,
            discountPercent: customer.discountPercent,
            isVip: customer.isVip,
            notes: customer.notes
        });
        this.showAddModal.set(true);
    }

    closeAddModal(): void {
        this.showAddModal.set(false);
        this.selectedCustomer.set(null);
    }

    saveCustomer(): void {
        if (this.customerForm.invalid) return;

        this.isSaving.set(true);
        const request: CompanyCustomerRequest = this.customerForm.value;

        this.service.createOrUpdate(this.companyId, request).subscribe({
            next: () => {
                this.closeAddModal();
                this.loadCustomers();
                this.isSaving.set(false);
            },
            error: (err) => {
                console.error('Failed to save customer:', err);
                this.isSaving.set(false);
            }
        });
    }

    // Pricing Modal
    openPricingModal(customer: CompanyCustomer): void {
        this.selectedCustomer.set(customer);
        this.pricingForm.patchValue({
            customPrice: customer.customPrice,
            discountPercent: customer.discountPercent
        });
        this.showPricingModal.set(true);
    }

    closePricingModal(): void {
        this.showPricingModal.set(false);
        this.selectedCustomer.set(null);
    }

    savePricing(): void {
        if (this.pricingForm.invalid || !this.selectedCustomer()) return;

        this.isSaving.set(true);
        const pricing: PricingUpdate = this.pricingForm.value;

        this.service.setPricing(this.selectedCustomer()!.companyCustomerId, pricing).subscribe({
            next: () => {
                this.closePricingModal();
                this.loadCustomers();
                this.isSaving.set(false);
            },
            error: (err) => {
                console.error('Failed to update pricing:', err);
                this.isSaving.set(false);
            }
        });
    }

    // Toggle VIP
    toggleVip(customer: CompanyCustomer): void {
        this.service.setVipStatus(customer.companyCustomerId, !customer.isVip).subscribe({
            next: () => this.loadCustomers(),
            error: (err) => console.error('Failed to toggle VIP:', err)
        });
    }

    // Delete
    deleteCustomer(customer: CompanyCustomer): void {
        if (!confirm(`Xác nhận xóa khách hàng "${customer.fullName}"?`)) return;

        this.service.deleteCustomer(customer.companyCustomerId).subscribe({
            next: () => this.loadCustomers(),
            error: (err) => console.error('Failed to delete:', err)
        });
    }

    // Helpers
    formatPrice(price: number | null): string {
        return this.service.formatPrice(price);
    }

    formatCurrency(value: number): string {
        return new Intl.NumberFormat('vi-VN').format(value) + 'đ';
    }

    get totalCustomers(): number {
        return this.customers().length;
    }

    get vipCount(): number {
        return this.customers().filter(c => c.isVip).length;
    }

    get totalRevenue(): number {
        return this.customers().reduce((sum, c) => sum + c.totalRevenue, 0);
    }
}
