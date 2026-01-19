import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../auth/services/auth.service';

interface CustomerProfile {
  customerId: number;
  userId: number;
  fullName: string;
  email: string;
  phone: string;
  companyName?: string;
  address?: string;
  isRegular: boolean;
  createdAt: string;
}

@Component({
  selector: 'app-customer-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="customer-settings">
      <h1>Cài đặt tài khoản</h1>
      
      <!-- Loading State -->
      @if (isLoading()) {
      <div class="loading-state">
        <div class="spinner"></div>
        <p>Đang tải thông tin...</p>
      </div>
      }
      
      <!-- Profile Form -->
      @if (!isLoading() && customer()) {
      <div class="settings-sections">
        
        <!-- Customer Profile Section -->
        <section class="settings-section">
          <div class="section-header">
            <h2>👤 Thông tin khách hàng</h2>
            @if (customer()!.isRegular) {
            <span class="regular-badge">⭐ Khách hàng thân thiết</span>
            }
          </div>
          
          <div class="form-group">
            <label>Họ và tên</label>
            <input type="text" [(ngModel)]="formData.fullName" name="fullName" required>
          </div>
          
          <div class="form-row">
            <div class="form-group">
              <label>Email</label>
              <input type="email" [(ngModel)]="formData.email" name="email" required>
            </div>
            <div class="form-group">
              <label>Số điện thoại</label>
              <input type="text" [(ngModel)]="formData.phone" name="phone" required>
            </div>
          </div>
          
          <div class="form-group">
            <label>Tên công ty (nếu có)</label>
            <input type="text" [(ngModel)]="formData.companyName" name="companyName">
          </div>
          
          <div class="form-group">
            <label>Địa chỉ mặc định</label>
            <textarea [(ngModel)]="formData.address" name="address" rows="2"></textarea>
          </div>
          
          <div class="form-row">
            <div class="form-group readonly">
              <label>Mã khách hàng</label>
              <input type="text" [value]="'KH' + customer()!.customerId" readonly>
            </div>
            <div class="form-group readonly">
              <label>Ngày đăng ký</label>
              <input type="text" [value]="formatDate(customer()!.createdAt)" readonly>
            </div>
          </div>

          <!-- Save with password confirmation -->
          <div class="save-section">
            <div class="form-group">
              <label>Nhập mật khẩu để xác nhận thay đổi *</label>
              <input type="password" [(ngModel)]="confirmPassword" name="confirmPassword" 
                     placeholder="Mật khẩu của bạn">
            </div>
            
            @if (message()) {
            <div class="message" [class.success]="messageType() === 'success'" [class.error]="messageType() === 'error'">
              {{ message() }}
            </div>
            }
            
            <div class="form-actions">
              <button type="button" class="cancel-btn" (click)="resetForm()">Hủy thay đổi</button>
              <button type="button" class="submit-btn" (click)="saveProfile()" [disabled]="isSaving() || !confirmPassword">
                {{ isSaving() ? 'Đang lưu...' : 'Lưu thay đổi' }}
              </button>
            </div>
          </div>
        </section>
        
        <!-- Password Change Section -->
        <section class="settings-section">
          <h2>🔐 Đổi mật khẩu</h2>
          <div class="form-group">
            <label>Mật khẩu hiện tại</label>
            <input type="password" [(ngModel)]="passwordData.currentPassword" name="currentPassword" required>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Mật khẩu mới</label>
              <input type="password" [(ngModel)]="passwordData.newPassword" name="newPassword" required minlength="6">
            </div>
            <div class="form-group">
              <label>Xác nhận mật khẩu mới</label>
              <input type="password" [(ngModel)]="passwordData.confirmPassword" name="confirmNewPassword" required>
            </div>
          </div>
          
          @if (passwordMessage()) {
          <div class="message" [class.success]="passwordMessageType() === 'success'" [class.error]="passwordMessageType() === 'error'">
            {{ passwordMessage() }}
          </div>
          }
          
          <div class="form-actions">
            <button type="button" class="submit-btn" (click)="changePassword()" [disabled]="isChangingPassword()">
              {{ isChangingPassword() ? 'Đang đổi...' : 'Đổi mật khẩu' }}
            </button>
          </div>
        </section>
        
        <!-- Stats Section -->
        <section class="settings-section stats-section">
          <h2>📊 Thống kê đơn hàng</h2>
          <div class="stats-grid">
            <div class="stat-item">
              <span class="stat-value">{{ stats().totalOrders }}</span>
              <span class="stat-label">Tổng đơn</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ stats().delivered }}</span>
              <span class="stat-label">Đã giao</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ stats().inProgress }}</span>
              <span class="stat-label">Đang giao</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">{{ formatCurrency(stats().totalSpent) }}</span>
              <span class="stat-label">Tổng chi tiêu</span>
            </div>
          </div>
        </section>
      </div>
      }

      @if (!isLoading() && !customer()) {
      <div class="error-state">
        <p>Không thể tải thông tin khách hàng. Vui lòng thử lại sau.</p>
        <button class="submit-btn" (click)="loadProfile()">Thử lại</button>
      </div>
      }
    </div>
  `,
  styles: [`
    .customer-settings { max-width: 700px; margin: 0 auto; padding: 0 16px; }
    h1 { font-size: 1.5rem; color: #1a1a1a; margin: 0 0 24px; font-weight: 700; }
    
    .loading-state, .error-state { 
      background: #fff; padding: 60px 40px; text-align: center; border-radius: 16px; color: #888;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04);
    }
    .spinner { width: 40px; height: 40px; border: 3px solid #e5e7eb; border-top-color: #f97316; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 16px; }
    @keyframes spin { to { transform: rotate(360deg); } }
    
    .settings-sections { display: flex; flex-direction: column; gap: 24px; }
    
    .settings-section {
      background: white; border-radius: 16px; padding: 24px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04);
    }
    .section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; }
    .settings-section h2 { font-size: 1.1rem; color: #1a1a1a; margin: 0 0 20px; font-weight: 600; }
    .section-header h2 { margin-bottom: 0; }
    .regular-badge { background: linear-gradient(135deg, #fbbf24, #f59e0b); color: white; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600; }
    
    .form-group { margin-bottom: 16px; }
    .form-group label { display: block; font-size: 13px; color: #374151; margin-bottom: 6px; font-weight: 500; }
    .form-group input, .form-group textarea {
      width: 100%; padding: 12px; border: 1px solid #e5e7eb; border-radius: 8px;
      font-size: 14px; transition: border-color 0.2s; background: white;
      font-family: inherit; resize: vertical;
    }
    .form-group input:focus, .form-group textarea:focus { outline: none; border-color: #f97316; }
    .form-group.readonly input { background: #f9fafb; color: #6b7280; cursor: not-allowed; }
    
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    
    .save-section { 
      margin-top: 24px; padding-top: 24px; border-top: 1px solid #e5e7eb;
    }
    
    .message {
      padding: 12px 16px; border-radius: 8px; font-size: 14px; margin-bottom: 16px;
    }
    .message.success { background: #dcfce7; color: #16a34a; }
    .message.error { background: #fee2e2; color: #dc2626; }
    
    .form-actions { display: flex; justify-content: flex-end; gap: 12px; margin-top: 8px; }
    .cancel-btn { 
      padding: 10px 20px; background: #f3f4f6; border: none; border-radius: 8px; 
      cursor: pointer; color: #4b5563; transition: all 0.2s;
    }
    .cancel-btn:hover { background: #e5e7eb; }
    .submit-btn { 
      padding: 10px 20px; background: linear-gradient(135deg, #f97316, #ea580c);
      color: white; border: none; border-radius: 8px; cursor: pointer; font-weight: 600;
      transition: all 0.2s;
    }
    .submit-btn:hover { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(249,115,22,0.3); }
    .submit-btn:disabled { opacity: 0.6; cursor: not-allowed; transform: none; }
    
    .stats-section .stats-grid {
      display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px;
    }
    .stat-item {
      text-align: center; padding: 16px; background: #f9fafb; border-radius: 12px;
    }
    .stat-value { display: block; font-size: 1.5rem; font-weight: 700; color: #1a1a1a; }
    .stat-label { display: block; font-size: 12px; color: #6b7280; margin-top: 4px; }
    
    @media (max-width: 600px) {
      .form-row { grid-template-columns: 1fr; }
      .stats-section .stats-grid { grid-template-columns: repeat(2, 1fr); }
    }
  `]
})
export class CustomerSettingsComponent implements OnInit {
  customer = signal<CustomerProfile | null>(null);
  isLoading = signal(true);
  isSaving = signal(false);
  isChangingPassword = signal(false);
  message = signal('');
  messageType = signal<'success' | 'error'>('success');
  passwordMessage = signal('');
  passwordMessageType = signal<'success' | 'error'>('success');
  stats = signal({ totalOrders: 0, delivered: 0, inProgress: 0, totalSpent: 0 });
  confirmPassword = '';

  formData = {
    fullName: '',
    email: '',
    phone: '',
    companyName: '',
    address: ''
  };

  passwordData = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  constructor(
    private api: ApiService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    const user = this.authService.getCurrentUser();
    if (!user) {
      this.isLoading.set(false);
      return;
    }

    // Get customer profile by userId
    this.api.get<any>(`/customer/user/${user.userId}`).subscribe({
      next: (res) => {
        const data = res.data || res;
        this.customer.set(data);
        this.formData = {
          fullName: data.fullName || user.fullName || '',
          email: data.email || user.email || '',
          phone: data.phone || user.phone || '',
          companyName: data.companyName || '',
          address: data.address || ''
        };
        this.isLoading.set(false);
        // Load stats
        if (data.customerId) {
          this.loadStats(data.customerId);
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  loadStats(customerId: number): void {
    this.api.get<any>(`/customer/${customerId}/statistics`).subscribe({
      next: (res) => {
        const data = res.data || res;
        this.stats.set({
          totalOrders: data.totalOrders || 0,
          delivered: data.deliveredOrders || data.delivered || 0,
          inProgress: data.inProgressOrders || data.inProgress || 0,
          totalSpent: data.totalSpent || data.totalAmount || 0
        });
      },
      error: () => { }
    });
  }

  resetForm(): void {
    const c = this.customer();
    if (c) {
      this.formData = {
        fullName: c.fullName || '',
        email: c.email || '',
        phone: c.phone || '',
        companyName: c.companyName || '',
        address: c.address || ''
      };
    }
    this.confirmPassword = '';
    this.message.set('');
  }

  saveProfile(): void {
    if (!this.customer()) return;

    if (!this.confirmPassword) {
      this.message.set('Vui lòng nhập mật khẩu để xác nhận');
      this.messageType.set('error');
      return;
    }

    this.isSaving.set(true);
    this.message.set('');

    // Update customer profile with password confirmation
    const payload = {
      ...this.formData,
      password: this.confirmPassword
    };

    this.api.put<any>(`/customer/${this.customer()!.customerId}`, payload).subscribe({
      next: (res) => {
        this.isSaving.set(false);
        this.message.set('Cập nhật thành công!');
        this.messageType.set('success');
        this.confirmPassword = '';
        const updatedCustomer = res.data || res;
        if (updatedCustomer) {
          this.customer.set({
            ...this.customer()!,
            ...updatedCustomer
          });
        }
      },
      error: (err) => {
        this.isSaving.set(false);
        this.message.set(err.error?.message || 'Mật khẩu không đúng hoặc có lỗi xảy ra.');
        this.messageType.set('error');
      }
    });
  }

  changePassword(): void {
    this.passwordMessage.set('');

    if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
      this.passwordMessage.set('Mật khẩu xác nhận không khớp');
      this.passwordMessageType.set('error');
      return;
    }

    if (this.passwordData.newPassword.length < 6) {
      this.passwordMessage.set('Mật khẩu mới phải có ít nhất 6 ký tự');
      this.passwordMessageType.set('error');
      return;
    }

    this.isChangingPassword.set(true);

    this.api.patch<any>('/users/me/password', {
      currentPassword: this.passwordData.currentPassword,
      newPassword: this.passwordData.newPassword
    }).subscribe({
      next: () => {
        this.isChangingPassword.set(false);
        this.passwordMessage.set('Đổi mật khẩu thành công!');
        this.passwordMessageType.set('success');
        this.passwordData = { currentPassword: '', newPassword: '', confirmPassword: '' };
      },
      error: (err) => {
        this.isChangingPassword.set(false);
        this.passwordMessage.set(err.error?.message || 'Mật khẩu hiện tại không đúng');
        this.passwordMessageType.set('error');
      }
    });
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return 'N/A';
    const date = new Date(dateStr);
    return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount || 0);
  }
}
