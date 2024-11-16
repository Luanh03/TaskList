using Microsoft.EntityFrameworkCore;

namespace TaskList.Models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options) 
        { 
        }
    
        public DbSet<Tasks> Tasks { get; set; }
    }
}
