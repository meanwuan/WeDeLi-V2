using AutoMapper;
using wedeli.Models.Domain;
using wedeli.Models.Domain.Data;
using wedeli.Models.DTO.Auth;
using wedeli.Models.DTO.COD;
using wedeli.Models.DTO.Common;
using wedeli.Models.DTO.Company;
using wedeli.Models.DTO.Complaint;
using wedeli.Models.DTO.Customer;
using wedeli.Models.DTO.Driver;
using wedeli.Models.DTO.Order;
using wedeli.Models.DTO.Partnership;
using wedeli.Models.DTO.Payment;
using wedeli.Models.DTO.Rating;
using wedeli.Models.DTO.Report;
using wedeli.Models.DTO.Route;
using wedeli.Models.DTO.Trip;
using wedeli.Models.DTO.Vehicle;

namespace wedeli.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ============================================
            // AUTH & USER MAPPINGS
            // ============================================

            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src =>
                    src.TransportCompany != null ? src.TransportCompany.CompanyName : null));

            CreateMap<RegisterRequestDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true));

            CreateMap<UpdateProfileRequestDto, User>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<User, LoginResponseDto>()
                .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshToken, opt => opt.Ignore())
                .ForMember(dest => dest.TokenExpiration, opt => opt.Ignore())
                .ForMember(dest => dest.RefreshTokenExpiration, opt => opt.Ignore())
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src =>
                    src.TransportCompany != null ? (int?)src.TransportCompany.CompanyId : null))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src =>
                    src.TransportCompany != null ? src.TransportCompany.CompanyName : null));

            CreateMap<User, RegisterResponseDto>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.RoleName))
                .ForMember(dest => dest.Message, opt => opt.Ignore());

            CreateMap<Role, RoleDto>();

            // ============================================
            // ORDER MAPPINGS
            // ============================================

            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.TrackingCode, opt => opt.Ignore())
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => "pending_pickup"))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "unpaid"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateOrderDto, Order>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Order, OrderResponseDto>()
                // NOTE: Customer is in Platform DB - populated by service
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerPhone, opt => opt.Ignore())
                .ForMember(dest => dest.IsRegularCustomer, opt => opt.Ignore())
                .ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Route != null ? src.Route.RouteName : null))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.LicensePlate : null))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.CompanyUser.FullName : null))
                .ForMember(dest => dest.DriverPhone, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.CompanyUser.Phone : null))
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.OrderPhotos))
                .ForMember(dest => dest.StatusHistory, opt => opt.MapFrom(src => src.OrderStatusHistories));

            CreateMap<Order, OrderDetailDto>()
                .IncludeBase<Order, OrderResponseDto>()
                // NOTE: Company is in Platform DB - populated by service
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.Vehicle != null ? (int?)src.Vehicle.CompanyId : null))
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore());

            CreateMap<Order, OrderListItemDto>()
                // NOTE: Customer is in Platform DB - populated by service
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver != null ? src.Driver.CompanyUser.FullName : null))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src => src.Vehicle != null ? src.Vehicle.LicensePlate : null));

            CreateMap<OrderStatusHistory, OrderStatusDto>()
                .ForMember(dest => dest.UpdatedByName, opt => opt.MapFrom(src => src.UpdatedBy != null ? src.UpdatedBy : null));

            CreateMap<UpdateOrderStatusDto, OrderStatusHistory>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<OrderPhoto, OrderPhotoDto>()
                .ForMember(dest => dest.UploadedByName, opt => opt.MapFrom(src => src.UploadedBy != null ? src.UploadedBy : null));

            CreateMap<UploadOrderPhotoDto, OrderPhoto>()
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<AssignOrderDto, Order>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ============================================
            // CUSTOMER MAPPINGS
            // ============================================

            CreateMap<CreateCustomerDto, Customer>()
                .ForMember(dest => dest.IsRegular, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.TotalOrders, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalRevenue, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.PaymentPrivilege, opt => opt.MapFrom(src => "prepay"))
                .ForMember(dest => dest.CreditLimit, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateCustomerDto, Customer>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Customer, CustomerResponseDto>();

            CreateMap<Customer, CustomerDetailDto>()
                .ForMember(dest => dest.PendingOrders, opt => opt.MapFrom(src =>
                    src.Orders.Count(o => o.OrderStatus == "pending_pickup" || o.OrderStatus == "in_transit")))
                .ForMember(dest => dest.CompletedOrders, opt => opt.MapFrom(src =>
                    src.Orders.Count(o => o.OrderStatus == "delivered")))
                .ForMember(dest => dest.CancelledOrders, opt => opt.MapFrom(src =>
                    src.Orders.Count(o => o.OrderStatus == "cancelled")))
                .ForMember(dest => dest.AverageOrderValue, opt => opt.MapFrom(src =>
                    src.Orders.Any() ? src.Orders.Average(o => o.ShippingFee) : 0))
                .ForMember(dest => dest.LastOrderDate, opt => opt.MapFrom(src =>
                    src.Orders.Any() ? src.Orders.Max(o => o.CreatedAt) : (DateTime?)null))
                .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.CustomerAddresses))
                .ForMember(dest => dest.RecentOrders, opt => opt.Ignore());

            CreateMap<Customer, CustomerListItemDto>()
                .ForMember(dest => dest.LastOrderDate, opt => opt.MapFrom(src =>
                    src.Orders.Any() ? src.Orders.Max(o => o.CreatedAt) : (DateTime?)null));

            CreateMap<CustomerAddress, CustomerAddressDto>();

            CreateMap<CreateCustomerAddressDto, CustomerAddress>()
                .ForMember(dest => dest.UsageCount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateCustomerAddressDto, CustomerAddress>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<UpdateCustomerClassificationDto, Customer>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ============================================
            // DRIVER MAPPINGS
            // ============================================

            CreateMap<CreateDriverDto, Driver>()
                .ForMember(dest => dest.TotalTrips, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.SuccessRate, opt => opt.MapFrom(src => 100))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 5.0m))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateDriverDto, Driver>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Driver, DriverResponseDto>()
                // NOTE: Company is in Platform DB - populated by service
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.CompanyUser != null ? src.CompanyUser.FullName : null))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.CompanyUser != null ? src.CompanyUser.Phone : null))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.CompanyUser != null ? src.CompanyUser.Email : null))
                .ForMember(dest => dest.LicenseExpiry, opt => opt.MapFrom(src => src.LicenseExpiry.HasValue
                    ? src.LicenseExpiry.Value.ToDateTime(TimeOnly.MinValue)
                    : (DateTime?)null));

            CreateMap<Driver, DriverDetailDto>()
                .IncludeBase<Driver, DriverResponseDto>();

            // ============================================
            // VEHICLE MAPPINGS
            // ============================================

            CreateMap<CreateVehicleDto, Vehicle>()
                .ForMember(dest => dest.CurrentWeightKg, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CapacityPercentage, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.AllowOverload, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CurrentStatus, opt => opt.MapFrom(src => "available"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateVehicleDto, Vehicle>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Vehicle, VehicleResponseDto>()
                // NOTE: Company is in Platform DB - populated by service
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore());

            CreateMap<Vehicle, VehicleDetailDto>()
                .IncludeBase<Vehicle, VehicleResponseDto>();

            CreateMap<UpdateVehicleStatusDto, Vehicle>()
                .ForMember(dest => dest.CurrentStatus, opt => opt.MapFrom(src => src.Status));

            // ============================================
            // ROUTE & TRIP MAPPINGS
            // ============================================

            CreateMap<CreateRouteDto, Models.Domain.Route>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateRouteDto, Models.Domain.Route>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Models.Domain.Route, RouteResponseDto>()
                // NOTE: Company is in Platform DB - populated by service
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore());

            CreateMap<CreateTripDto, Models.Domain.Trip>()
                .ForMember(dest => dest.TripStatus, opt => opt.MapFrom(src => "scheduled"))
                .ForMember(dest => dest.TotalOrders, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.TotalWeightKg, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateTripDto, Models.Domain.Trip>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Models.Domain.Trip, TripResponseDto>()
                .ForMember(dest => dest.RouteName, opt => opt.MapFrom(src => src.Route.RouteName))
                .ForMember(dest => dest.VehicleLicensePlate, opt => opt.MapFrom(src => src.Vehicle.LicensePlate))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src => src.Driver.CompanyUser.FullName));

            CreateMap<Models.Domain.Trip, TripDetailDto>()
                .IncludeBase<Models.Domain.Trip, TripResponseDto>()
                .ForMember(dest => dest.OriginProvince, opt => opt.MapFrom(src => src.Route.OriginProvince))
                .ForMember(dest => dest.DestinationProvince, opt => opt.MapFrom(src => src.Route.DestinationProvince))
                .ForMember(dest => dest.DistanceKm, opt => opt.MapFrom(src => src.Route.DistanceKm));

            // ============================================
            // COD & PAYMENT MAPPINGS
            // ============================================

            CreateMap<CollectCodDto, CodTransaction>()
                .ForMember(dest => dest.CollectedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.CollectionStatus, opt => opt.MapFrom(src => "collected"))
                .ForMember(dest => dest.OverallStatus, opt => opt.MapFrom(src => "collected"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<CodTransaction, CodTransactionResponseDto>()
                .ForMember(dest => dest.TrackingCode, opt => opt.MapFrom(src => src.Order.TrackingCode))
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src =>
                    src.CollectedByDriver != null ? src.CollectedByDriver : null));

            CreateMap<CreatePaymentDto, Payment>()
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<Payment, PaymentResponseDto>()
                .ForMember(dest => dest.TrackingCode, opt => opt.MapFrom(src => src.Order.TrackingCode))
                // NOTE: Customer is in Platform DB - populated by service
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore());

            CreateMap<GenerateInvoiceDto, PeriodicInvoice>()
                .ForMember(dest => dest.InvoiceStatus, opt => opt.MapFrom(src => "pending"))
                .ForMember(dest => dest.PaidAmount, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<PeriodicInvoice, PeriodicInvoiceResponseDto>()
                // NOTE: Customer/Company are in Platform DB - populated by service
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore());

            // ============================================
            // COMPANY & PARTNERSHIP MAPPINGS
            // ============================================

            CreateMap<CreateCompanyDto, TransportCompany>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => 5.0m))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateCompanyDto, TransportCompany>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<TransportCompany, CompanyResponseDto>();

            CreateMap<TransportCompany, CompanyDetailDto>();

            CreateMap<CreatePartnershipDto, CompanyPartnership>()
                .ForMember(dest => dest.TotalTransferredOrders, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdatePartnershipDto, CompanyPartnership>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CompanyPartnership, PartnershipResponseDto>()
                // NOTE: Company/PartnerCompany are in Platform DB - populated by service
                .ForMember(dest => dest.CompanyName, opt => opt.Ignore())
                .ForMember(dest => dest.PartnerCompanyName, opt => opt.Ignore());

            CreateMap<TransferOrderDto, OrderTransfer>()
                .ForMember(dest => dest.TransferStatus, opt => opt.MapFrom(src => "pending"))
                .ForMember(dest => dest.TransferredAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<OrderTransfer, OrderTransferResponseDto>()
                .ForMember(dest => dest.TrackingCode, opt => opt.MapFrom(src => src.Order.TrackingCode))
                // NOTE: FromCompany/ToCompany are in Platform DB - populated by service
                .ForMember(dest => dest.FromCompanyName, opt => opt.Ignore())
                .ForMember(dest => dest.ToCompanyName, opt => opt.Ignore());

            // ============================================
            // RATING & COMPLAINT MAPPINGS
            // ============================================

            CreateMap<CreateRatingDto, Models.Domain.Rating>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateRatingDto, Models.Domain.Rating>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Models.Domain.Rating, RatingResponseDto>()
                .ForMember(dest => dest.TrackingCode, opt => opt.MapFrom(src => src.Order.TrackingCode))
                // NOTE: Customer is in Platform DB - populated by service
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
                .ForMember(dest => dest.DriverName, opt => opt.MapFrom(src =>
                    src.Driver != null ? src.Driver.CompanyUser.FullName : null));

            CreateMap<CreateComplaintDto, Complaint>()
                .ForMember(dest => dest.ComplaintStatus, opt => opt.MapFrom(src => "pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateComplaintDto, Complaint>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Complaint, ComplaintResponseDto>()
                .ForMember(dest => dest.TrackingCode, opt => opt.MapFrom(src => src.Order.TrackingCode))
                // NOTE: Customer is in Platform DB - populated by service
                .ForMember(dest => dest.CustomerName, opt => opt.Ignore())
                .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src =>
                    src.ResolvedBy != null ? src.ResolvedBy : null));

            // ============================================
            // COMMON MAPPINGS
            // ============================================

            CreateMap<CreateWarehouseStaffDto, WarehouseStaff>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<WarehouseStaff, WarehouseStaffDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.CompanyUser != null ? src.CompanyUser.FullName : null))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.CompanyUser != null ? src.CompanyUser.Phone : null));

            CreateMap<Notification, NotificationDto>();

            CreateMap<DailyActivityLog, ActivityLogDto>()
                .ForMember(dest => dest.ChangedByName, opt => opt.MapFrom(src =>
                    src.ChangedBy != null ? src.ChangedBy : null));

            CreateMap<DailySummary, DailySummaryDto>();
            
            // ============================================
            // COMPANY CUSTOMER MAPPINGS
            // ============================================
            CreateMap<CompanyCustomer, wedeli.Models.DTO.CompanyCustomerResponseDto>();
        }
    }
}