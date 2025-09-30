using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBlog.Core.Models;
using MyBlog.Infrastructure.Data;

namespace MyBlog.Infrastructure.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Создаем роли, если их нет
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin", Description = "Администратор системы" },
                    new Role { Name = "Moderator", Description = "Модератор контента" },
                    new Role { Name = "User", Description = "Обычный пользователь" }
                );
                context.SaveChanges();
            }

            // Создаем администратора, если нет пользователей
            if (!context.Users.Any())
            {
                var adminRole = context.Roles.First(r => r.Name == "Admin");
                var moderatorRole = context.Roles.First(r => r.Name == "Moderator");
                var userRole = context.Roles.First(r => r.Name == "User");

                var users = new[]
                {
                    new User
                    {
                        Email = "admin@blog.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        FirstName = "Администратор",
                        LastName = "Системы",
                        Roles = new List<Role> { adminRole }
                    },
                    new User
                    {
                        Email = "moderator@blog.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("moderator123"),
                        FirstName = "Модератор",
                        LastName = "Контента",
                        Roles = new List<Role> { moderatorRole }
                    },
                    new User
                    {
                        Email = "user@blog.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                        FirstName = "Обычный",
                        LastName = "Пользователь",
                        Roles = new List<Role> { userRole }
                    }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
            }
        }
    }
}