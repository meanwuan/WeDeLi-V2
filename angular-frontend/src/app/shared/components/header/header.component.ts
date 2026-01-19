import { Component, Input, Output, EventEmitter, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
    selector: 'app-header',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss'
})
export class HeaderComponent {
    private router = inject(Router);

    @Input() pageTitle = 'Dashboard';
    @Input() breadcrumbs: string[] = ['Pages'];
    @Input() showSearch = true;
    @Output() toggleDarkMode = new EventEmitter<void>();

    searchQuery = signal('');
    showNotifications = signal(false);
    showUserMenu = signal(false);
    isDarkMode = signal(false);

    notifications = [
        { id: 1, title: 'Đơn hàng mới', message: 'Có 5 đơn hàng mới cần xử lý', time: '2 phút trước', read: false },
        { id: 2, title: 'Xe #51A-12345', message: 'Đã hoàn thành chuyến giao hàng', time: '15 phút trước', read: true }
    ];

    unreadCount = this.notifications.filter(n => !n.read).length;

    onSearch(): void {
        if (this.searchQuery()) {
            // Implement global search
            console.log('Searching:', this.searchQuery());
        }
    }

    toggleTheme(): void {
        this.isDarkMode.update(v => !v);
        if (this.isDarkMode()) {
            // Dark mode - remove attribute since :root is dark by default
            document.documentElement.removeAttribute('data-theme');
            localStorage.setItem('theme', 'dark');
        } else {
            // Light mode - set data-theme="light"
            document.documentElement.setAttribute('data-theme', 'light');
            localStorage.setItem('theme', 'light');
        }
        this.toggleDarkMode.emit();
    }

    toggleNotificationsPanel(): void {
        this.showNotifications.update(v => !v);
        this.showUserMenu.set(false);
    }

    toggleUserMenu(): void {
        this.showUserMenu.update(v => !v);
        this.showNotifications.set(false);
    }

    logout(): void {
        localStorage.removeItem('access_token');
        localStorage.removeItem('refresh_token');
        localStorage.removeItem('current_user');
        this.router.navigate(['/auth/login']);
    }

    closeDropdowns(): void {
        this.showNotifications.set(false);
        this.showUserMenu.set(false);
    }
}
