using HotelBookingPlatform.Common;
using HotelBookingPlatform.Data;
using HotelBookingPlatform.DTOs;
using HotelBookingPlatform.Models;
using HotelBookingPlatform.Services.Interfaces;

namespace HotelBookingPlatform.Services;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;

    public ReviewService(AppDbContext db)
    {
        _db = db;
    }

    public ReviewResponse Create(CreateReviewRequest request)
    {
        var booking = _db.Bookings.FirstOrDefault(b => b.Id == request.BookingId)
            ?? throw new NotFoundException($"Booking with id {request.BookingId} was not found.");

        var guest = _db.Guests.First(g => g.Id == booking.GuestId);
        if (guest.Email.ToLower() != request.GuestEmail.Trim().ToLower())
        {
            throw new BadRequestException("The email provided does not match the guest on this booking.");
        }

        if (booking.Status != BookingStatus.CheckedOut)
        {
            throw new ConflictException("You can only review a stay after it has been checked out.");
        }

        if (_db.Reviews.Any(r => r.BookingId == booking.Id))
        {
            throw new ConflictException("A review has already been submitted for this booking.");
        }

        var review = new Review
        {
            BookingId = booking.Id,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim()
        };
        _db.Reviews.Add(review);
        _db.SaveChanges();

        var room = _db.Rooms.First(r => r.Id == booking.RoomId);
        var roomType = _db.RoomTypes.First(rt => rt.Id == room.RoomTypeId);

        return new ReviewResponse
        {
            Id = review.Id,
            BookingId = review.BookingId,
            RoomTypeId = roomType.Id,
            RoomTypeName = roomType.Name,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAtUtc = review.CreatedAtUtc
        };
    }
}
