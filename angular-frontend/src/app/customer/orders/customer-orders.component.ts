import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../auth/services/auth.service';

interface Order {
  orderId: number;
  trackingCode: string;
  senderName: string;
  receiverName: string;
  receiverAddress: string;
  receiverProvince: string;
  orderStatus: string;
  createdAt: string;
  codAmount: number;
  shippingFee: number;
}

@Component({
  selector: 'app-customer-orders',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="customer-orders">
      <header class="page-header">
        <h1>Đơn hàng của bạn</h1>
        <button class="create-btn" routerLink="/customer/orders/create">+ Tạo đơn mới</button>
      </header>

      <!-- Filters -->
      <div class="filters-bar">
        <div class="search-box">
          <span class="icon">🔍</span>
          <input type="text" placeholder="Tìm theo mã đơn, người nhận..." 
                 (input)="onSearch($event)">
        </div>
        <div class="filter-tabs">
          <button [class.active]="activeFilter() === 'all'" (click)="setFilter('all')">Tất cả</button>
          <button [class.active]="activeFilter() === 'pending_pickup'" (click)="setFilter('pending_pickup')">Chờ xử lý</button>
          <button [class.active]="activeFilter() === 'in_transit'" (click)="setFilter('in_transit')">Đang giao</button>
          <button [class.active]="activeFilter() === 'picked_up'" (click)="setFilter('picked_up')">Đã lấy hàng</button>
        </div>
      </div>
      
      <!-- Loading State -->
      @if (isLoading()) {
      <div class="orders-placeholder">
        <p>Đang tải danh sách đơn hàng...</p>
      </div>
      }
      
      <!-- Empty State -->
      @if (!isLoading() && orders().length === 0) {
      <div class="empty-state">
        <div class="empty-icon">📦</div>
        <h3>Chưa có đơn hàng nào</h3>
        <p>Tạo đơn hàng đầu tiên của bạn ngay bây giờ!</p>
        <button class="create-btn" routerLink="/customer/orders/create">+ Tạo đơn mới</button>
      </div>
      }
      
      <!-- Orders List -->
      @if (!isLoading() && orders().length > 0) {
      <div class="orders-list">
        @for (order of filteredOrders(); track order.orderId) {
        <div class="order-card">
          <div class="order-header">
            <div class="order-code">
              <span class="label">Mã vận đơn</span>
              <span class="code">{{ order.trackingCode }}</span>
            </div>
            <span class="status-badge" [class]="getStatusClass(order.orderStatus)">
              {{ getStatusLabel(order.orderStatus) }}
            </span>
          </div>
          
          <div class="order-route">
            <div class="route-point sender">
              <span class="dot"></span>
              <div class="info">
                <span class="type">Người gửi</span>
                <span class="name">{{ order.senderName }}</span>
              </div>
            </div>
            <div class="route-line"></div>
            <div class="route-point receiver">
              <span class="dot"></span>
              <div class="info">
                <span class="type">Người nhận</span>
                <span class="name">{{ order.receiverName }}</span>
                <span class="address">{{ order.receiverProvince || order.receiverAddress }}</span>
              </div>
            </div>
          </div>
          
          <div class="order-footer">
            <div class="order-meta">
              <span class="date">📅 {{ formatDate(order.createdAt) }}</span>
              @if (order.codAmount > 0) {
              <span class="cod">💰 COD: {{ formatCurrency(order.codAmount) }}</span>
              }
            </div>
            <button class="detail-btn" [routerLink]="['/customer/orders', order.orderId]">
              Xem chi tiết →
            </button>
          </div>
        </div>
        }
      </div>
      }
    </div>
  `,
  styles: [`
    .customer-orders { max-width: 900px; margin: 0 auto; padding: 0 16px; }
    
    .page-header { 
      display: flex; justify-content: space-between; align-items: center; 
      margin-bottom: 24px; 
    }
    .page-header h1 { font-size: 1.5rem; color: #1a1a1a; margin: 0; font-weight: 700; }
    
    .create-btn { 
      padding: 12px 24px; background: linear-gradient(135deg, #f97316, #ea580c); 
      color: white; border: none; border-radius: 10px; cursor: pointer; 
      font-weight: 600; transition: all 0.2s;
    }
    .create-btn:hover { transform: translateY(-1px); box-shadow: 0 4px 12px rgba(249,115,22,0.3); }
    
    .filters-bar {
      display: flex; justify-content: space-between; align-items: center;
      margin-bottom: 20px; gap: 16px; flex-wrap: wrap;
    }
    .search-box {
      display: flex; align-items: center; gap: 10px;
      background: white; border: 1px solid #e5e7eb; border-radius: 10px;
      padding: 10px 16px; flex: 1; max-width: 300px;
    }
    .search-box input { border: none; outline: none; flex: 1; font-size: 14px; }
    .search-box .icon { color: #9ca3af; }
    
    .filter-tabs { display: flex; gap: 8px; }
    .filter-tabs button {
      padding: 8px 16px; border: 1px solid #e5e7eb; background: white;
      border-radius: 8px; cursor: pointer; font-size: 13px; color: #6b7280;
      transition: all 0.2s;
    }
    .filter-tabs button:hover { border-color: #f97316; color: #f97316; }
    .filter-tabs button.active { 
      background: #fff7ed; border-color: #f97316; color: #ea580c; font-weight: 600; 
    }
    
    .orders-placeholder, .empty-state { 
      background: #fff; padding: 60px 40px; text-align: center; 
      border-radius: 16px; color: #888; 
    }
    .empty-state .empty-icon { font-size: 48px; margin-bottom: 16px; }
    .empty-state h3 { font-size: 1.25rem; color: #1a1a1a; margin: 0 0 8px; }
    .empty-state p { color: #6b7280; margin: 0 0 24px; }
    
    .orders-list { display: flex; flex-direction: column; gap: 16px; }
    
    .order-card {
      background: white; border-radius: 16px; padding: 20px;
      box-shadow: 0 2px 8px rgba(0,0,0,0.04);
      transition: all 0.2s;
    }
    .order-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); }
    
    .order-header {
      display: flex; justify-content: space-between; align-items: flex-start;
      margin-bottom: 20px;
    }
    .order-code .label { display: block; font-size: 12px; color: #9ca3af; margin-bottom: 4px; }
    .order-code .code { font-size: 15px; font-weight: 700; color: #1a1a1a; font-family: monospace; }
    
    .status-badge {
      padding: 6px 12px; border-radius: 20px; font-size: 12px; font-weight: 600;
    }
    .status-badge.pending { background: #fef3c7; color: #b45309; }
    .status-badge.confirmed { background: #dbeafe; color: #1d4ed8; }
    .status-badge.picked_up { background: #e0e7ff; color: #4338ca; }
    .status-badge.in_transit { background: #dbeafe; color: #2563eb; }
    .status-badge.out_for_delivery { background: #fae8ff; color: #a21caf; }
    .status-badge.delivered { background: #dcfce7; color: #16a34a; }
    .status-badge.cancelled { background: #fee2e2; color: #dc2626; }
    
    .order-route {
      display: flex; align-items: flex-start; gap: 12px; margin-bottom: 20px;
    }
    .route-point { display: flex; align-items: flex-start; gap: 10px; flex: 1; }
    .route-point .dot { 
      width: 12px; height: 12px; border-radius: 50%; margin-top: 4px; flex-shrink: 0;
    }
    .route-point.sender .dot { background: #f97316; }
    .route-point.receiver .dot { background: #10b981; }
    .route-point .info { display: flex; flex-direction: column; gap: 2px; }
    .route-point .type { font-size: 11px; color: #9ca3af; text-transform: uppercase; }
    .route-point .name { font-size: 14px; font-weight: 600; color: #1a1a1a; }
    .route-point .address { font-size: 13px; color: #6b7280; }
    
    .route-line { 
      flex: 0 0 40px; height: 2px; background: #e5e7eb; 
      align-self: center; margin-top: 6px;
    }
    
    .order-footer {
      display: flex; justify-content: space-between; align-items: center;
      padding-top: 16px; border-top: 1px solid #f3f4f6;
    }
    .order-meta { display: flex; gap: 16px; font-size: 13px; color: #6b7280; }
    
    .detail-btn {
      padding: 8px 16px; background: transparent; border: 1px solid #e5e7eb;
      border-radius: 8px; cursor: pointer; font-size: 13px; color: #4b5563;
      transition: all 0.2s;
    }
    .detail-btn:hover { background: #f97316; border-color: #f97316; color: white; }
    
    @media (max-width: 600px) {
      .filters-bar { flex-direction: column; align-items: stretch; }
      .search-box { max-width: none; }
      .filter-tabs { overflow-x: auto; padding-bottom: 8px; }
      .order-route { flex-direction: column; gap: 16px; }
      .route-line { display: none; }
    }
  `]
})
export class CustomerOrdersComponent implements OnInit {
  orders = signal<Order[]>([]);
  isLoading = signal(true);
  activeFilter = signal('all');
  searchTerm = signal('');
  customerId: number | null = null;

  constructor(
    private api: ApiService,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
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
          this.fetchOrders();
        } else {
          this.isLoading.set(false);
        }
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  private fetchOrders(): void {
    this.api.get<any>(`/orders/customer/${this.customerId}?pageNumber=1&pageSize=50`).subscribe({
      next: (res) => {
        const data = res.data?.items || res.items || res.data || [];
        this.orders.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  filteredOrders(): Order[] {
    let result = this.orders();

    // Filter by status
    if (this.activeFilter() !== 'all') {
      result = result.filter(o => o.orderStatus.toLowerCase() === this.activeFilter());
    }

    // Filter by search
    const term = this.searchTerm().toLowerCase();
    if (term) {
      result = result.filter(o =>
        o.trackingCode?.toLowerCase().includes(term) ||
        o.receiverName?.toLowerCase().includes(term) ||
        o.senderName?.toLowerCase().includes(term)
      );
    }

    return result;
  }

  setFilter(filter: string): void {
    this.activeFilter.set(filter);
  }

  onSearch(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
  }

  getStatusLabel(status: string): string {
    const labels: Record<string, string> = {
      'pending_pickup': 'Chờ lấy hàng',
      'picked_up': 'Đã lấy hàng',
      'in_transit': 'Đang vận chuyển',
      'out_for_delivery': 'Đang giao',
      'delivered': 'Đã giao',
      'returned': 'Hoàn trả',
      'cancelled': 'Đã hủy'
    };
    return labels[status?.toLowerCase()] || status || 'Không xác định';
  }

  getStatusClass(status: string): string {
    return status.toLowerCase().replace(/ /g, '_');
  }

  formatDate(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
  }
}
