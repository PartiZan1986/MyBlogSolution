using Microsoft.EntityFrameworkCore;
using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using MyBlog.Infrastructure;
using MyBlog.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public RoleService(IRoleRepository roleRepository, IUserRepository userRepository, ApplicationDbContext context)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<Role> CreateRoleAsync(string name, string description)
        {
            var existingRole = await _roleRepository.GetByNameAsync(name);
            if (existingRole != null)
                throw new InvalidOperationException("Роль с таким названием уже существует");

            var role = new Role { Name = name, Description = description };
            await _roleRepository.AddAsync(role);
            return role;
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            return await _roleRepository.GetByIdAsync(id);
        }

        public async Task<Role> GetRoleByNameAsync(string name)
        {
            return await _roleRepository.GetByNameAsync(name);
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _roleRepository.GetAllAsync();
        }

        public async Task AssignRoleToUserAsync(int userId, int roleId)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            var role = await _roleRepository.GetByIdAsync(roleId);

            if (user == null || role == null)
                throw new InvalidOperationException("Пользователь или роль не найдены");

            if (!user.Roles.Any(r => r.Id == roleId))
            {
                user.Roles.Add(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateRoleAsync(Role role)
        {
            var existingRole = await _roleRepository.GetByIdAsync(role.Id);
            if (existingRole == null)
                throw new KeyNotFoundException("Роль не найдена");

            // Проверяем, не пытаются ли изменить стандартные роли
            if (IsStandardRole(existingRole.Name) && existingRole.Name != role.Name)
                throw new InvalidOperationException("Нельзя изменять название стандартной роли");

            existingRole.Name = role.Name;
            existingRole.Description = role.Description;

            await _roleRepository.UpdateAsync(existingRole);
        }

        public async Task DeleteRoleAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new KeyNotFoundException("Роль не найдена");

            // Проверяем, не пытаются ли удалить стандартную роль
            if (IsStandardRole(role.Name))
                throw new InvalidOperationException("Нельзя удалять стандартные роли");

            // Проверяем, нет ли пользователей с этой ролью
            var usersWithRole = await _context.Users
                .Where(u => u.Roles.Any(r => r.Id == id))
                .CountAsync();

            if (usersWithRole > 0)
                throw new InvalidOperationException("Нельзя удалить роль, так как есть пользователи с этой ролью");

            await _roleRepository.DeleteAsync(id);
        }

        private bool IsStandardRole(string roleName)
        {
            var standardRoles = new[] { "Admin", "Moderator", "User" };
            return standardRoles.Contains(roleName);
        }

        public async Task RemoveRoleFromUserAsync(int userId, int roleId)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            var role = user?.Roles.FirstOrDefault(r => r.Id == roleId);

            if (user != null && role != null)
            {
                user.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName)
        {
            var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId);
            return user?.Roles.Any(r => r.Name == roleName) ?? false;
        }
    }
}
