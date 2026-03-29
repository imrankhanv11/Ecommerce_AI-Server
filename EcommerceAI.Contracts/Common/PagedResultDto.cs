namespace EcommerceAI.Contracts.Common;

public class PagedResultDto<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Limit);
    public bool HasMore => Page < TotalPages;
}
