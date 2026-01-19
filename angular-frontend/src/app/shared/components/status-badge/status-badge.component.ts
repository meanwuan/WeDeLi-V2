import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type BadgeVariant = 'default' | 'success' | 'warning' | 'danger' | 'info' | 'primary';

@Component({
    selector: 'app-status-badge',
    standalone: true,
    imports: [CommonModule],
    template: `
        <span class="badge" [class]="'badge-' + variant" [class.badge-outline]="outline">
            <span class="badge-dot" *ngIf="showDot"></span>
            {{ label }}
        </span>
    `,
    styles: [`
        .badge {
            display: inline-flex;
            align-items: center;
            gap: 6px;
            padding: 4px 12px;
            border-radius: 20px;
            font-size: 0.75rem;
            font-weight: 600;
            white-space: nowrap;
        }

        .badge-dot {
            width: 6px;
            height: 6px;
            border-radius: 50%;
            background: currentColor;
        }

        .badge-default {
            background: var(--bg-tertiary, #f3f4f6);
            color: var(--text-secondary, #6b7280);
        }

        .badge-success {
            background: var(--success-bg, #dcfce7);
            color: var(--success-color, #22c55e);
        }

        .badge-warning {
            background: var(--warning-bg, #fef3c7);
            color: var(--warning-color, #f59e0b);
        }

        .badge-danger {
            background: var(--danger-bg, #fee2e2);
            color: var(--danger-color, #ef4444);
        }

        .badge-info {
            background: var(--info-bg, #dbeafe);
            color: var(--info-color, #3b82f6);
        }

        .badge-primary {
            background: var(--primary-light, #fff0eb);
            color: var(--primary-color, #FF6B35);
        }

        .badge-outline {
            background: transparent;
            border: 1px solid currentColor;
        }
    `]
})
export class StatusBadgeComponent {
    @Input() label = '';
    @Input() variant: BadgeVariant = 'default';
    @Input() showDot = false;
    @Input() outline = false;
}
