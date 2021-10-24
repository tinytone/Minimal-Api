using Microsoft.EntityFrameworkCore;

namespace Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) 
            : base(options)
        {
        }

        public DbSet<Blog> Blogs {  get; set; }

        public DbSet<Post> Posts {  get; set; }
    }
}
