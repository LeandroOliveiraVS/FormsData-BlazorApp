using FormulariosData.Authentication;
using FormulariosData.Models;
using Microsoft.EntityFrameworkCore;

namespace FormulariosData.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {     
        }

        // Esta propriedade representa a nossa tabela "Recebimentos" no banco de dados.
        public DbSet<Recebimento> Recebimentos { get; set; }
        // public DbSet<AppUser> GoogleUsers { get; set; }
    }
}
