using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using MyBlog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyBlog.Infrastructure.Data;

namespace MyBlog.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public UserService(IUserRepository userRepository, ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<User> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            if (await _userRepository.UserExistsAsync(email))
                throw new InvalidOperationException("Пользователь с таким email уже существует");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Email и пароль обязательны");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Email = email.Trim().ToLower(),
                PasswordHash = passwordHash,
                FirstName = firstName?.Trim(),
                LastName = lastName?.Trim(),
                RegisteredAt = DateTime.UtcNow
            };

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole != null)
            {
                user.Roles.Add(userRole);
            }

            await _userRepository.AddAsync(user);
            return user;
        }

        public async Task<User> GetUserWithRolesAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            var user = await GetUserWithRolesAsync(userId);
            return user?.Roles.Any(r => r.Name == roleName) ?? false;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");
            return user;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _userRepository.GetByIdAsync(user.Id);
            if (existingUser == null)
                throw new KeyNotFoundException("Пользователь не найден");

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;

            await _userRepository.UpdateAsync(existingUser);
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            await _userRepository.DeleteAsync(user.Id);
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
