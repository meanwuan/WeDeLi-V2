import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../../auth/services/auth.service';

interface MenuItem {
    icon: string;
    label: string;
    route: string;
    badge?: number;
}

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.scss'
})
export class SidebarComponent {
    private router = inject(Router);
    private authService = inject(AuthService);

    isCollapsed = signal(false);
    currentRoute = signal('/dashboard');

    menuItems: MenuItem[] = [
        { icon: 'dashboard', label: 'Dashboard', route: '/dashboard' },
        { icon: 'orders', label: 'Đơn hàng', route: '/orders' },
        { icon: 'warehouse', label: 'Nhân viên kho', route: '/staff' },
        { icon: 'drivers', label: 'Tài xế', route: '/drivers' },
        { icon: 'customers', label: 'Khách quen', route: '/company-customers' },
        { icon: 'vehicles', label: 'Xe', route: '/vehicles' },
        { icon: 'routes', label: 'Tuyến đường', route: '/routes' },
        { icon: 'reports', label: 'Báo cáo', route: '/reports' },
        { icon: 'settings', label: 'Cài đặt', route: '/settings' }
    ];

    constructor() {
        // Track current route
        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd)
        ).subscribe((event: NavigationEnd) => {
            this.currentRoute.set(event.urlAfterRedirects);
        });
    }

    toggleSidebar(): void {
        this.isCollapsed.update(v => !v);
    }

    isActive(route: string): boolean {
        return this.currentRoute().startsWith(route);
    }

    navigate(route: string): void {
        this.router.navigate([route]);
    }

    logout(): void {
        this.authService.logout().subscribe({
            next: () => {
                // Redirect handled in clearSession
            },
            error: () => {
                // clearSession also called on error
            }
        });
    }

    get userName(): string {
        const user = this.authService.getCurrentUser();
        return user?.fullName || 'Admin User';
    }

    get userRole(): string {
        const user = this.authService.getCurrentUser();
        return user?.roleName || 'Quản lý';
    }

    get userInitials(): string {
        const user = this.authService.getCurrentUser();
        if (user?.fullName) {
            const parts = user.fullName.split(' ');
            if (parts.length >= 2) {
                return parts[0][0] + parts[parts.length - 1][0];
            }
            return user.fullName.substring(0, 2).toUpperCase();
        }
        return 'AU';
    }
}
