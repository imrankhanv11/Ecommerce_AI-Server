using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.Product;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponseDto<PagedResultDto<ProductResponseDto>>>> GetAll(
        [FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetFilteredAsync(filter);
        return Ok(ApiResponseDto<PagedResultDto<ProductResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<ProductResponseDto>>> GetById(Guid id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound(ApiResponseDto<ProductResponseDto>.FailResponse("Product not found"));
        return Ok(ApiResponseDto<ProductResponseDto>.SuccessResponse(product));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponseDto<ProductResponseDto>>> Create(
        [FromBody] ProductRequestDto request)
    {
        var product = await _productService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            ApiResponseDto<ProductResponseDto>.SuccessResponse(product, "Product created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<ProductResponseDto>>> Update(
        Guid id, [FromBody] ProductRequestDto request)
    {
        var product = await _productService.UpdateAsync(id, request);
        if (product == null)
            return NotFound(ApiResponseDto<ProductResponseDto>.FailResponse("Product not found"));
        return Ok(ApiResponseDto<ProductResponseDto>.SuccessResponse(product, "Product updated successfully"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _productService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
