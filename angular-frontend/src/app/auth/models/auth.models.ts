// API Response wrapper
export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T | null;
    errors?: string[];
}

// Login
export interface LoginRequest {
    emailOrUsername: string;
    password: string;
    rememberMe?: boolean;
}

export interface LoginResponse {
    userId: number;
    username: string;
    fullName: string;
    email: string;
    phone: string;
    roleName: string;
    roleId: number;
    companyId: number | null;
    companyName: string | null;
    accessToken: string;
    refreshToken: string;
    tokenExpiration: string;
    refreshTokenExpiration: string;
}

// Register
export interface RegisterRequest {
    username: string;
    fullName: string;
    email?: string;
    phone: string;
    password: string;
    confirmPassword: string;
    roleId: number;
    companyId?: number;
}

export interface RegisterResponse {
    userId: number;
    username: string;
    fullName: string;
    email: string;
    phone: string;
    roleName: string;
    message: string;
}

// Password Management
export interface ForgotPasswordRequest {
    email: string;
}

export interface ResetPasswordRequest {
    email: string;
    resetToken: string;
    newPassword: string;
    confirmPassword: string;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword: string;
    confirmPassword: string;
}

// Token
export interface RefreshTokenRequest {
    accessToken: string;
    refreshToken: string;
}

export interface RefreshTokenResponse {
    accessToken: string;
    refreshToken: string;
    tokenExpiration: string;
    refreshTokenExpiration: string;
}

// Logout
export interface LogoutRequest {
    refreshToken: string;
}

// User Profile
export interface UserProfile {
    userId: number;
    username: string;
    fullName: string;
    phone: string;
    email: string;
    roleName: string;
    roleId: number;
    isActive: boolean;
    companyId: number | null;
    companyName: string | null;
    createdAt: string;
    updatedAt: string | null;
}
