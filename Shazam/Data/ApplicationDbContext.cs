using Shazam.Models;
using Microsoft.EntityFrameworkCore;

namespace Shazam.Data
{
    // Esta classe representa a ligação à base de dados.
    // Cada DbSet corresponde a uma tabela.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Treinador> Treinadores { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<Inscricao> Inscricoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Esta configuração evita um erro do SQL Server chamado
            // "multiple cascade paths" (dois caminhos de apagamento em cascata
            // a chegar à mesma tabela). Quando se apaga uma aula, temos de
            // apagar primeiro as inscrições no código (feito no AulasController).
            modelBuilder.Entity<Inscricao>()
                .HasOne(i => i.Aula)
                .WithMany(a => a.Inscricoes)
                .HasForeignKey(i => i.AulaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
