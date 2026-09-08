using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelBookingPlatform.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("occupancy")]
    public ActionResult<OccupancyReportResponse> GetOccupancy()
    {
        return Ok(_reportService.GetOccupancy());
    }

    [HttpGet("best-reviewed-room-types")]
    public ActionResult<List<RoomTypeRatingResponse>> GetBestReviewedRoomTypes()
    {
        return Ok(_reportService.GetBestReviewedRoomTypes());
    }
}
