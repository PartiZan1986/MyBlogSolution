using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Interfaces
{
    public interface ILoggerService
    {
        Task LogUserActionAsync(string action, string description, int? userId = null, string? userName = null);
        Task LogErrorAsync(string message, Exception? exception = null, int? userId = null);
        Task LogInfoAsync(string message, int? userId = null);
    }
}
