import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-settings',
    standalone: true,
    imports: [CommonModule],
    template: `
        <div class="page-placeholder">
            <h1>Cài đặt</h1>
            <p>Trang này đang được phát triển...</p>
        </div>
    `,
    styles: [`
        .page-placeholder {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            min-height: 400px;
            background: var(--bg-primary, white);
            border-radius: var(--radius-lg, 12px);
            border: 1px solid var(--border-color, #e5e7eb);

            h1 {
                font-size: 1.5rem;
                color: var(--text-primary, #111827);
                margin-bottom: 8px;
            }

            p {
                color: var(--text-secondary, #6b7280);
            }
        }
    `]
})
export class SettingsComponent { }
