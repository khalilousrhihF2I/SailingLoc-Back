using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.DTOs;

namespace Core.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task<UserDto> UpdateUserAsync(UpdateUserDto dto);
        Task<bool> DeleteUserAsync(Guid id);
        Task<UserDto> VerifyUserAsync(Guid id);
    }
}
