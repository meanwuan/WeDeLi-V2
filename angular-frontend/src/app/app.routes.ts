import { Routes } from '@angular/router';
import { authGuard, noAuthGuard, adminGuard, customerGuard } from './auth/guards/auth.guard';

export const routes: Routes = [
    // Auth routes (public)
    {
        path: 'auth',
        canActivate: [noAuthGuard],
        loadChildren: () => import('./auth/auth.routes').then(m => m.AUTH_ROUTES)
    },
    // Public tracking page (no auth required)
    {
        path: 'tracking',
        loadComponent: () => import('./tracking/tracking.component').then(m => m.TrackingComponent)
    },
    // Customer Portal routes (protected - Customer & Driver only)
    {
        path: 'customer',
        canActivate: [customerGuard],
        loadComponent: () => import('./customer/layouts/customer-layout.component').then(m => m.CustomerLayoutComponent),
        children: [
            {
                path: '',
                loadComponent: () => import('./customer/dashboard/customer-dashboard.component').then(m => m.CustomerDashboardComponent)
            },
            {
                path: 'orders',
                loadComponent: () => import('./customer/orders/customer-orders.component').then(m => m.CustomerOrdersComponent)
            },
            {
                path: 'orders/create',
                loadComponent: () => import('./customer/orders/create/customer-order-create.component').then(m => m.CustomerOrderCreateComponent)
            },
            {
                path: 'orders/:orderId',
                loadComponent: () => import('./customer/orders/detail/customer-order-detail.component').then(m => m.CustomerOrderDetailComponent)
            },
            {
                path: 'addresses',
                loadComponent: () => import('./customer/addresses/customer-addresses.component').then(m => m.CustomerAddressesComponent)
            },
            {
                path: 'settings',
                loadComponent: () => import('./customer/settings/customer-settings.component').then(m => m.CustomerSettingsComponent)
            }
        ]
    },
    // Admin app routes (protected - Admin roles only)
    {
        path: '',
        canActivate: [adminGuard],
        loadComponent: () => import('./layouts/main-layout/main-layout.component').then(m => m.MainLayoutComponent),
        children: [
            {
                path: '',
                redirectTo: 'dashboard',
                pathMatch: 'full'
            },
            {
                path: 'dashboard',
                loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent)
            },
            {
                path: 'orders',
                loadComponent: () => import('./orders/orders.component').then(m => m.OrdersComponent)
            },
            {
                path: 'vehicles',
                loadComponent: () => import('./vehicles/vehicles.component').then(m => m.VehiclesComponent)
            },
            {
                path: 'drivers',
                loadComponent: () => import('./drivers/drivers.component').then(m => m.DriversComponent)
            },
            {
                path: 'drivers/create',
                loadComponent: () => import('./drivers/create/driver-create.component').then(m => m.DriverCreateComponent)
            },
            {
                path: 'reports',
                loadComponent: () => import('./reports/reports.component').then(m => m.ReportsComponent)
            },
            {
                path: 'settings',
                loadComponent: () => import('./settings/settings.component').then(m => m.SettingsComponent)
            },
            {
                path: 'routes',
                loadComponent: () => import('./routes/routes.component').then(m => m.RoutesComponent)
            },
            {
                path: 'staff',
                loadComponent: () => import('./staff/staff.component').then(m => m.StaffComponent)
            },
            {
                path: 'staff/create',
                loadComponent: () => import('./staff/create/staff-create.component').then(m => m.StaffCreateComponent)
            },
            {
                path: 'company-customers',
                loadComponent: () => import('./company-customers/company-customers.component').then(m => m.CompanyCustomersComponent)
            },
            {
                path: 'orders/create',
                loadComponent: () => import('./orders/create/order-create.component').then(m => m.OrderCreateComponent)
            },
            {
                path: 'orders/:orderId',
                loadComponent: () => import('./orders/detail/order-detail.component').then(m => m.OrderDetailComponent)
            },
            {
                path: 'orders/:orderId/edit',
                loadComponent: () => import('./orders/edit/order-edit.component').then(m => m.OrderEditComponent)
            }
        ]
    },
    // Fallback
    {
        path: '**',
        redirectTo: ''
    }
];
