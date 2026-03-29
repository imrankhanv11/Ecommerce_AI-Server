using Microsoft.EntityFrameworkCore;
using EcommerceAI.Models;
using EcommerceAI.Models.Entities;
using EcommerceAI.Repositories.Interfaces;

namespace EcommerceAI.Repositories.Implementations;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task<HashSet<string>> GetAllSlugsAsync()
    {
        var slugs = await _context.Categories
            .Select(c => c.Slug)
            .ToListAsync();
        return slugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
