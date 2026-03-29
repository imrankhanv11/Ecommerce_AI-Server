namespace EcommerceAI.Contracts.DTOs.Product;

public class ProductFilterDto
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string>? Tags { get; set; }
    public string? SearchTerm { get; set; }
}
