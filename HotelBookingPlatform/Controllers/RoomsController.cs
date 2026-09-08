using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [AllowAnonymous]
    [HttpGet("available")]
    public ActionResult<List<RoomResponse>> GetAvailable(
        [FromQuery] DateOnly checkIn,
        [FromQuery] DateOnly checkOut,
        [FromQuery] int? roomTypeId)
    {
        return Ok(_roomService.GetAvailableRooms(checkIn, checkOut, roomTypeId));
    }

    [Authorize]
    [HttpGet]
    public ActionResult<List<RoomResponse>> GetAll()
    {
        return Ok(_roomService.GetAll());
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public ActionResult<RoomResponse> GetById(int id)
    {
        return Ok(_roomService.GetById(id));
    }

    [Authorize]
    [HttpPost]
    public ActionResult<RoomResponse> Create([FromBody] CreateRoomRequest request)
    {
        var room = _roomService.Create(request);
        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }

    [Authorize]
    [HttpPatch("{id:int}/deactivate")]
    public ActionResult<RoomResponse> Deactivate(int id)
    {
        return Ok(_roomService.SetActive(id, false));
    }

    [Authorize]
    [HttpPatch("{id:int}/activate")]
    public ActionResult<RoomResponse> Activate(int id)
    {
        return Ok(_roomService.SetActive(id, true));
    }
}
