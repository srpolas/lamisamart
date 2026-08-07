using MediatR;
using Microsoft.EntityFrameworkCore;
using LamisaMart.Ordering.Application.Common.Interfaces;
using LamisaMart.Ordering.Application.DTOs;
using LamisaMart.Ordering.Domain.Entities;
using LamisaMart.Shared.Domain.ValueObjects;

namespace LamisaMart.Ordering.Application.Orders.Commands;

public record CheckoutCommand(
    Guid CustomerId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    Address ShippingAddress,
    string PaymentMethod = "SSLCommerz"
) : IRequest<OrderDto>;

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, OrderDto>
{
    private readonly IOrderingDbContext _context;

    public CheckoutCommandHandler(IOrderingDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        // 1. Fetch Customer Cart
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);

        if (cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty or not found.");
        }

        // 2. Generate Unique Order Number
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ShippingAddress = request.ShippingAddress,
            PaymentMethod = request.PaymentMethod,
            Status = OrderStatus.Pending
        };

        // 3. Multi-Vendor Order Splitting by VendorId
        var itemsByVendor = cart.Items.GroupBy(item => item.VendorId);
        decimal grandTotal = 0m;
        int subOrderIndex = 1;

        foreach (var vendorGroup in itemsByVendor)
        {
            var vendorId = vendorGroup.Key;
            var subOrderNumber = $"{orderNumber}-V{subOrderIndex++}";
            decimal subTotalAmount = 0m;

            var subOrder = new VendorSubOrder
            {
                VendorId = vendorId,
                SubOrderNumber = subOrderNumber,
                Status = SubOrderStatus.Pending
            };

            foreach (var cartItem in vendorGroup)
            {
                var itemTotal = cartItem.UnitPrice * cartItem.Quantity;
                subTotalAmount += itemTotal;

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    ProductVariantId = cartItem.ProductVariantId,
                    ProductName = cartItem.ProductName,
                    VariantName = cartItem.VariantName,
                    SKU = cartItem.SKU,
                    ProductImageUrl = cartItem.ProductImageUrl,
                    UnitPrice = new Money(cartItem.UnitPrice),
                    Quantity = cartItem.Quantity
                };

                subOrder.Items.Add(orderItem);
            }

            subOrder.SubTotal = new Money(subTotalAmount);
            // Default 10% commission calculation
            var commission = subTotalAmount * 0.10m;
            subOrder.CommissionAmount = new Money(commission);
            subOrder.VendorPayoutAmount = new Money(subTotalAmount - commission);

            order.VendorSubOrders.Add(subOrder);
            grandTotal += subTotalAmount;
        }

        order.TotalAmount = new Money(grandTotal);

        // 4. Persist Order and Clear Cart
        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Return Order DTO
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            TotalAmount = order.TotalAmount.Amount,
            ShippingFee = order.ShippingFee.Amount,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            VendorSubOrders = order.VendorSubOrders.Select(so => new VendorSubOrderDto
            {
                Id = so.Id,
                SubOrderNumber = so.SubOrderNumber,
                VendorId = so.VendorId,
                VendorName = so.VendorName,
                SubTotal = so.SubTotal.Amount,
                Status = so.Status.ToString(),
                Items = so.Items.Select(i => new OrderItemDto
                {
                    ProductName = i.ProductName,
                    VariantName = i.VariantName,
                    SKU = i.SKU,
                    UnitPrice = i.UnitPrice.Amount,
                    Quantity = i.Quantity,
                    LineTotal = i.LineTotal.Amount
                }).ToList()
            }).ToList()
        };
    }
}
