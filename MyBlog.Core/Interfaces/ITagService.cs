using MyBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Core.Interfaces
{
    public interface ITagService
    {
        Task<Tag> CreateTagAsync(string name);
        Task<Tag> GetTagByIdAsync(int id);
        Task<Tag> GetTagByNameAsync(string name);
        Task<IEnumerable<Tag>> GetAllTagsAsync();
        Task UpdateTagAsync(Tag tag);
        Task DeleteTagAsync(int id);
    }
}
