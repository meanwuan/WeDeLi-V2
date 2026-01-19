import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

// Helper to check if user is admin role
const isAdminRole = (roleName: string | undefined): boolean => {
    const role = roleName?.toLowerCase();
    return role === 'superadmin' || role === 'companyadmin' || role === 'warehousestaff';
};

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (authService.isAuthenticated()) {
        return true;
    }

    router.navigate(['/auth/login'], {
        queryParams: { returnUrl: state.url }
    });
    return false;
};

// Guard for guest-only routes (login, register)
export const noAuthGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
        return true;
    }

    // Redirect based on role
    const user = authService.getCurrentUser();
    if (isAdminRole(user?.roleName)) {
        router.navigate(['/dashboard']);
    } else {
        router.navigate(['/customer']);
    }
    return false;
};

// Guard for admin-only routes (SuperAdmin, CompanyAdmin, WarehouseStaff)
export const adminGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
        router.navigate(['/auth/login']);
        return false;
    }

    const user = authService.getCurrentUser();
    if (isAdminRole(user?.roleName)) {
        return true;
    }

    // Non-admin user trying to access admin routes - redirect to customer portal
    router.navigate(['/customer']);
    return false;
};

// Guard for customer-only routes (Customer, Driver)
export const customerGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
        router.navigate(['/auth/login']);
        return false;
    }

    const user = authService.getCurrentUser();
    if (!isAdminRole(user?.roleName)) {
        return true;
    }

    // Admin user trying to access customer routes - redirect to admin dashboard
    router.navigate(['/dashboard']);
    return false;
};
