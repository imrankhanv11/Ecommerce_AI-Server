namespace EcommerceAI.Services.Interfaces;

public interface ISearchService
{
    Task<List<string>> GetSearchSuggestionsAsync(string keyword);
}
