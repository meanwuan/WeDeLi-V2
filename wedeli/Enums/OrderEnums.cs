// File: Enums/OrderEnums.cs

namespace wedeli.Enums;

/// <summary>
/// Trạng thái đơn hàng - mapping với DB ENUM
/// </summary>
public enum OrderStatus
{
    PendingPickup,    // pending_pickup
    PickedUp,         // picked_up
    InTransit,        // in_transit
    OutForDelivery,   // out_for_delivery
    Delivered,        // delivered
    Returned,         // returned
    Cancelled         // cancelled
}

/// <summary>
/// Trạng thái thanh toán
/// </summary>
public enum PaymentStatus
{
    Unpaid,   // unpaid
    Paid,     // paid
    Pending   // pending
}

/// <summary>
/// Phương thức thanh toán
/// </summary>
public enum PaymentMethod
{
    Cash,           // cash
    BankTransfer,   // bank_transfer
    EWallet,        // e_wallet
    Periodic        // periodic
}

/// <summary>
/// Loại hàng hóa
/// </summary>
public enum ParcelType
{
    Fragile,      // fragile
    Electronics,  // electronics
    Food,         // food
    Cold,         // cold
    Document,     // document
    Other         // other
}

/// <summary>
/// Loại ảnh đơn hàng
/// </summary>
public enum PhotoType
{
    BeforeDelivery,   // before_delivery
    AfterDelivery,    // after_delivery
    ParcelCondition,  // parcel_condition
    Signature,        // signature
    DamageProof       // damage_proof
}

/// <summary>
/// Trạng thái xe
/// </summary>
public enum VehicleStatus
{
    Available,    // available
    InTransit,    // in_transit
    Maintenance,  // maintenance
    Inactive,     // inactive
    Overloaded    // overloaded
}

/// <summary>
/// Loại xe
/// </summary>
public enum VehicleType
{
    Truck,      // truck
    Van,        // van
    Motorbike   // motorbike
}

/// <summary>
/// Trạng thái chuyến đi
/// </summary>
public enum TripStatus
{
    Scheduled,   // scheduled
    InProgress,  // in_progress
    Completed,   // completed
    Cancelled    // cancelled
}

/// <summary>
/// Loại khiếu nại
/// </summary>
public enum ComplaintType
{
    Lost,         // lost
    Damaged,      // damaged
    Late,         // late
    WrongAddress, // wrong_address
    Other         // other
}

/// <summary>
/// Trạng thái khiếu nại
/// </summary>
public enum ComplaintStatus
{
    Pending,       // pending
    Investigating, // investigating
    Resolved,      // resolved
    Rejected       // rejected
}

/// <summary>
/// Trạng thái COD
/// </summary>
public enum CodStatus
{
    PendingCollection,    // pending_collection
    Collected,            // collected
    SubmittedToCompany,   // submitted_to_company
    Completed,            // completed
    Failed                // failed
}