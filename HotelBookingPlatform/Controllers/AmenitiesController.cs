using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.Controllers;

[ApiController]
[Route("api/amenities")]
public class AmenitiesController : ControllerBase
{
    private readonly IAmenityService _amenityService;

    public AmenitiesController(IAmenityService amenityService)
    {
        _amenityService = amenityService;
    }

    [AllowAnonymous]
    [HttpGet]
    public ActionResult<List<AmenityResponse>> GetAll()
    {
        return Ok(_amenityService.GetAll());
    }

    [Authorize]
    [HttpPost]
    public ActionResult<AmenityResponse> Create([FromBody] CreateAmenityRequest request)
    {
        var amenity = _amenityService.Create(request);
        return CreatedAtAction(nameof(GetAll), new { }, amenity);
    }
}
