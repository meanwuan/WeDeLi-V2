import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-forgot-password',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    templateUrl: './forgot-password.component.html',
    styleUrl: './forgot-password.component.scss'
})
export class ForgotPasswordComponent {
    private fb = inject(FormBuilder);
    private authService = inject(AuthService);

    forgotPasswordForm: FormGroup;
    isLoading = false;
    errorMessage = '';
    successMessage = '';

    constructor() {
        this.forgotPasswordForm = this.fb.group({
            email: ['', [Validators.required, Validators.email]]
        });
    }

    onSubmit(): void {
        if (this.forgotPasswordForm.valid) {
            this.isLoading = true;
            this.errorMessage = '';
            this.successMessage = '';

            const email = this.forgotPasswordForm.get('email')?.value;

            this.authService.forgotPassword(email).subscribe({
                next: (response) => {
                    this.isLoading = false;
                    if (response.success) {
                        this.successMessage = 'Link đặt lại mật khẩu đã được gửi đến email của bạn. Vui lòng kiểm tra hộp thư.';
                        this.forgotPasswordForm.reset();
                    }
                },
                error: (error) => {
                    this.isLoading = false;
                    // For security, show success message even on error
                    this.successMessage = 'Nếu email tồn tại trong hệ thống, link đặt lại mật khẩu đã được gửi.';
                }
            });
        }
    }
}
