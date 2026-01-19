import { Component, Input, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

export type StatCardVariant = 'default' | 'primary' | 'success' | 'warning' | 'danger';

@Component({
    selector: 'app-stat-card',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './stat-card.component.html',
    styleUrl: './stat-card.component.scss'
})
export class StatCardComponent {
    @Input() title = '';
    @Input() value: string | number = 0;
    @Input() icon = '';
    @Input() variant: StatCardVariant = 'default';
    @Input() trend?: number; // percentage change
    @Input() trendLabel?: string;
    @Input() subtitle?: string;
    @Input() loading = false;

    get formattedValue(): string {
        if (typeof this.value === 'number') {
            // Format large numbers with commas
            return new Intl.NumberFormat('vi-VN').format(this.value);
        }
        return this.value;
    }

    get trendClass(): string {
        if (!this.trend) return '';
        return this.trend >= 0 ? 'trend-up' : 'trend-down';
    }

    get trendIcon(): string {
        if (!this.trend) return '';
        return this.trend >= 0 ? '↑' : '↓';
    }
}
