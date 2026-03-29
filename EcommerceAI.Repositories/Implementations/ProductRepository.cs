using Microsoft.EntityFrameworkCore;
using EcommerceAI.Models;
using EcommerceAI.Models.Entities;
using EcommerceAI.Contracts.DTOs.Chatbot;
using EcommerceAI.Repositories.Interfaces;
using System.Text.Json;

namespace EcommerceAI.Repositories.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(List<Product> Items, int TotalCount)> GetFilteredAsync(
        Guid? categoryId, decimal? minPrice, decimal? maxPrice,
        List<string>? tags, string? searchTerm, int page, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => 
                p.Name.Contains(searchTerm) || 
                p.TagsJson.Contains(searchTerm));
        }

        // Apply pagination
        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Product>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => ids.Contains(p.Id) && p.IsActive)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByCategoryAsync(Guid categoryId)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .ToListAsync();
    }

    public async Task<List<Product>> GetTopSellingAsync(int limit)
    {
        // Top-selling = products with the most order items
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.OrderItems.Count)
            .ThenByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Product>> GetTopByStockAsync(int limit)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Stock)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByKeywordsAsync(string keyword, int limit)
    {
        var searchTerm = $"%{keyword}%";
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Stock > 0 && (
                EF.Functions.Like(p.Name, searchTerm) ||
                EF.Functions.Like(p.TagsJson, searchTerm) ||
                EF.Functions.Like(p.Category.Name, searchTerm)
            ))
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Product>> GetByCategoryAndKeywordsAsync(string categoryName, string keyword, int limit)
    {
        var searchTerm = $"%{keyword}%";
        return await _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive && p.Stock > 0 && p.Category.Name == categoryName && (
                EF.Functions.Like(p.Name, searchTerm) ||
                EF.Functions.Like(p.TagsJson, searchTerm)
            ))
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Product>> FilterByChatbotAsync(ChatbotFilterDto filters)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Category))
            query = query.Where(p => p.Category.Slug == filters.Category);

        if (filters.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filters.MinPrice.Value);

        if (filters.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filters.MaxPrice.Value);

        var results = await query
            .OrderByDescending(p => p.CreatedAt)
            .Take(50) // Limit chatbot results
            .ToListAsync();

        // Tag filter in-memory (JSON column)
        if (filters.Tags.Any())
        {
            results = results.Where(p =>
                filters.Tags.Any(tag => p.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            ).ToList();
        }

        return results;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
