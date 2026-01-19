import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';

@Component({
    selector: 'app-customer-layout',
    standalone: true,
    imports: [CommonModule, RouterModule],
    templateUrl: './customer-layout.component.html',
    styleUrl: './customer-layout.component.scss'
})
export class CustomerLayoutComponent {
    sidebarItems = [
        { icon: 'home', label: 'Trang chủ', route: '/customer' },
        { icon: 'package', label: 'Đơn hàng', route: '/customer/orders' },
        { icon: 'map-pin', label: 'Địa chỉ', route: '/customer/addresses' },
        { icon: 'settings', label: 'Cài đặt', route: '/customer/settings' }
    ];

    constructor(private authService: AuthService) { }

    get userName(): string {
        const user = this.authService.getCurrentUser();
        return user?.fullName || 'Khách hàng';
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
}
