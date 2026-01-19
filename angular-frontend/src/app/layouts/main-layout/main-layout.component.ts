import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../../shared/components/sidebar/sidebar.component';
import { HeaderComponent } from '../../shared/components/header/header.component';

@Component({
    selector: 'app-main-layout',
    standalone: true,
    imports: [CommonModule, RouterOutlet, SidebarComponent, HeaderComponent],
    template: `
        <div class="layout">
            <app-sidebar />
            <div class="layout-main">
                <app-header [pageTitle]="pageTitle" />
                <main class="main-content">
                    <router-outlet />
                </main>
            </div>
        </div>
    `,
    styles: [`
        .layout {
            display: flex;
            min-height: 100vh;
            background: var(--bg-secondary);
        }

        .layout-main {
            flex: 1;
            margin-left: var(--sidebar-width, 260px);
            transition: margin-left 0.3s ease;
        }

        .main-content {
            padding: 24px;
            margin-top: var(--header-height, 64px);
            min-height: calc(100vh - var(--header-height, 64px));
        }

        :host-context(.sidebar-collapsed) .layout-main {
            margin-left: var(--sidebar-collapsed-width, 72px);
        }
    `]
})
export class MainLayoutComponent implements OnInit {
    pageTitle = 'Dashboard';

    ngOnInit(): void {
        // Initialize theme from localStorage or default to dark
        const savedTheme = localStorage.getItem('theme');
        if (savedTheme === 'light') {
            document.documentElement.setAttribute('data-theme', 'light');
        } else {
            // Default to dark theme - no attribute needed since :root is dark
            document.documentElement.removeAttribute('data-theme');
            localStorage.setItem('theme', 'dark');
        }
    }
}

