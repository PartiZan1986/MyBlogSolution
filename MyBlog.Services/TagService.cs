using MyBlog.Core.Interfaces;
using MyBlog.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<Tag> CreateTagAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название тега не может быть пустым");

            var existingTag = await _tagRepository.GetByNameAsync(name);
            if (existingTag != null)
                throw new InvalidOperationException("Тег с таким названием уже существует");

            var tag = new Tag { Name = name.Trim() };
            await _tagRepository.AddAsync(tag);
            return tag;
        }

        public async Task<Tag> GetTagByIdAsync(int id)
        {
            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<Tag> GetTagByNameAsync(string name)
        {
            return await _tagRepository.GetByNameAsync(name);
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        public async Task UpdateTagAsync(Tag tag)
        {
            await _tagRepository.UpdateAsync(tag);
        }

        public async Task DeleteTagAsync(int id)
        {
            await _tagRepository.DeleteAsync(id);
        }
    }
}
