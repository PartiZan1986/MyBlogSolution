using MyBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Interfaces
{
    public interface IRoleService
    {
        Task<Role> CreateRoleAsync(string name, string description);
        Task<Role> GetRoleByIdAsync(int id);
        Task<Role> GetRoleByNameAsync(string name);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task UpdateRoleAsync(Role role);
        Task DeleteRoleAsync(int id);
        Task AssignRoleToUserAsync(int userId, int roleId);
        Task RemoveRoleFromUserAsync(int userId, int roleId);
        Task<bool> UserHasRoleAsync(int userId, string roleName);
    }
}
