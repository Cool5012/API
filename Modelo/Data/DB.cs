using API.Modelo;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Calendario> Calendarios { get; set; }
        public DbSet<Tarefa> Tarefas { get; set; }
        public DbSet<Etiqueta> Etiquetas { get; set; }
        public DbSet<Tem> Tem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Mapear tabelas (MySQL)
            modelBuilder.Entity<Utilizador>().ToTable("Utilizador");
            modelBuilder.Entity<Calendario>().ToTable("Calendario");
            modelBuilder.Entity<Tarefa>().ToTable("Tarefas");
            modelBuilder.Entity<Etiqueta>().ToTable("Etiquetas");
            modelBuilder.Entity<Tem>().ToTable("Tem");

            // Chave primária composta da tabela Tem
            modelBuilder.Entity<Tem>()
                .HasKey(t => new { t.ID_Tarefa, t.ID_Etiqueta });

            // Relações Tarefa <-> Utilizador
            modelBuilder.Entity<Tarefa>()
                .HasOne(t => t.Utilizador)
                .WithMany(u => u.Tarefas)
                .HasForeignKey(t => t.ID_Utilizador)
                .OnDelete(DeleteBehavior.Cascade);

            // Relações Tarefa <-> Calendario
            modelBuilder.Entity<Tarefa>()
                .HasOne(t => t.Calendario)
                .WithMany(c => c.Tarefas)
                .HasForeignKey(t => t.ID_Calendario)
                .OnDelete(DeleteBehavior.SetNull);

            // Relação Etiqueta <-> Utilizador
            modelBuilder.Entity<Etiqueta>()
                .HasOne(e => e.Utilizador)
                .WithMany(u => u.Etiquetas)
                .HasForeignKey(e => e.ID_Utilizador)
                .OnDelete(DeleteBehavior.Cascade);

            // Relações Tem <-> Tarefa
            modelBuilder.Entity<Tem>()
                .HasOne(te => te.Tarefa)
                .WithMany(t => t.Tem)
                .HasForeignKey(te => te.ID_Tarefa)
                .OnDelete(DeleteBehavior.Cascade);

            // Relações Tem <-> Etiqueta
            modelBuilder.Entity<Tem>()
                .HasOne(te => te.Etiqueta)
                .WithMany(e => e.Tem)
                .HasForeignKey(te => te.ID_Etiqueta)
                .OnDelete(DeleteBehavior.Cascade);

            // Estado da tarefa
            modelBuilder.Entity<Tarefa>()
                .Property(t => t.Estado)
                .HasConversion<string>()
                .HasDefaultValue("Pendente");
        }
    }
}
