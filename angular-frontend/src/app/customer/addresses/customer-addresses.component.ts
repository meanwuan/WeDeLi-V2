import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../auth/services/auth.service';

interface Address {
  addressId: number;
  addressName: string;
  fullAddress: string;
  province: string;
  district: string;
  ward: string;
  street: string;
  isDefault: boolean;
  contactName: string;
  contactPhone: string;
}

@Component({
  selector: 'app-customer-addresses',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="customer-addresses">
      <header class="page-header">
        <h1>Sổ địa chỉ</h1>
        <button class="add-btn" (click)="showAddForm = true">+ Thêm địa chỉ mới</button>
      </header>
      
      <!-- Loading State -->
      @if (isLoading()) {
      <div class="loading-state">
        <div class="spinner"></div>
        <p>Đang tải danh sách địa chỉ...</p>
      </div>
      }
      
      <!-- Empty State -->
      @if (!isLoading() && addresses().length === 0) {
      <div class="empty-state">
        <div class="empty-icon">📍</div>
        <h3>Chưa có địa chỉ nào</h3>
        <p>Thêm địa chỉ để tiện lợi khi tạo đơn hàng</p>
        <button class="add-btn" (click)="showAddForm = true">+ Thêm địa chỉ đầu tiên</button>
      </div>
      }
      
      <!-- Addresses List -->
      @if (!isLoading() && addresses().length > 0) {
      <div class="addresses-list">
        @for (addr of addresses(); track addr.addressId) {
        <div class="address-card" [class.default]="addr.isDefault">
          @if (addr.isDefault) {
          <span class="default-badge">Mặc định</span>
          }
          <div class="address-header">
            <h3>{{ addr.addressName || 'Địa chỉ' }}</h3>
            <div class="actions">
              <button class="edit-btn" (click)="editAddress(addr)">✏️</button>
              <button class="delete-btn" (click)="deleteAddress(addr.addressId)">🗑️</button>
            </div>
          </div>
          <div class="address-body">
            <p class="contact">
              <strong>{{ addr.contactName }}</strong> • {{ addr.contactPhone }}
            </p>
            <p class="full-address">{{ addr.fullAddress || buildFullAddress(addr) }}</p>
          </div>
          @if (!addr.isDefault) {
          <button class="set-default-btn" (click)="setDefault(addr.addressId)">
            Đặt làm mặc định
          </button>
          }
        </div>
        }
      </div>
      }
      
      <!-- Add/Edit Form Modal -->
      @if (showAddForm) {
      <div class="modal-overlay" (click)="closeForm()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <h2>{{ editingAddress ? 'Chỉnh sửa địa chỉ' : 'Thêm địa chỉ mới' }}</h2>
          <form (ngSubmit)="saveAddress()">
            <div class="form-group">
              <label>Tên địa chỉ</label>
              <input type="text" [(ngModel)]="formData.addressName" name="addressName" placeholder="VD: Nhà, Công ty...">
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Họ tên người nhận *</label>
                <input type="text" [(ngModel)]="formData.contactName" name="contactName" required>
              </div>
              <div class="form-group">
                <label>Số điện thoại *</label>
                <input type="text" [(ngModel)]="formData.contactPhone" name="contactPhone" required>
              </div>
            </div>
            <div class="form-group">
              <label>Tỉnh/Thành phố *</label>
              <input type="text" [(ngModel)]="formData.province" name="province" required>
            </div>
            <div class="form-row">
              <div class="form-group">
                <label>Quận/Huyện *</label>
                <input type="text" [(ngModel)]="formData.district" name="district" required>
              </div>
              <div class="form-group">
                <label>Phường/Xã *</label>
                <input type="text" [(ngModel)]="formData.ward" name="ward" required>
              </div>
            </div>
            <div class="form-group">
              <label>Địa chỉ chi tiết *</label>
              <input type="text" [(ngModel)]="formData.street" name="street" required placeholder="Số nhà, tên đường...">
            </div>
            <div class="form-actions">
              <button type="button" class="cancel-btn" (click)="closeForm()">Hủy</button>
              <button type="submit" class="submit-btn" [disabled]="isSaving()">
                {{ isSaving() ? 'Đang lưu...' : 'Lưu địa chỉ' }}
              </button>
            </div>
          </form>
        </div>
      </div>
      }
    </div>
  `,
  styles: [`
    .customer-addresses { max-width: 800px; margin: 0 auto; padding: 0 16px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .page-header h1 { font-size: 1.5rem; color: #1a1a1a; margin: 0; font-weight: 700; }
    
    .add-btn { 
      padding: 10px 20px; background: linear-gradient(135deg, #f97316, #ea580c); 
      color: white; border: none; border-radius: 8px; cursor: pointer; font-weight: 600;
      transition: all 0.2s;
    }
    .add-btn:hover { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(249,115,22,0.3); }
    
    .loading-state, .empty-state { 
      background: #fff; padding: 60px 40px; text-align: center; border-radius: 16px; color: #888;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04);
    }
    .spinner { width: 40px; height: 40px; border: 3px solid #e5e7eb; border-top-color: #f97316; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 16px; }
    @keyframes spin { to { transform: rotate(360deg); } }
    
    .empty-state .empty-icon { font-size: 48px; margin-bottom: 16px; }
    .empty-state h3 { font-size: 1.25rem; color: #1a1a1a; margin: 0 0 8px; }
    .empty-state p { color: #6b7280; margin: 0 0 24px; }
    
    .addresses-list { display: flex; flex-direction: column; gap: 16px; }
    
    .address-card {
      background: white; border-radius: 12px; padding: 20px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04); position: relative;
      border: 2px solid transparent; transition: all 0.2s;
    }
    .address-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); }
    .address-card.default { border-color: #f97316; }
    
    .default-badge {
      position: absolute; top: 12px; right: 12px;
      background: linear-gradient(135deg, #f97316, #ea580c); color: white;
      padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600;
    }
    
    .address-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
    .address-header h3 { margin: 0; font-size: 1rem; color: #1a1a1a; }
    .actions { display: flex; gap: 8px; }
    .actions button { background: none; border: none; cursor: pointer; font-size: 16px; padding: 4px; opacity: 0.6; transition: opacity 0.2s; }
    .actions button:hover { opacity: 1; }
    
    .address-body .contact { margin: 0 0 8px; font-size: 14px; color: #374151; }
    .address-body .full-address { margin: 0; font-size: 14px; color: #6b7280; }
    
    .set-default-btn {
      margin-top: 12px; padding: 8px 16px; background: transparent;
      border: 1px solid #e5e7eb; border-radius: 6px; cursor: pointer;
      font-size: 13px; color: #4b5563; transition: all 0.2s;
    }
    .set-default-btn:hover { border-color: #f97316; color: #f97316; }
    
    /* Modal */
    .modal-overlay {
      position: fixed; inset: 0; background: rgba(0,0,0,0.5);
      display: flex; align-items: center; justify-content: center; z-index: 1000;
    }
    .modal-content {
      background: white; border-radius: 16px; padding: 24px; width: 90%; max-width: 500px;
      max-height: 90vh; overflow-y: auto;
    }
    .modal-content h2 { margin: 0 0 20px; font-size: 1.25rem; color: #1a1a1a; }
    
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-size: 13px; color: #374151; margin-bottom: 6px; font-weight: 500; }
    .form-group input {
      width: 100%; padding: 12px; border: 1px solid #e5e7eb; border-radius: 8px;
      font-size: 14px; transition: border-color 0.2s;
    }
    .form-group input:focus { outline: none; border-color: #f97316; }
    
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 24px; }
    .cancel-btn { padding: 10px 20px; background: #f3f4f6; border: none; border-radius: 8px; cursor: pointer; color: #4b5563; }
    .submit-btn { 
      padding: 10px 20px; background: linear-gradient(135deg, #f97316, #ea580c);
      color: white; border: none; border-radius: 8px; cursor: pointer; font-weight: 600;
    }
    .submit-btn:disabled { opacity: 0.6; cursor: not-allowed; }
    
    @media (max-width: 600px) {
      .form-row { grid-template-columns: 1fr; }
    }
  `]
})
export class CustomerAddressesComponent implements OnInit {
  addresses = signal<Address[]>([]);
  isLoading = signal(true);
  isSaving = signal(false);
  showAddForm = false;
  editingAddress: Address | null = null;
  customerId: number | null = null;

  formData = {
    addressName: '',
    contactName: '',
    contactPhone: '',
    province: '',
    district: '',
    ward: '',
    street: ''
  };

  constructor(
    private api: ApiService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadAddresses();
  }

  loadAddresses(): void {
    const user = this.authService.getCurrentUser();
    if (!user) {
      this.isLoading.set(false);
      return;
    }

    // First get customerId
    this.api.get<any>(`/customer/user/${user.userId}`).subscribe({
      next: (res) => {
        this.customerId = res.customerId || res.data?.customerId;
        if (this.customerId) {
          this.fetchAddresses();
        } else {
          this.isLoading.set(false);
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  private fetchAddresses(): void {
    this.api.get<any>(`/customer/${this.customerId}/addresses`).subscribe({
      next: (res) => {
        const data = res.data || res || [];
        this.addresses.set(Array.isArray(data) ? data : []);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  buildFullAddress(addr: Address): string {
    return [addr.street, addr.ward, addr.district, addr.province].filter(Boolean).join(', ');
  }

  editAddress(addr: Address): void {
    this.editingAddress = addr;
    this.formData = {
      addressName: addr.addressName || '',
      contactName: addr.contactName || '',
      contactPhone: addr.contactPhone || '',
      province: addr.province || '',
      district: addr.district || '',
      ward: addr.ward || '',
      street: addr.street || ''
    };
    this.showAddForm = true;
  }

  closeForm(): void {
    this.showAddForm = false;
    this.editingAddress = null;
    this.formData = {
      addressName: '',
      contactName: '',
      contactPhone: '',
      province: '',
      district: '',
      ward: '',
      street: ''
    };
  }

  saveAddress(): void {
    if (!this.customerId) return;
    this.isSaving.set(true);

    const fullAddress = [this.formData.street, this.formData.ward, this.formData.district, this.formData.province].filter(Boolean).join(', ');
    const payload = { ...this.formData, fullAddress };

    if (this.editingAddress) {
      // Update
      this.api.put<any>(`/customer/addresses/${this.editingAddress.addressId}`, payload).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeForm();
          this.fetchAddresses();
        },
        error: () => this.isSaving.set(false)
      });
    } else {
      // Create
      this.api.post<any>(`/customer/${this.customerId}/addresses`, payload).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeForm();
          this.fetchAddresses();
        },
        error: () => this.isSaving.set(false)
      });
    }
  }

  deleteAddress(addressId: number): void {
    if (!confirm('Bạn có chắc muốn xóa địa chỉ này?')) return;

    this.api.delete<any>(`/customer/addresses/${addressId}`).subscribe({
      next: () => this.fetchAddresses(),
      error: (err) => console.error('Error deleting address:', err)
    });
  }

  setDefault(addressId: number): void {
    if (!this.customerId) return;

    this.api.patch<any>(`/customer/${this.customerId}/addresses/${addressId}/set-default`, {}).subscribe({
      next: () => this.fetchAddresses(),
      error: (err) => console.error('Error setting default:', err)
    });
  }
}
