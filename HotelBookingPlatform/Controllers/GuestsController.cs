using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.Controllers;

[ApiController]
[Route("api/guests")]
[Authorize]
public class GuestsController : ControllerBase
{
    private readonly IGuestService _guestService;

    public GuestsController(IGuestService guestService)
    {
        _guestService = guestService;
    }

    [HttpGet]
    public ActionResult<List<GuestResponse>> GetAll()
    {
        return Ok(_guestService.GetAll());
    }

    [HttpGet("{id:int}")]
    public ActionResult<GuestResponse> GetById(int id)
    {
        return Ok(_guestService.GetById(id));
    }

    [HttpGet("{id:int}/bookings")]
    public ActionResult<List<BookingResponse>> GetBookingHistory(int id)
    {
        return Ok(_guestService.GetBookingHistory(id));
    }
}
