using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _db;
        private readonly ReviewRepository _repo;

        public ReviewService(ApplicationDbContext db, ReviewRepository repo)
        {
            _db = db;
            _repo = repo;
        }

        private ReviewDto Map(Review r)
        {
            return new ReviewDto
            {
                Id = r.Id,
                BoatId = r.BoatId,
                UserId = r.UserId.ToString(),
                UserName = r.UserName,
                UserAvatar = r.UserAvatar,
                Rating = r.Rating,
                Comment = r.Comment,
                Date = r.CreatedAt.ToString("yyyy-MM-dd")
            };
        }

        public async Task<List<ReviewDto>> GetAllReviewsAsync()
        {
            var list = await _db.Reviews.Include(r => r.User).Include(r => r.Boat).AsNoTracking().OrderByDescending(r => r.CreatedAt).ToListAsync();
            return list.Select(Map).ToList();
        }

        public async Task<List<ReviewDto>> GetReviewsByBoatIdAsync(int boatId)
        {
            var list = await _db.Reviews.Where(r => r.BoatId == boatId).Include(r => r.User).AsNoTracking().OrderByDescending(r => r.CreatedAt).ToListAsync();
            return list.Select(Map).ToList();
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return null;
            return Map(r);
        }

        public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto input)
        {
            var boat = await _db.Boats.FirstOrDefaultAsync(b => b.Id == input.BoatId);
            if (boat == null) throw new KeyNotFoundException("Boat not found");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == input.UserId);
            if (user == null) throw new KeyNotFoundException("User not found");

            var review = new Review
            {
                BoatId = input.BoatId,
                BookingId = input.BookingId,
                UserId = input.UserId,
                UserName = user.FirstName + " " + user.LastName,
                UserAvatar = user.AvatarUrl,
                Rating = input.Rating,
                Comment = input.Comment,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(review);
            await _repo.SaveChangesAsync();

            // recalculer moyenne
            var avg = await GetAverageRatingAsync(input.BoatId);
            // Option: update boat rating
            var existingBoat = await _db.Boats.FirstOrDefaultAsync(b => b.Id == input.BoatId);
            if (existingBoat != null)
            {
                existingBoat.Rating = (decimal)avg;
                existingBoat.ReviewCount = await _db.Reviews.CountAsync(r => r.BoatId == input.BoatId);
                await _db.SaveChangesAsync();
            }

            return Map(review);
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;
            _repo.Remove(existing);
            await _repo.SaveChangesAsync();

            // recalc avg
            var avg = await GetAverageRatingAsync(existing.BoatId);
            var boat = await _db.Boats.FirstOrDefaultAsync(b => b.Id == existing.BoatId);
            if (boat != null)
            {
                boat.Rating = (decimal)avg;
                boat.ReviewCount = await _db.Reviews.CountAsync(r => r.BoatId == existing.BoatId);
                await _db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<double> GetAverageRatingAsync(int boatId)
        {
            var avg = await _db.Reviews.Where(r => r.BoatId == boatId).Select(r => (double?)r.Rating).AverageAsync();
            return Math.Round(avg ?? 0.0, 2);
        }

        public async Task<List<ReviewDto>> GetRecentReviewsAsync(int limit = 10)
        {
            var list = await _db.Reviews.Include(r => r.User).Include(r => r.Boat).AsNoTracking().OrderByDescending(r => r.CreatedAt).Take(limit).ToListAsync();
            return list.Select(Map).ToList();
        }
    }
}
