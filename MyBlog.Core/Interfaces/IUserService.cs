using MyBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Interfaces
{
    public interface IUserService
    {
        Task<User> RegisterAsync(string email, string password, string? firstName, string? lastName);
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User?> GetUserWithRolesAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> ValidateUserAsync(string email, string password);
        Task<bool> UserHasRoleAsync(int userId, string roleName);
        //Task ChangeUserRoleAsync(int userId, string newRole);
    }
}
