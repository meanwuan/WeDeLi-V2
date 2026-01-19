import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReportService, DailyReportDto, ReportSummary } from './services/report.service';
import { AuthService } from '../auth/services/auth.service';

@Component({
    selector: 'app-reports',
    standalone: true,
    imports: [CommonModule, FormsModule],
    templateUrl: './reports.component.html',
    styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
    private reportService = inject(ReportService);
    private authService = inject(AuthService);

    private get companyId(): number {
        return this.authService.getCurrentUser()?.companyId ?? 0;
    }

    // State
    loading = signal(true);
    summary = signal<ReportSummary | null>(null);
    dailyData = signal<DailyReportDto[]>([]);
    activeTab = signal<'overview' | 'orders' | 'revenue'>('overview');

    // Date range
    startDate = signal(this.getDefaultStartDate());
    endDate = signal(this.getDefaultEndDate());

    tabs = [
        { id: 'overview' as const, label: 'Tổng quan', icon: '📊' },
        { id: 'orders' as const, label: 'Đơn hàng', icon: '📦' },
        { id: 'revenue' as const, label: 'Doanh thu', icon: '💰' }
    ];

    ngOnInit(): void {
        this.loadReportData();
    }

    loadReportData(): void {
        this.loading.set(true);

        // Load summary
        this.reportService.getReportSummary(this.companyId, this.startDate(), this.endDate()).subscribe({
            next: (data) => {
                this.summary.set(data);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });

        // Load daily data
        this.reportService.getDailyReport(this.companyId, this.startDate(), this.endDate()).subscribe({
            next: (data) => this.dailyData.set(data)
        });
    }

    onDateChange(): void {
        this.loadReportData();
    }

    onTabChange(tabId: 'overview' | 'orders' | 'revenue'): void {
        this.activeTab.set(tabId);
    }

    exportToExcel(): void {
        this.reportService.exportReport('excel', {
            startDate: this.startDate(),
            endDate: this.endDate(),
            companyId: this.companyId,
            reportType: 'daily'
        }).subscribe({
            next: (blob) => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `report_${this.startDate()}_${this.endDate()}.xlsx`;
                a.click();
                window.URL.revokeObjectURL(url);
            },
            error: (err) => console.error('Export failed:', err)
        });
    }

    formatCurrency(amount: number): string {
        return this.reportService.formatCurrency(amount);
    }

    formatPercent(value: number): string {
        return this.reportService.formatPercent(value);
    }

    private getDefaultStartDate(): string {
        const date = new Date();
        date.setDate(date.getDate() - 30);
        return date.toISOString().split('T')[0];
    }

    private getDefaultEndDate(): string {
        return new Date().toISOString().split('T')[0];
    }

    // Chart helpers
    getMaxRevenue(): number {
        const data = this.dailyData();
        if (data.length === 0) return 100000;
        return Math.max(...data.map(d => d.totalRevenue));
    }

    getBarHeight(revenue: number): number {
        const max = this.getMaxRevenue();
        return max > 0 ? (revenue / max) * 100 : 0;
    }

    // Line chart helpers
    getPointX(index: number): number {
        const data = this.dailyData();
        if (data.length <= 1) return 50;
        return (index / (data.length - 1)) * 100;
    }

    getPointY(revenue: number): number {
        const max = this.getMaxRevenue();
        return max > 0 ? (revenue / max) * 100 : 0;
    }

    getLinePath(): string {
        const data = this.dailyData();
        if (data.length === 0) return '';

        const max = this.getMaxRevenue();
        const points = data.map((item, i) => {
            const x = data.length > 1 ? (i / (data.length - 1)) * 100 : 50;
            const y = max > 0 ? 100 - (item.totalRevenue / max) * 100 : 50;
            return `${x},${y}`;
        });

        return `M ${points.join(' L ')}`;
    }

    getAreaPath(): string {
        const data = this.dailyData();
        if (data.length === 0) return '';

        const max = this.getMaxRevenue();
        const points = data.map((item, i) => {
            const x = data.length > 1 ? (i / (data.length - 1)) * 100 : 50;
            const y = max > 0 ? 100 - (item.totalRevenue / max) * 100 : 50;
            return `${x},${y}`;
        });

        return `M 0,100 L ${points.join(' L ')} L 100,100 Z`;
    }

    getYAxisLabels(): number[] {
        const max = this.getMaxRevenue();
        return [max, max * 0.75, max * 0.5, max * 0.25, 0];
    }

    formatShortCurrency(amount: number): string {
        if (amount >= 1000000) {
            return (amount / 1000000).toFixed(1) + 'M';
        } else if (amount >= 1000) {
            return (amount / 1000).toFixed(0) + 'K';
        }
        return amount.toString();
    }
}
