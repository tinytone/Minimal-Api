using Api.Data;
using Api.Framework;
using System.Security.Claims;

namespace Api.Services
{
    public class CreateBlog : Handler<CreateBlogRequest, Blog>
    {
        private readonly AppDbContext ctx;

        public CreateBlog(AppDbContext ctx)
        {
            this.ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public override async Task<Blog> Run(CreateBlogRequest request)
        {
            var blog = new Blog
            {
                Title = request.Title,
                CreatedBy = request.CreatedBy,
            };

            this.ctx.Blogs.Add(blog);

            await ctx.SaveChangesAsync();

            return blog;
        }
    }

    public class CreateBlogRequest : IRequest<Blog>, IFromJsonBody, IWithUserContext
    {
        public string Title { get; set; }

        public string CreatedBy { get; set; }

        public void BindFromUser(ClaimsPrincipal user)
        {
            this.CreatedBy = user.Claims.FirstOrDefault(x => x.Type == "username").Value;
        }
    }
}
