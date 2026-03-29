using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EcommerceAI.Contracts.DTOs.Chatbot;
using EcommerceAI.Contracts.DTOs.Product;
using EcommerceAI.Repositories.Interfaces;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.Services.Implementations;

public class ChatbotService : IChatbotService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ChatbotService> _logger;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;

    private const int MaxRetries = 2;

    public ChatbotService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ChatbotService> logger,
        ICategoryRepository categoryRepository,
        ICartRepository cartRepository,
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _categoryRepository = categoryRepository;
        _cartRepository = cartRepository;
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        
        // Fix 2: Read timeout from config
        _httpClient.Timeout = TimeSpan.FromSeconds(
            _configuration.GetValue<int>("AI:TimeoutSeconds", 30)
        );
    }

    public async Task<ChatbotResponseDto> ProcessQueryAsync(Guid userId, ChatbotQueryRequestDto request)
    {
        string? rawResponse = null;
        var products = new List<ProductResponseDto>();
        var userMessage = (request.Query ?? "").ToLower();

        // ─── Step 1: Improved Keyword Extraction (Fix 5) ────────────────────────
        var keywords = new[] {
            "show me", "find me", "i need", "i want",
            "looking for", "recommend", "suggest", "buy", "get me"
        };

        string? searchTerm = null;
        foreach (var kw in keywords)
        {
            if (userMessage.Contains(kw))
            {
                var afterKeyword = userMessage
                    .Substring(userMessage.IndexOf(kw) + kw.Length)
                    .Trim();

                afterKeyword = afterKeyword
                    .Replace("some ", "")
                    .Replace("a ", "")
                    .Replace("an ", "")
                    .Replace("the ", "")
                    .Trim();

                searchTerm = string.Join(" ",
                    afterKeyword.Split(' ')
                        .Take(3)
                        .Where(w => w.Length > 1));
                break;
            }
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var found = await _productRepository.GetByKeywordsAsync(searchTerm, 4);
            products = found.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Stock = p.Stock,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.Name ?? "General"
            }).ToList();
        }

        // ─── Step 2: Slim Context for 1b Model (Fix 4) ─────────────────────────
        var user = await _userRepository.GetByIdAsync(userId);
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        var ordersResult = await _orderRepository.GetByUserIdAsync(userId, 1, 3);
        
        var cartItems = cart?.Items.Select(i => new {
            i.Product.Name,
            i.Product.Price,
            i.Quantity,
            Subtotal = i.Product.Price * i.Quantity
        }).ToList() ?? new();
        
        var cartTotal = cartItems.Sum(i => i.Subtotal);
        var recentOrders = ordersResult.Items.ToList();

        var cartSummary = cartItems.Count == 0
            ? "Cart is empty"
            : string.Join(", ", cartItems
                .Take(5)
                .Select(i => $"{i.Name} x{i.Quantity} ₹{i.Subtotal}"))
              + $" | Total: ₹{cartTotal}";

        var orderSummary = recentOrders.Count == 0
            ? "No recent orders"
            : string.Join(" | ", recentOrders
                .Take(3)
                .Select(o => $"Order {o.Id.ToString().Substring(0, 8)}: {o.Status} ₹{o.TotalAmount}"));

        var context = $"Customer: {user?.FirstName} | Cart: {cartSummary} | Orders: {orderSummary}";

        // ─── Step 3: Improved Retry With Smart Fallback (Fix 7) ─────────────────
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                // Fix 1: Renamed method
                rawResponse = await CallOllamaAsync(userMessage, context, request.History ?? new());
                if (!string.IsNullOrWhiteSpace(rawResponse)) break;
                _logger.LogWarning("Empty response attempt {Attempt}", attempt);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Ollama timeout attempt {Attempt}", attempt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Ollama error attempt {Attempt}", attempt);
            }

            if (attempt < MaxRetries)
                await Task.Delay(500);
        }

        var fallback = cartItems.Count > 0
            ? $"You have {cartItems.Count} items totalling ₹{cartTotal}. How can I help?"
            : "I am having trouble right now. Please try again shortly.";

        return new ChatbotResponseDto
        {
            AIResponse = rawResponse ?? fallback,
            Products = products,
            TotalCount = products.Count
        };
    }

    // Fix 1: Renamed to CallOllamaAsync
    private async Task<string> CallOllamaAsync(string userMessage, string context, List<ChatHistoryItem> history)
    {
        var model = _configuration["AI:OllamaModel"] ?? "llama3.2:1b";
        var baseUrl = _configuration["AI:OllamaBaseUrl"] ?? "http://localhost:11434";
        
        // Fix 3: Shorten system prompt
        var systemPrompt = @"You are Aura, a friendly shopping assistant for Aura Store.
You have the customer's REAL cart and order data below.
Always use this data to answer accurately. Never make up data.

Rules:
- Max 3 lines per response
- Always use ₹ for prices
- Only answer shopping questions
- If unrelated question say: I can only help with shopping questions!
- If cart empty say: Your cart is empty! Start browsing our collections.";

        // Fix 6: Build prompt with history
        var historyText = history
            .TakeLast(4)
            .Select(h => $"{h.Role}: {h.Content}")
            .Aggregate("", (a, b) => a + "\n" + b);

        var fullPrompt = context +
                         "\n\nConversation:\n" + historyText +
                         "\n\nCustomer: " + userMessage +
                         "\nAura:";

        var requestBody = new
        {
            model = model,
            system = systemPrompt,
            prompt = fullPrompt,
            stream = false
        };

        var url = $"{baseUrl.TrimEnd('/')}/api/generate";
        var response = await _httpClient.PostAsJsonAsync(url, requestBody);
        
        if (!response.IsSuccessStatusCode) return string.Empty;

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("response").GetString()!;
    }
}
