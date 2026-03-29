using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Category;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<List<CategoryResponseDto>>>> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return Ok(ApiResponseDto<List<CategoryResponseDto>>.SuccessResponse(result));
    }
}
