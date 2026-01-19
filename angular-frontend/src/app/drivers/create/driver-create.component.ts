import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { DriverService, CreateDriverDto } from '../services/driver.service';
import { AuthService } from '../../auth/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { ApiResponse } from '../../core/models/common.model';

@Component({
    selector: 'app-driver-create',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule],
    templateUrl: './driver-create.component.html',
    styleUrl: './driver-create.component.scss'
})
export class DriverCreateComponent implements OnInit {
    private driverService = inject(DriverService);
    private authService = inject(AuthService);
    private apiService = inject(ApiService);
    private router = inject(Router);

    // Form data - NEW DRIVER INFO
    fullName = signal('');
    phone = signal('');
    email = signal('');
    username = signal('');
    password = signal('');
    confirmPassword = signal('');
    driverLicense = signal('');
    licenseExpiry = signal('');

    // State
    loading = signal(false);
    error = signal<string | null>(null);
    success = signal(false);

    // Company info from logged-in user
    private companyId = 0;
    companyName = signal('');

    // Role ID for Driver (from your DB)
    private readonly DRIVER_ROLE_ID = 3;

    ngOnInit(): void {
        const user = this.authService.getCurrentUser();
        this.companyId = user?.companyId ?? 0;
        this.companyName.set(user?.companyName ?? 'Không xác định');
    }

    // Auto-generate username from phone
    onPhoneChange(): void {
        if (this.phone() && !this.username()) {
            this.username.set('tx_' + this.phone());
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
        if (!this.driverLicense()) {
            this.error.set('Vui lòng nhập số bằng lái');
            return;
        }
        if (!this.licenseExpiry()) {
            this.error.set('Vui lòng chọn ngày hết hạn bằng lái');
            return;
        }

        this.loading.set(true);
        this.error.set(null);

        // Step 1: Register new user with Driver role
        const registerDto = {
            username: this.username() || ('tx_' + this.phone()),
            email: this.email(),
            phone: this.phone(),
            fullName: this.fullName(),
            password: this.password(),
            confirmPassword: this.confirmPassword(),
            roleId: this.DRIVER_ROLE_ID,
            companyId: this.companyId
        };

        this.apiService.post<ApiResponse<{ userId: number }>>('/auth/register', registerDto).subscribe({
            next: (response: ApiResponse<{ userId: number }>) => {
                const userId = response.data?.userId;
                if (!userId) {
                    this.loading.set(false);
                    this.error.set('Không thể lấy userId sau khi đăng ký');
                    return;
                }

                // Step 2: Create driver record
                // Convert date string (yyyy-MM-dd) to ISO DateTime format
                const expiryDate = new Date(this.licenseExpiry() + 'T00:00:00');
                const driverDto: CreateDriverDto = {
                    userId: userId,
                    companyId: this.companyId,
                    driverLicense: this.driverLicense(),
                    licenseExpiry: expiryDate.toISOString()
                };

                this.driverService.createDriver(driverDto).subscribe({
                    next: () => {
                        this.loading.set(false);
                        this.success.set(true);
                        setTimeout(() => {
                            this.router.navigate(['/drivers']);
                        }, 1500);
                    },
                    error: (err) => {
                        this.loading.set(false);
                        const message = err.error?.message || err.message || 'Không thể tạo tài xế. Vui lòng thử lại.';
                        this.error.set(message);
                    }
                });
            },
            error: (err) => {
                this.loading.set(false);
                const message = err.error?.message || err.message || 'Không thể đăng ký tài khoản. Vui lòng thử lại.';
                this.error.set(message);
            }
        });
    }

    goBack(): void {
        this.router.navigate(['/drivers']);
    }
}
