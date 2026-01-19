import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})
export class LoginComponent {
    private fb = inject(FormBuilder);
    private authService = inject(AuthService);
    private router = inject(Router);

    loginForm: FormGroup;
    isLoading = false;
    errorMessage = '';
    showPassword = false;

    constructor() {
        this.loginForm = this.fb.group({
            emailOrUsername: ['', [Validators.required]],
            password: ['', [Validators.required, Validators.minLength(6)]],
            rememberMe: [false]
        });
    }

    togglePassword(): void {
        this.showPassword = !this.showPassword;
    }

    onSubmit(): void {
        if (this.loginForm.valid) {
            this.isLoading = true;
            this.errorMessage = '';

            this.authService.login(this.loginForm.value).subscribe({
                next: (response) => {
                    this.isLoading = false;
                    if (response.success && response.data) {
                        // Redirect based on role
                        const roleName = response.data.roleName?.toLowerCase();
                        // Admin roles go to Admin Dashboard
                        if (roleName === 'superadmin' || roleName === 'companyadmin' || roleName === 'warehousestaff') {
                            this.router.navigate(['/dashboard']);
                        } else {
                            // Customer, Driver -> Customer Portal
                            this.router.navigate(['/customer']);
                        }
                    }
                },
                error: (error) => {
                    this.isLoading = false;
                    this.errorMessage = error.message || 'Đăng nhập thất bại. Vui lòng thử lại.';
                }
            });
        }
    }

    loginWithGoogle(): void {
        // TODO: Implement Google OAuth
        console.log('Google login clicked');
    }

    loginWithFacebook(): void {
        // TODO: Implement Facebook OAuth
        console.log('Facebook login clicked');
    }
}
