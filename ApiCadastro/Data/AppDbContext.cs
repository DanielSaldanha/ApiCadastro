using Microsoft.EntityFrameworkCore;
using ApiCadastro.Model;

namespace ApiCadastro.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Cadastro { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("sua_string_de_conexão",
                    new MySqlServerVersion(new Version(8, 0, 21)),
                    options => options.CommandTimeout(80)); // Timeout de 60 segundos
            }
        }
    }
}

