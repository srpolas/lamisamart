namespace LamisaMart.Ordering.Application.DTOs;

public record OrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal ShippingFee { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<VendorSubOrderDto> VendorSubOrders { get; init; } = new();
}

public record VendorSubOrderDto
{
    public Guid Id { get; init; }
    public string SubOrderNumber { get; init; } = string.Empty;
    public Guid VendorId { get; init; }
    public string VendorName { get; init; } = string.Empty;
    public decimal SubTotal { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? CourierName { get; init; }
    public string? TrackingNumber { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}

public record OrderItemDto
{
    public string ProductName { get; init; } = string.Empty;
    public string VariantName { get; init; } = string.Empty;
    public string SKU { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
