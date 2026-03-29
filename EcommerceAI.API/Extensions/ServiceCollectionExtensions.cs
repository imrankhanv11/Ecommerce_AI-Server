using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Repositories.Implementations;
using EcommerceAI.Services.Interfaces;
using EcommerceAI.Services.Implementations;

namespace EcommerceAI.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // ─── Repositories ────────────────────────────────
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IUserActivityRepository, UserActivityRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // ─── Services ────────────────────────────────────
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserActivityService, UserActivityService>();
        services.AddScoped<ISearchService, SearchService>();
        // NOTE: IChatbotService is registered via AddHttpClient in Program.cs

        return services;
    }
}
