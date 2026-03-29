using Microsoft.AspNetCore.Mvc;
using EcommerceAI.Contracts.Common;
using EcommerceAI.Contracts.DTOs.User;
using EcommerceAI.Services.Interfaces;

namespace EcommerceAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> Register(
        [FromBody] RegisterRequestDto request)
    {
        var user = await _userService.RegisterAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            ApiResponseDto<UserResponseDto>.SuccessResponse(user, "User registered successfully"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponseDto<LoginResponseDto>>> Login(
        [FromBody] LoginRequestDto request)
    {
        var result = await _userService.LoginAsync(request);
        return Ok(ApiResponseDto<LoginResponseDto>.SuccessResponse(result, "Login successful"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponseDto<UserResponseDto>.FailResponse("User not found"));
        return Ok(ApiResponseDto<UserResponseDto>.SuccessResponse(user));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponseDto<UserResponseDto>>> UpdateProfile(
        Guid id, [FromBody] UpdateUserProfileDto request)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound(ApiResponseDto<UserResponseDto>.FailResponse("User not found"));

        var updated = await _userService.UpdateProfileAsync(id, request.FirstName, request.LastName);
        return Ok(ApiResponseDto<UserResponseDto>.SuccessResponse(updated!, "Profile updated successfully"));
    }
}
