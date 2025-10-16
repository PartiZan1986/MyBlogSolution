using MyBlog.Core.Interfaces;
using NLog; // ✅ Эта директива теперь будет работать
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public class LoggerService : ILoggerService
    {
        private readonly NLog.ILogger _userActionsLogger;
        private readonly NLog.ILogger _errorLogger;

        public LoggerService()
        {
            _userActionsLogger = LogManager.GetLogger("UserActions");
            _errorLogger = LogManager.GetLogger("errorFile");
        }

        public async Task LogUserActionAsync(string action, string description, int? userId = null, string? userName = null)
        {
            await Task.Run(() =>
            {
                var userInfo = userId.HasValue ? $"UserID: {userId}" : "Anonymous";
                if (!string.IsNullOrEmpty(userName))
                {
                    userInfo += $", UserName: {userName}";
                }

                _userActionsLogger.Info($"[UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] ACTION: {action} | {userInfo} | {description}");
            });
        }

        public async Task LogErrorAsync(string message, Exception? exception = null, int? userId = null)
        {
            await Task.Run(() =>
            {
                var userInfo = userId.HasValue ? $"UserID: {userId}" : "No user";
                var logMessage = $"[UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message} | {userInfo}";

                if (exception != null)
                {
                    _errorLogger.Error(exception, logMessage);
                }
                else
                {
                    _errorLogger.Error(logMessage);
                }
            });
        }

        public async Task LogInfoAsync(string message, int? userId = null)
        {
            await Task.Run(() =>
            {
                var userInfo = userId.HasValue ? $"UserID: {userId}" : "No user";
                _userActionsLogger.Info($"INFO: {message} | {userInfo}");
            });
        }
    }
}