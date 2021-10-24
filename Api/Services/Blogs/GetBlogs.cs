using Api.Data;
using Api.Framework;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class GetBlogs: Handler<GetBlogsRequest, List<Blog>>
    {
        private readonly AppDbContext ctx;

        public GetBlogs(AppDbContext ctx)
        {
            this.ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public override Task<List<Blog>> Run(GetBlogsRequest v)
        {
            return Task.FromResult(ctx.Blogs.ToList());
            //return ctx.Blogs.ToListAsync();
        }
    }

    public class GetBlogsRequest : IRequest<List<Blog>>
    {
    }
}

 