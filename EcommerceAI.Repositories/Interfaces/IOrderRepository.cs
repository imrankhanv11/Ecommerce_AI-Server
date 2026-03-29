using EcommerceAI.Models.Entities;

namespace EcommerceAI.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<(List<Order> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int page, int pageSize);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
}
