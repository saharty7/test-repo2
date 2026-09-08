using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.Controllers;

[ApiController]
[Route("api/roomtypes")]
public class RoomTypesController : ControllerBase
{
    private readonly IRoomTypeService _roomTypeService;

    public RoomTypesController(IRoomTypeService roomTypeService)
    {
        _roomTypeService = roomTypeService;
    }

    [AllowAnonymous]
    [HttpGet]
    public ActionResult<List<RoomTypeResponse>> GetAll()
    {
        return Ok(_roomTypeService.GetAll());
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ActionResult<RoomTypeResponse> GetById(int id)
    {
        return Ok(_roomTypeService.GetById(id));
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/reviews")]
    public ActionResult<List<ReviewResponse>> GetReviews(int id)
    {
        return Ok(_roomTypeService.GetReviews(id));
    }

    [Authorize]
    [HttpPost]
    public ActionResult<RoomTypeResponse> Create([FromBody] CreateRoomTypeRequest request)
    {
        var roomType = _roomTypeService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = roomType.Id }, roomType);
    }
}
