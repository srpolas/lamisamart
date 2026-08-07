using Microsoft.EntityFrameworkCore;
using LamisaMart.Ordering.Domain.Entities;

namespace LamisaMart.Ordering.Application.Common.Interfaces;

public interface IOrderingDbContext
{
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<VendorSubOrder> VendorSubOrders { get; }
    DbSet<OrderItem> OrderItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
