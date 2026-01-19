import { Injectable, inject } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ApiService } from '../../core/services/api.service';

// Matching backend DailyReportSummaryDto
export interface DailyReportDto {
    date: string;
    totalOrders: number;
    completedOrders: number;
    cancelledOrders: number;
    pendingOrders: number;
    totalRevenue: number;
    generatedAt: string;
}

export interface MonthlyReportDto {
    month: string;
    year: number;
    totalOrders: number;
    completedOrders: number;
    cancelledOrders: number;
    averageDeliveryTime: number;
    totalRevenue: number;
    growthRate: number;
}

// Matching backend ReportSummaryDto
export interface ReportSummary {
    totalOrders: number;
    completedOrders: number;
    cancelledOrders: number;
    pendingOrders: number;
    completionRate: number;
    totalRevenue: number;
    averageOrderValue: number;
    totalDrivers: number;
    totalVehicles: number;
    startDate: string | null;
    endDate: string | null;
}

export interface ReportFilters {
    startDate: string;
    endDate: string;
    companyId?: number;
    reportType: 'daily' | 'monthly' | 'summary';
}

@Injectable({
    providedIn: 'root'
})
export class ReportService {
    private api = inject(ApiService);
    private readonly endpoint = '/reports';

    // Get daily reports for a date range - calls /reports/daily/range
    getDailyReport(companyId: number, startDate: string, endDate: string): Observable<DailyReportDto[]> {
        const params = { startDate, endDate };
        return this.api.get<DailyReportDto[]>(`${this.endpoint}/daily/range`, params).pipe(
            map(response => response || []),
            catchError(() => of([]))
        );
    }

    // Get monthly report - calls /reports/monthly
    getMonthlyReport(year: number, month: number): Observable<DailyReportDto[]> {
        const params = { year, month };
        return this.api.get<DailyReportDto[]>(`${this.endpoint}/monthly`, params).pipe(
            map(response => response || []),
            catchError(() => of([]))
        );
    }

    // Get report summary - calls /reports/summary
    getReportSummary(companyId: number, startDate?: string, endDate?: string): Observable<ReportSummary> {
        const params: any = { companyId };
        if (startDate) params.startDate = startDate;
        if (endDate) params.endDate = endDate;

        return this.api.get<ReportSummary>(`${this.endpoint}/summary`, params).pipe(
            map(response => response),
            catchError(() => of({
                totalOrders: 0,
                completedOrders: 0,
                cancelledOrders: 0,
                pendingOrders: 0,
                completionRate: 0,
                totalRevenue: 0,
                averageOrderValue: 0,
                totalDrivers: 0,
                totalVehicles: 0,
                startDate: null,
                endDate: null
            }))
        );
    }

    // Export report - placeholder
    exportReport(format: 'excel' | 'pdf', filters: ReportFilters): Observable<Blob> {
        return this.api.getBlob(`${this.endpoint}/export`, {
            format,
            ...filters
        });
    }

    // Format currency
    formatCurrency(amount: number): string {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    }

    // Format percentage
    formatPercent(value: number): string {
        return value.toFixed(1) + '%';
    }
}

