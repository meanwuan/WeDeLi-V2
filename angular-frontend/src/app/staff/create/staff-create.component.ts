import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StaffService, CreateStaffDto } from '../services/staff.service';
import { AuthService } from '../../auth/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { ApiResponse } from '../../core/models/common.model';

@Component({
    selector: 'app-staff-create',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule],
    templateUrl: './staff-create.component.html',
    styleUrl: './staff-create.component.scss'
})
export class StaffCreateComponent implements OnInit {
    private staffService = inject(StaffService);
    private authService = inject(AuthService);
    private apiService = inject(ApiService);
    private router = inject(Router);

    // Form data - NEW EMPLOYEE INFO
    fullName = signal('');
    phone = signal('');
    email = signal('');
    username = signal('');
    password = signal('');
    confirmPassword = signal('');
    warehouseLocation = signal('');

    // State
    loading = signal(false);
    error = signal<string | null>(null);
    success = signal(false);

    // Company info from logged-in user
    private companyId = 0;
    companyName = signal('');

    // Role ID for WarehouseStaff (from your DB)
    private readonly WAREHOUSE_STAFF_ROLE_ID = 4;

    ngOnInit(): void {
        const user = this.authService.getCurrentUser();
        this.companyId = user?.companyId ?? 0;
        this.companyName.set(user?.companyName ?? 'Không xác định');
        console.log('StaffCreate - User:', user);
        console.log('StaffCreate - CompanyId:', this.companyId);
    }

    // Auto-generate username from phone
    onPhoneChange(): void {
        if (this.phone() && !this.username()) {
            this.username.set('nv_' + this.phone());
        }
    }

    onSubmit(): void {
        // Validation
        if (!this.fullName()) {
            this.error.set('Vui lòng nhập họ tên');
            return;
        }
        if (!this.phone()) {
            this.error.set('Vui lòng nhập số điện thoại');
            return;
        }
        if (!this.email()) {
            this.error.set('Vui lòng nhập email');
            return;
        }
        if (!this.password()) {
            this.error.set('Vui lòng nhập mật khẩu');
            return;
        }
        if (this.password() !== this.confirmPassword()) {
            this.error.set('Mật khẩu xác nhận không khớp');
            return;
        }
        if (!this.warehouseLocation()) {
            this.error.set('Vui lòng nhập vị trí kho');
            return;
        }

        this.loading.set(true);
        this.error.set(null);

        // Step 1: Register new user with WarehouseStaff role
        const registerDto = {
            username: this.username() || ('nv_' + this.phone()),
            email: this.email(),
            phone: this.phone(),
            fullName: this.fullName(),
            password: this.password(),
            confirmPassword: this.confirmPassword(),
            roleId: this.WAREHOUSE_STAFF_ROLE_ID,
            companyId: this.companyId
        };

        this.apiService.post<ApiResponse<{ userId: number }>>('/auth/register', registerDto).subscribe({
            next: (response: ApiResponse<{ userId: number }>) => {
                console.log('Register response:', response);
                const userId = response.data?.userId;
                console.log('Extracted userId:', userId);
                if (!userId) {
                    this.loading.set(false);
                    this.error.set('Không thể lấy userId sau khi đăng ký. Response: ' + JSON.stringify(response));
                    return;
                }

                // Step 2: Create staff record
                const staffDto: CreateStaffDto = {
                    userId: userId,
                    companyId: this.companyId,
                    warehouseLocation: this.warehouseLocation()
                };

                this.staffService.createStaff(staffDto).subscribe({
                    next: () => {
                        this.loading.set(false);
                        this.success.set(true);
                        setTimeout(() => {
                            this.router.navigate(['/staff']);
                        }, 1500);
                    },
                    error: (err) => {
                        this.loading.set(false);
                        const message = err.error?.message || err.message || 'Không thể tạo nhân viên. Vui lòng thử lại.';
                        this.error.set(message);
                    }
                });
            },
            error: (err) => {
                this.loading.set(false);
                // Try to get detailed error message from backend
                let message = 'Không thể đăng ký tài khoản. Vui lòng thử lại.';
                if (err.error?.errors && Array.isArray(err.error.errors)) {
                    message = err.error.errors.join(', ');
                } else if (err.error?.message) {
                    message = err.error.message;
                } else if (err.message) {
                    message = err.message;
                }
                console.log('Register error:', err.error);
                this.error.set(message);
            }
        });
    }

    goBack(): void {
        this.router.navigate(['/staff']);
    }
}
