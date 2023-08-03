using Microsoft.EntityFrameworkCore;
using TodoGrpcApp.Models;

namespace TodoGrpcApp.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }

        public DbSet<Todo> Todos { get; set; }
    }
}
