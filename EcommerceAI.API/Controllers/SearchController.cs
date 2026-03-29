using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet("suggestions")]
    public async Task<ActionResult<ApiResponseDto<List<string>>>> GetSuggestions([FromQuery] string keyword)
    {
        var suggestions = await _searchService.GetSearchSuggestionsAsync(keyword);
        return Ok(ApiResponseDto<List<string>>.SuccessResponse(suggestions));
    }
}
