import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { StatusBadgeComponent } from '../shared/components/status-badge/status-badge.component';
import { StaffService, Staff, StaffStatusCount, StaffFilters } from './services/staff.service';
import { AuthService } from '../auth/services/auth.service';

type ViewMode = 'grid' | 'list';

@Component({
    selector: 'app-staff',
    standalone: true,
    imports: [CommonModule, FormsModule, RouterModule, StatusBadgeComponent],
    templateUrl: './staff.component.html',
    styleUrl: './staff.component.scss'
})
export class StaffComponent implements OnInit {
    private staffService = inject(StaffService);
    private authService = inject(AuthService);

    private get companyId(): number {
        const user = this.authService.getCurrentUser();
        console.log('StaffComponent - User:', user);
        return user?.companyId ?? 0;
    }

    // State
    staff = signal<Staff[]>([]);
    loading = signal(true);
    error = signal<string | null>(null);

    // View mode
    viewMode = signal<ViewMode>('grid');

    // Filters
    searchTerm = signal('');
    locationFilter = signal('');
    locations = signal<string[]>([]);

    // Status counts
    statusCounts = signal<StaffStatusCount>({
        total: 0,
        active: 0,
        inactive: 0
    });

    // Computed
    filteredStaff = computed(() => {
        let result = this.staff();

        if (this.searchTerm()) {
            const term = this.searchTerm().toLowerCase();
            result = result.filter(s =>
                s.fullName.toLowerCase().includes(term) ||
                s.phone.includes(term)
            );
        }

        if (this.locationFilter()) {
            result = result.filter(s => s.warehouseLocation === this.locationFilter());
        }

        return result;
    });

    ngOnInit(): void {
        this.loadLocations();
        this.loadStatusCounts();
        this.loadStaff();
    }

    loadLocations(): void {
        this.staffService.getLocations(this.companyId).subscribe({
            next: (locations) => this.locations.set(locations),
            error: (err) => console.error('Failed to load locations:', err)
        });
    }

    loadStatusCounts(): void {
        this.staffService.getStatusCounts(this.companyId).subscribe({
            next: (counts) => this.statusCounts.set(counts),
            error: (err) => console.error('Failed to load status counts:', err)
        });
    }

    loadStaff(): void {
        this.loading.set(true);
        this.error.set(null);

        this.staffService.getStaffByCompany(this.companyId).subscribe({
            next: (staffList) => {
                console.log('StaffComponent - Response:', staffList);
                this.staff.set(staffList);
                this.loading.set(false);
            },
            error: (err) => {
                console.error('StaffComponent - Error:', err);
                this.error.set(err.message || 'Không thể tải danh sách nhân viên');
                this.loading.set(false);
                this.staff.set([]);
            }
        });
    }

    onSearch(): void {
        // Filtering is done via computed signal
    }

    onLocationFilterChange(location: string): void {
        this.locationFilter.set(location);
    }

    setViewMode(mode: ViewMode): void {
        this.viewMode.set(mode);
    }

    toggleStaffStatus(staffMember: Staff, event: Event): void {
        event.stopPropagation();
        const newStatus = !staffMember.isActive;

        this.staffService.toggleStaffStatus(staffMember.staffId, newStatus).subscribe({
            next: () => {
                // Update local state
                const updatedStaff = this.staff().map(s =>
                    s.staffId === staffMember.staffId ? { ...s, isActive: newStatus } : s
                );
                this.staff.set(updatedStaff);
                this.loadStatusCounts();
            },
            error: (err) => console.error('Failed to toggle status:', err)
        });
    }

    // UI helpers
    getInitials(name: string): string {
        return name.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
    }

    getStaffCode(staffId: number): string {
        return `#ST${staffId.toString().padStart(4, '0')}`;
    }

    getStatusVariant(isActive: boolean): 'success' | 'default' {
        return isActive ? 'success' : 'default';
    }

    getStatusLabel(isActive: boolean): string {
        return isActive ? 'Active' : 'Inactive';
    }

    // Stats growth percentage (mock data)
    getGrowthPercentage(): string {
        return '+12%';
    }
}
