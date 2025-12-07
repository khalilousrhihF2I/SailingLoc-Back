using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.DTOs;
using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<AppUser> _um;
        private readonly UserRepository _repo;

        public UserService(ApplicationDbContext db, UserManager<AppUser> um, UserRepository repo)
        {
            _db = db;
            _um = um;
            _repo = repo;
        }

        private UserDto Map(AppUser u)
        {
            var dto = new UserDto
            {
                Id = u.Id,
                Name = u.FirstName + " " + u.LastName,
                Email = u.Email,
                Type = u.UserType,
                Avatar = u.AvatarUrl,
                Phone = u.PhoneNumber,
                Verified = u.Verified,
                MemberSince = u.MemberSince,
                Documents = u.UserDocuments?.Select(d => d.DocumentUrl).ToList() ?? new List<string>(),
                BoatsCount = u.BoatsOwned?.Count ?? 0,
                TotalRevenue = u.BoatsOwned?.Sum(b => b.Bookings?.Sum(x => x.TotalPrice) ?? 0) ?? 0
            };
            return dto;
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            var users = await _db.Users
                .Include(u => u.UserDocuments)
                .Include(u => u.BoatsOwned).ThenInclude(b => b.Bookings)
                .AsNoTracking()
                .ToListAsync();
            return users.Select(Map).ToList();
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id)
        {
            var u = await _repo.GetUserWithDetailsAsync(id);
            if (u == null) return null;
            return Map(u);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var u = await _db.Users.Include(x => x.UserDocuments).Include(x => x.BoatsOwned).ThenInclude(b => b.Bookings)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Email == email);
            if (u == null) return null;
            return Map(u);
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var user = new AppUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.Phone,
                UserType = dto.Type,
                Verified = false,
                MemberSince = DateTime.UtcNow
            };

            var res = await _um.CreateAsync(user, dto.Password);
            if (!res.Succeeded) throw new InvalidOperationException(string.Join(';', res.Errors.Select(e => e.Description)));

            // assign role
            var role = dto.Type.Equals("owner", StringComparison.OrdinalIgnoreCase) ? "Owner" : "Renter";
            await _um.AddToRoleAsync(user, role);

            return Map(user);
        }

        public async Task<UserDto> UpdateUserAsync(UpdateUserDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
            if (user == null) throw new KeyNotFoundException("User not found");

            if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                user.Email = dto.Email;
                user.UserName = dto.Email;
            }
            if (!string.IsNullOrWhiteSpace(dto.Phone)) user.PhoneNumber = dto.Phone;
            if (!string.IsNullOrWhiteSpace(dto.Avatar)) user.AvatarUrl = dto.Avatar;

            await _db.SaveChangesAsync();
            return Map(user);
        }

        public async Task<UserDto> VerifyUserAsync(Guid id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) throw new KeyNotFoundException("User not found");
            user.Verified = true;
            await _db.SaveChangesAsync();
            return Map(user);
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return false;
            var res = await _um.DeleteAsync(user);
            return res.Succeeded;
        }
    }
}
