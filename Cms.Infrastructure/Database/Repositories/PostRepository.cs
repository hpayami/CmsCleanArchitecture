﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cms.Core.Domain;
using Cms.Core.Dtos.UseCaseDtos;
using Cms.Core.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Database.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly CmsDbContext _cmsDbContext;

        public PostRepository(CmsDbContext cmsDbContext)
        {
            _cmsDbContext = cmsDbContext;
        }

        public async Task<Post> Add(Post domain)
        {
            var post = new Enities.Post
            {
                Title = domain.Title,
                CreatedDate = domain.CreatedDate,
                ModifiedDate = domain.ModifiedDate,
                Content = domain.Content
            };

            await _cmsDbContext.Posts.AddAsync(post);
            await _cmsDbContext.SaveChangesAsync();

            return new Post
            {
                Content = post.Content,
                ModifiedDate = post.ModifiedDate,
                Title = post.Title,
                CreatedDate = post.CreatedDate,
                Id = post.Id
            };
        }

        public async Task<bool> IsPostExist(int id)
        {
            return await _cmsDbContext.Posts.AnyAsync(x => x.Id == id);
        }

        public async Task RemoveById(int id)
        {
            var post = new Enities.Post() { Id = id };
            _cmsDbContext.Posts.Attach(post);
            _cmsDbContext.Posts.Remove(post);
            await _cmsDbContext.SaveChangesAsync();
        }

        public async Task<List<Post>> GetAll()
        {
          return await _cmsDbContext.Posts.Select(x=>new Post
          {
            ModifiedDate = x.ModifiedDate,
            Content = x.Content,
            CreatedDate = x.CreatedDate,
            Title = x.Title,
            Id = x.Id
          }).ToListAsync();
        }

        public async Task<Post> GetById(int id)
        {
            var dbItemQuery = _cmsDbContext.Posts
              .Include(x => x.Comments)
              .Include(x => x.PostCategories)
              .ThenInclude(x => x.Category)
              .AsQueryable();

            //Use AutoMapper Package for simplicity.
            //This section is for educational purposes only.
            return
              await dbItemQuery.Select(dbItem => new Post
              {
                  Id = dbItem.Id,
                  Content = dbItem.Content,
                  CreatedDate = dbItem.CreatedDate,
                  ModifiedDate = dbItem.ModifiedDate,
                  Title = dbItem.Title,
                  Comments = dbItem.Comments.Select(x => new Comment
                  {
                      Id = x.Id,
                      Content = x.Content,
                      Email = x.Email,
                      CreatedDate = x.CreatedDate,
                      Name = x.Name,
                      PostId = x.PostId
                  }),
                  Categories = dbItem.PostCategories.Select(x => new Category
                  {
                      Id = x.Id,
                      Title = x.Category.Title
                  })
              }).FirstOrDefaultAsync(dbItem => dbItem.Id == id);
        }

        public async Task<Post> GetByIdWithoutIncludes(int id)
        {
            return await _cmsDbContext.Posts
              .Select(x => new Post
              {
                  Id = x.Id,
                  Content = x.Content,
                  CreatedDate = x.CreatedDate,
                  Title = x.Title,
                  ModifiedDate = x.ModifiedDate
              })
              .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
