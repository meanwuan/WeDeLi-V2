import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable, tap, catchError, throwError } from 'rxjs';
import { Router } from '@angular/router';
import {
    ApiResponse,
    LoginRequest,
    LoginResponse,
    RegisterRequest,
    RegisterResponse,
    ForgotPasswordRequest,
    ResetPasswordRequest,
    ChangePasswordRequest,
    RefreshTokenRequest,
    RefreshTokenResponse,
    LogoutRequest,
    UserProfile
} from '../models/auth.models';

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    private http = inject(HttpClient);
    private router = inject(Router);

    private readonly API_URL = 'http://localhost:5009/api/v1/auth';
    private readonly TOKEN_KEY = 'access_token';
    private readonly REFRESH_TOKEN_KEY = 'refresh_token';
    private readonly USER_KEY = 'current_user';

    private currentUserSubject = new BehaviorSubject<UserProfile | null>(this.getStoredUser());
    currentUser$ = this.currentUserSubject.asObservable();

    private isAuthenticatedSubject = new BehaviorSubject<boolean>(this.hasValidToken());
    isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

    // Login
    login(credentials: LoginRequest): Observable<ApiResponse<LoginResponse>> {
        return this.http.post<ApiResponse<LoginResponse>>(`${this.API_URL}/login`, credentials)
            .pipe(
                tap(response => {
                    if (response.success && response.data) {
                        this.storeTokens(response.data.accessToken, response.data.refreshToken);
                        this.storeUser({
                            userId: response.data.userId,
                            username: response.data.username,
                            fullName: response.data.fullName,
                            email: response.data.email,
                            phone: response.data.phone,
                            roleName: response.data.roleName,
                            roleId: response.data.roleId,
                            companyId: response.data.companyId,
                            companyName: response.data.companyName,
                            isActive: true,
                            createdAt: new Date().toISOString(),
                            updatedAt: null
                        });
                        this.isAuthenticatedSubject.next(true);
                    }
                }),
                catchError(this.handleError)
            );
    }

    // Register
    register(data: RegisterRequest): Observable<ApiResponse<RegisterResponse>> {
        return this.http.post<ApiResponse<RegisterResponse>>(`${this.API_URL}/register`, data)
            .pipe(catchError(this.handleError));
    }

    // Forgot Password
    forgotPassword(email: string): Observable<ApiResponse<null>> {
        const request: ForgotPasswordRequest = { email };
        return this.http.post<ApiResponse<null>>(`${this.API_URL}/forgot-password`, request)
            .pipe(catchError(this.handleError));
    }

    // Reset Password
    resetPassword(data: ResetPasswordRequest): Observable<ApiResponse<null>> {
        return this.http.post<ApiResponse<null>>(`${this.API_URL}/reset-password`, data)
            .pipe(catchError(this.handleError));
    }

    // Change Password
    changePassword(data: ChangePasswordRequest): Observable<ApiResponse<null>> {
        return this.http.post<ApiResponse<null>>(`${this.API_URL}/change-password`, data, {
            headers: this.getAuthHeaders()
        }).pipe(catchError(this.handleError));
    }

    // Logout
    logout(): Observable<ApiResponse<null>> {
        const refreshToken = this.getRefreshToken();
        const request: LogoutRequest = { refreshToken: refreshToken || '' };

        return this.http.post<ApiResponse<null>>(`${this.API_URL}/logout`, request, {
            headers: this.getAuthHeaders()
        }).pipe(
            tap(() => this.clearSession()),
            catchError(error => {
                this.clearSession();
                return throwError(() => error);
            })
        );
    }

    // Refresh Token
    refreshToken(): Observable<ApiResponse<RefreshTokenResponse>> {
        const request: RefreshTokenRequest = {
            accessToken: this.getAccessToken() || '',
            refreshToken: this.getRefreshToken() || ''
        };

        return this.http.post<ApiResponse<RefreshTokenResponse>>(`${this.API_URL}/refresh-token`, request)
            .pipe(
                tap(response => {
                    if (response.success && response.data) {
                        this.storeTokens(response.data.accessToken, response.data.refreshToken);
                    }
                }),
                catchError(this.handleError)
            );
    }

    // Token Management
    getAccessToken(): string | null {
        return localStorage.getItem(this.TOKEN_KEY);
    }

    getRefreshToken(): string | null {
        return localStorage.getItem(this.REFRESH_TOKEN_KEY);
    }

    isAuthenticated(): boolean {
        return this.hasValidToken();
    }

    getCurrentUser(): UserProfile | null {
        return this.currentUserSubject.value;
    }

    // Force logout without API call (for interceptor use)
    forceLogout(): void {
        this.clearSession();
    }

    // Private methods
    private storeTokens(accessToken: string, refreshToken: string): void {
        localStorage.setItem(this.TOKEN_KEY, accessToken);
        localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
    }

    private storeUser(user: UserProfile): void {
        localStorage.setItem(this.USER_KEY, JSON.stringify(user));
        this.currentUserSubject.next(user);
    }

    private getStoredUser(): UserProfile | null {
        const userStr = localStorage.getItem(this.USER_KEY);
        return userStr ? JSON.parse(userStr) : null;
    }

    private hasValidToken(): boolean {
        const token = this.getAccessToken();
        return !!token;
    }

    private getAuthHeaders(): HttpHeaders {
        const token = this.getAccessToken();
        return new HttpHeaders({
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        });
    }

    private clearSession(): void {
        localStorage.removeItem(this.TOKEN_KEY);
        localStorage.removeItem(this.REFRESH_TOKEN_KEY);
        localStorage.removeItem(this.USER_KEY);
        this.currentUserSubject.next(null);
        this.isAuthenticatedSubject.next(false);
        this.router.navigate(['/auth/login']);
    }

    private handleError(error: any): Observable<never> {
        let errorMessage = 'Đã xảy ra lỗi';
        if (error.error?.message) {
            errorMessage = error.error.message;
        } else if (error.message) {
            errorMessage = error.message;
        }
        return throwError(() => new Error(errorMessage));
    }
}
