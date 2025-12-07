using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewDto>> GetAllReviewsAsync();
        Task<List<ReviewDto>> GetReviewsByBoatIdAsync(int boatId);
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<ReviewDto> CreateReviewAsync(CreateReviewDto input);
        Task<bool> DeleteReviewAsync(int id);
        Task<double> GetAverageRatingAsync(int boatId);
        Task<List<ReviewDto>> GetRecentReviewsAsync(int limit = 10);
    }
}
