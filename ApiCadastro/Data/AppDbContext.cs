using Microsoft.EntityFrameworkCore;
using ApiCadastro.Model;

namespace ApiCadastro.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Cadastro { get; set; }
    }
}
