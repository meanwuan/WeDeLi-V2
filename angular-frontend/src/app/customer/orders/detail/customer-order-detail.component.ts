import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';

interface OrderDetail {
  orderId: number;
  trackingCode: string;
  orderStatus: string;
  createdAt: string;
  updatedAt: string;
  // Sender
  senderName: string;
  senderPhone: string;
  senderAddress: string;
  // Receiver
  receiverName: string;
  receiverPhone: string;
  receiverAddress: string;
  receiverProvince: string;
  receiverDistrict: string;
  // Parcel
  parcelType: string;
  weightKg: number;
  declaredValue: number;
  specialInstructions: string;
  // Payment
  paymentMethod: string;
  codAmount: number;
  shippingFee: number;
  totalAmount: number;
  // Driver
  driverName?: string;
  driverPhone?: string;
}

interface TrackingEvent {
  timestamp: string;
  status: string;
  description: string;
  location?: string;
}

@Component({
  selector: 'app-customer-order-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="order-detail-page">
      <a class="back-link" routerLink="/customer/orders">← Quay lại danh sách</a>

      @if (isLoading()) {
      <div class="loading">Đang tải thông tin đơn hàng...</div>
      }

      @if (!isLoading() && order()) {
      <div class="order-detail">
        <!-- Header -->
        <div class="detail-header">
          <div class="header-left">
            <span class="tracking-label">Mã vận đơn</span>
            <h1 class="tracking-code">{{ order()!.trackingCode }}</h1>
          </div>
          <span class="status-badge" [class]="getStatusClass(order()!.orderStatus)">
            {{ getStatusLabel(order()!.orderStatus) }}
          </span>
        </div>

        <!-- Tracking Timeline -->
        <div class="section tracking-section">
          <h2>📍 Theo dõi đơn hàng</h2>
          <div class="timeline">
            @for (event of trackingEvents(); track event.timestamp) {
            <div class="timeline-item" [class.active]="$first">
              <div class="timeline-dot"></div>
              <div class="timeline-content">
                <span class="time">{{ formatDateTime(event.timestamp) }}</span>
                <span class="desc">{{ event.description }}</span>
                @if (event.location) {
                <span class="location">📍 {{ event.location }}</span>
                }
              </div>
            </div>
            }
          </div>
        </div>

        <!-- Sender & Receiver -->
        <div class="section-row">
          <div class="section">
            <h2>👤 Người gửi</h2>
            <div class="info-card">
              <div class="info-row">
                <span class="label">Họ tên</span>
                <span class="value">{{ order()!.senderName }}</span>
              </div>
              <div class="info-row">
                <span class="label">Số điện thoại</span>
                <span class="value">{{ order()!.senderPhone }}</span>
              </div>
              <div class="info-row">
                <span class="label">Địa chỉ</span>
                <span class="value">{{ order()!.senderAddress }}</span>
              </div>
            </div>
          </div>

          <div class="section">
            <h2>📦 Người nhận</h2>
            <div class="info-card">
              <div class="info-row">
                <span class="label">Họ tên</span>
                <span class="value">{{ order()!.receiverName }}</span>
              </div>
              <div class="info-row">
                <span class="label">Số điện thoại</span>
                <span class="value">{{ order()!.receiverPhone }}</span>
              </div>
              <div class="info-row">
                <span class="label">Địa chỉ</span>
                <span class="value">{{ order()!.receiverAddress }}, {{ order()!.receiverDistrict }}, {{ order()!.receiverProvince }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Parcel Info -->
        <div class="section">
          <h2>📋 Thông tin hàng hóa</h2>
          <div class="info-card">
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Loại hàng</span>
                <span class="value">{{ getParcelTypeLabel(order()!.parcelType) }}</span>
              </div>
              <div class="info-item">
                <span class="label">Khối lượng</span>
                <span class="value">{{ order()!.weightKg }} kg</span>
              </div>
              @if (order()!.declaredValue) {
              <div class="info-item">
                <span class="label">Giá trị khai báo</span>
                <span class="value">{{ formatCurrency(order()!.declaredValue) }}</span>
              </div>
              }
            </div>
            @if (order()!.specialInstructions) {
            <div class="info-row">
              <span class="label">Ghi chú</span>
              <span class="value">{{ order()!.specialInstructions }}</span>
            </div>
            }
          </div>
        </div>

        <!-- Payment Info -->
        <div class="section">
          <h2>💳 Thông tin thanh toán</h2>
          <div class="info-card payment-card">
            <div class="payment-row">
              <span>Phí vận chuyển</span>
              <span>{{ formatCurrency(order()!.shippingFee || 0) }}</span>
            </div>
            @if (order()!.codAmount > 0) {
            <div class="payment-row">
              <span>Tiền thu hộ (COD)</span>
              <span>{{ formatCurrency(order()!.codAmount) }}</span>
            </div>
            }
            <div class="payment-row total">
              <span>Tổng cộng</span>
              <span>{{ formatCurrency(order()!.shippingFee || 0) }}</span>
            </div>
            <div class="payment-method">
              <span class="label">Phương thức:</span>
              <span class="value">{{ getPaymentMethodLabel(order()!.paymentMethod) }}</span>
            </div>
          </div>
        </div>

        <!-- Driver Info (if assigned) -->
        @if (order()!.driverName) {
        <div class="section">
          <h2>🚚 Tài xế</h2>
          <div class="info-card driver-card">
            <div class="driver-info">
              <div class="driver-avatar">🧑‍✈️</div>
              <div class="driver-details">
                <span class="name">{{ order()!.driverName }}</span>
                <span class="phone">📞 {{ order()!.driverPhone }}</span>
              </div>
            </div>
            <button class="call-btn">Gọi tài xế</button>
          </div>
        </div>
        }

        <!-- Actions -->
        <div class="actions">
          @if (order()!.orderStatus === 'pending_pickup') {
          <button class="btn-danger" (click)="cancelOrder()">Hủy đơn hàng</button>
          }
          <button class="btn-secondary" (click)="copyTrackingCode()">📋 Sao chép mã</button>
        </div>
      </div>
      }

      @if (!isLoading() && !order()) {
      <div class="error-state">
        <h3>Không tìm thấy đơn hàng</h3>
        <p>Đơn hàng không tồn tại hoặc bạn không có quyền truy cập.</p>
        <a routerLink="/customer/orders" class="btn-primary">Quay lại danh sách</a>
      </div>
      }
    </div>
  `,
  styles: [`
    .order-detail-page { max-width: 800px; margin: 0 auto; padding: 0 16px 40px; }
    
    .back-link {
      display: inline-block; margin-bottom: 20px; color: #6b7280;
      text-decoration: none; font-size: 14px;
      &:hover { color: #f97316; }
    }
    
    .loading, .error-state {
      background: white; padding: 60px; text-align: center;
      border-radius: 16px; color: #6b7280;
    }
    .error-state h3 { color: #1a1a1a; margin: 0 0 8px; }
    .error-state p { margin: 0 0 24px; }
    
    .detail-header {
      display: flex; justify-content: space-between; align-items: flex-start;
      background: white; padding: 24px; border-radius: 16px;
      margin-bottom: 20px;
    }
    .tracking-label { font-size: 12px; color: #9ca3af; display: block; margin-bottom: 4px; }
    .tracking-code { font-size: 1.5rem; font-weight: 700; color: #1a1a1a; margin: 0; font-family: monospace; }
    
    .status-badge {
      padding: 8px 16px; border-radius: 20px; font-size: 13px; font-weight: 600;
    }
    .status-badge.pending { background: #fef3c7; color: #b45309; }
    .status-badge.confirmed { background: #dbeafe; color: #1d4ed8; }
    .status-badge.picked_up { background: #e0e7ff; color: #4338ca; }
    .status-badge.in_transit { background: #dbeafe; color: #2563eb; }
    .status-badge.out_for_delivery { background: #fae8ff; color: #a21caf; }
    .status-badge.delivered { background: #dcfce7; color: #16a34a; }
    .status-badge.cancelled { background: #fee2e2; color: #dc2626; }
    
    .section { margin-bottom: 20px; }
    .section h2 { font-size: 14px; font-weight: 600; color: #4b5563; margin: 0 0 12px; }
    
    .section-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
    @media (max-width: 600px) { .section-row { grid-template-columns: 1fr; } }
    
    .info-card {
      background: white; padding: 20px; border-radius: 12px;
    }
    .info-row {
      display: flex; justify-content: space-between; padding: 10px 0;
      border-bottom: 1px solid #f3f4f6;
      &:last-child { border-bottom: none; }
    }
    .info-row .label { font-size: 13px; color: #9ca3af; }
    .info-row .value { font-size: 14px; color: #1a1a1a; font-weight: 500; text-align: right; }
    
    .info-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 16px; margin-bottom: 16px; }
    .info-item { display: flex; flex-direction: column; gap: 4px; }
    .info-item .label { font-size: 12px; color: #9ca3af; }
    .info-item .value { font-size: 14px; color: #1a1a1a; font-weight: 500; }
    
    .tracking-section .timeline { padding-left: 8px; }
    .timeline-item {
      display: flex; gap: 16px; padding-bottom: 20px; position: relative;
      &::before {
        content: ''; position: absolute; left: 5px; top: 16px; bottom: 0;
        width: 2px; background: #e5e7eb;
      }
      &:last-child::before { display: none; }
    }
    .timeline-dot {
      width: 12px; height: 12px; border-radius: 50%; background: #e5e7eb;
      flex-shrink: 0; margin-top: 4px; z-index: 1;
    }
    .timeline-item.active .timeline-dot { background: #f97316; }
    .timeline-content { display: flex; flex-direction: column; gap: 4px; }
    .timeline-content .time { font-size: 12px; color: #9ca3af; }
    .timeline-content .desc { font-size: 14px; color: #1a1a1a; }
    .timeline-content .location { font-size: 12px; color: #6b7280; }
    
    .payment-card .payment-row {
      display: flex; justify-content: space-between; padding: 10px 0;
      font-size: 14px; color: #4b5563;
    }
    .payment-card .payment-row.total {
      border-top: 1px solid #e5e7eb; margin-top: 8px; padding-top: 16px;
      font-weight: 600; color: #1a1a1a; font-size: 16px;
    }
    .payment-method { margin-top: 16px; padding-top: 16px; border-top: 1px solid #f3f4f6; font-size: 13px; }
    .payment-method .label { color: #9ca3af; }
    .payment-method .value { color: #1a1a1a; font-weight: 500; margin-left: 8px; }
    
    .driver-card { display: flex; justify-content: space-between; align-items: center; }
    .driver-info { display: flex; gap: 12px; align-items: center; }
    .driver-avatar { font-size: 36px; }
    .driver-details { display: flex; flex-direction: column; }
    .driver-details .name { font-weight: 600; color: #1a1a1a; }
    .driver-details .phone { font-size: 13px; color: #6b7280; }
    .call-btn {
      padding: 10px 20px; background: #10b981; color: white;
      border: none; border-radius: 8px; cursor: pointer; font-weight: 600;
    }
    
    .actions {
      display: flex; gap: 12px; justify-content: flex-end; margin-top: 24px;
    }
    .btn-primary, .btn-secondary, .btn-danger {
      padding: 12px 24px; border-radius: 10px; font-weight: 600; cursor: pointer;
      border: none; font-size: 14px;
    }
    .btn-primary { background: #f97316; color: white; text-decoration: none; }
    .btn-secondary { background: white; border: 1px solid #e5e7eb; color: #4b5563; }
    .btn-danger { background: #fee2e2; color: #dc2626; }
  `]
})
export class CustomerOrderDetailComponent implements OnInit {
  order = signal<OrderDetail | null>(null);
  isLoading = signal(true);
  trackingEvents = signal<TrackingEvent[]>([]);

  constructor(
    private route: ActivatedRoute,
    private api: ApiService
  ) { }

  ngOnInit(): void {
    const orderId = this.route.snapshot.params['orderId'];
    if (orderId) {
      this.loadOrder(orderId);
    }
  }

  loadOrder(orderId: number): void {
    this.api.get<any>(`/orders/${orderId}`).subscribe({
      next: (res) => {
        const data = res.data || res;
        this.order.set(data);
        this.generateTrackingEvents(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  private generateTrackingEvents(order: OrderDetail): void {
    const events: TrackingEvent[] = [];
    const status = order.orderStatus?.toLowerCase() || '';

    // Generate events based on status
    events.push({
      timestamp: order.createdAt,
      status: 'created',
      description: 'Đơn hàng đã được tạo'
    });

    if (['picked_up', 'in_transit', 'out_for_delivery', 'delivered'].includes(status)) {
      events.push({
        timestamp: order.updatedAt || order.createdAt,
        status: 'picked_up',
        description: 'Đã lấy hàng từ người gửi'
      });
    }

    if (['in_transit', 'out_for_delivery', 'delivered'].includes(status)) {
      events.push({
        timestamp: order.updatedAt || order.createdAt,
        status: 'in_transit',
        description: 'Đang vận chuyển'
      });
    }

    if (['out_for_delivery', 'delivered'].includes(status)) {
      events.push({
        timestamp: order.updatedAt || order.createdAt,
        status: 'out_for_delivery',
        description: 'Đang giao hàng'
      });
    }

    if (['delivered'].includes(status)) {
      events.push({
        timestamp: order.updatedAt || order.createdAt,
        status: 'delivered',
        description: 'Giao hàng thành công'
      });
    }

    this.trackingEvents.set(events.reverse());
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

  getParcelTypeLabel(type: string): string {
    const labels: Record<string, string> = {
      'document': 'Tài liệu',
      'fragile': 'Dễ vỡ',
      'electronics': 'Điện tử',
      'food': 'Thực phẩm',
      'cold': 'Hàng lạnh',
      'other': 'Khác'
    };
    return labels[type?.toLowerCase()] || type || 'Khác';
  }

  getPaymentMethodLabel(method: string): string {
    const labels: Record<string, string> = {
      'cash': 'Tiền mặt',
      'bank_transfer': 'Chuyển khoản',
      'e_wallet': 'Ví điện tử'
    };
    return labels[method?.toLowerCase()] || method || 'Tiền mặt';
  }

  formatDateTime(dateStr: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    return date.toLocaleString('vi-VN', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit'
    });
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount || 0);
  }

  copyTrackingCode(): void {
    if (this.order()) {
      navigator.clipboard.writeText(this.order()!.trackingCode);
      alert('Đã sao chép mã vận đơn!');
    }
  }

  cancelOrder(): void {
    if (confirm('Bạn có chắc muốn hủy đơn hàng này?')) {
      this.api.patch<any>(`/orders/${this.order()!.orderId}/status`, { status: 'cancelled' }).subscribe({
        next: () => {
          this.loadOrder(this.order()!.orderId);
        },
        error: () => {
          alert('Không thể hủy đơn hàng. Vui lòng thử lại.');
        }
      });
    }
  }
}
