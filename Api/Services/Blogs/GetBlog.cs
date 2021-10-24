using Api.Data;
using Api.Framework;
using FluentValidation;

namespace Api.Services
{
    public class GetBlog: Handler<GetBlogRequest, Blog>
    {
        private readonly AppDbContext ctx;

        public GetBlog(AppDbContext ctx)
        {
            this.ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
        }

        public override Task<Blog> Run(GetBlogRequest request)
        {
            // TODO - why can't we use FirstOrDefaultAsync ?
            var blog = this.ctx.Blogs.FirstOrDefault(b => b.Id == request.Id);

            return Task.FromResult(blog);   
        }
    }

    public class GetBlogRequest : IRequest<Blog>, IFromRoute
    {
        public int Id { get; private set; }

        public void BindFromRoute(RouteValueDictionary routeValues)
        {
            // Use an Extension method to get the Id from the Route
            this.Id = routeValues.GetInt("id");
        }
    }

    public class GetBlogRequestValidation : AbstractValidator<GetBlogRequest>
    {
        public GetBlogRequestValidation()
        {
            this.RuleFor(r => r.Id)
                .Must(v => v > 0)
                .WithMessage("Id needs to be greater than 0");
        }
    }
}

