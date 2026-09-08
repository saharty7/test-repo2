using HotelBookingPlatform.DTOs;

namespace HotelBookingPlatform.Services.Interfaces;

public interface IReviewService
{
    ReviewResponse Create(CreateReviewRequest request);
}
