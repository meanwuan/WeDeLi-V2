import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    templateUrl: './register.component.html',
    styleUrl: './register.component.scss'
})
export class RegisterComponent {
    private fb = inject(FormBuilder);
    private authService = inject(AuthService);
    private router = inject(Router);

    registerForm: FormGroup;
    isLoading = false;
    errorMessage = '';
    showPassword = false;
    showConfirmPassword = false;
    passwordStrength = { level: 0, text: '' };
    agreedToTerms = false;

    constructor() {
        this.registerForm = this.fb.group({
            fullName: ['', [Validators.required, Validators.maxLength(200)]],
            email: ['', [Validators.email]],
            phone: ['', [Validators.required, Validators.pattern(/^(0|\+84)[0-9]{9,10}$/)]],
            password: ['', [
                Validators.required,
                Validators.minLength(6),
                Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{6,}$/)
            ]],
            confirmPassword: ['', [Validators.required]],
            agreeTerms: [false, [Validators.requiredTrue]]
        }, { validators: this.passwordMatchValidator });

        // Watch password changes for strength indicator
        this.registerForm.get('password')?.valueChanges.subscribe(value => {
            this.calculatePasswordStrength(value);
        });
    }

    passwordMatchValidator(control: AbstractControl): { [key: string]: boolean } | null {
        const password = control.get('password');
        const confirmPassword = control.get('confirmPassword');

        if (password && confirmPassword && password.value !== confirmPassword.value) {
            confirmPassword.setErrors({ mismatch: true });
            return { mismatch: true };
        }
        return null;
    }

    calculatePasswordStrength(password: string): void {
        let score = 0;

        if (!password) {
            this.passwordStrength = { level: 0, text: '' };
            return;
        }

        // Length check
        if (password.length >= 6) score++;
        if (password.length >= 10) score++;

        // Contains lowercase
        if (/[a-z]/.test(password)) score++;

        // Contains uppercase
        if (/[A-Z]/.test(password)) score++;

        // Contains number
        if (/\d/.test(password)) score++;

        // Contains special character
        if (/[^a-zA-Z0-9]/.test(password)) score++;

        if (score <= 2) {
            this.passwordStrength = { level: 1, text: 'Yếu' };
        } else if (score <= 4) {
            this.passwordStrength = { level: 2, text: 'Trung bình' };
        } else {
            this.passwordStrength = { level: 3, text: 'Mạnh' };
        }
    }

    togglePassword(): void {
        this.showPassword = !this.showPassword;
    }

    toggleConfirmPassword(): void {
        this.showConfirmPassword = !this.showConfirmPassword;
    }

    onSubmit(): void {
        if (this.registerForm.valid) {
            this.isLoading = true;
            this.errorMessage = '';

            const formData = this.registerForm.value;

            this.authService.register({
                username: formData.email || formData.phone, // Use email or phone as username
                fullName: formData.fullName,
                email: formData.email,
                phone: formData.phone,
                password: formData.password,
                confirmPassword: formData.confirmPassword,
                roleId: 5 // Customer role
            }).subscribe({
                next: (response) => {
                    this.isLoading = false;
                    if (response.success) {
                        // Redirect to login with success message
                        this.router.navigate(['/auth/login'], {
                            queryParams: { registered: 'true' }
                        });
                    }
                },
                error: (error) => {
                    this.isLoading = false;
                    this.errorMessage = error.message || 'Đăng ký thất bại. Vui lòng thử lại.';
                }
            });
        }
    }
}
