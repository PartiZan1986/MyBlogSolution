using NLog;
using NLog.Web;
using MyBlog.Core.Interfaces;
using MyBlog.Services;
using MyBlog.Infrastructure.Data;
using MyBlog.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Настройка NLog
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.AccessDeniedPath = "/Auth/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });


    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Регистрация сервисов
    builder.Services.AddControllersWithViews();

    
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IRoleService, RoleService>();
    builder.Services.AddScoped<IArticleService, ArticleService>();
    builder.Services.AddScoped<ITagService, TagService>();
    builder.Services.AddScoped<ILoggerService, LoggerService>();

    
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IRoleRepository, RoleRepository>();
    builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
    builder.Services.AddScoped<ITagRepository, TagRepository>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            
            context.Database.Migrate();

            
            MyBlog.Infrastructure.Data.SeedData.Initialize(services);

            Console.WriteLine("База данных инициализирована с тестовыми данными!");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILoggerService>();
            await logger.LogErrorAsync("Ошибка при инициализации базы данных", ex);
        }
    }


    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    LogManager.GetCurrentClassLogger().Error(ex, "Stopped program because of exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}