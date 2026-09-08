using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [AllowAnonymous]
    [HttpPost]
    public ActionResult<BookingResponse> Create([FromBody] CreateBookingRequest request)
    {
        var booking = _bookingService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public ActionResult<BookingResponse> GetById(int id)
    {
        return Ok(_bookingService.GetById(id));
    }

    [Authorize]
    [HttpPost("{id:int}/confirm")]
    public ActionResult<BookingResponse> Confirm(int id)
    {
        return Ok(_bookingService.Confirm(id));
    }

    [Authorize]
    [HttpPost("{id:int}/cancel")]
    public ActionResult<BookingResponse> Cancel(int id)
    {
        return Ok(_bookingService.Cancel(id));
    }

    [Authorize]
    [HttpPost("{id:int}/checkin")]
    public ActionResult<BookingResponse> CheckIn(int id)
    {
        return Ok(_bookingService.CheckIn(id));
    }

    [Authorize]
    [HttpPost("{id:int}/checkout")]
    public ActionResult<BookingResponse> CheckOut(int id)
    {
        return Ok(_bookingService.CheckOut(id));
    }
}
