using Microsoft.EntityFrameworkCore;
using SistemaTurnos.Domain.Entities;

namespace SistemaTurnos.Infrastructure.Persistence;

public class SistemaTurnosDbContext : DbContext
{
    public SistemaTurnosDbContext(DbContextOptions<SistemaTurnosDbContext> options)
        : base(options)
    {
    }
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Persona>(entity =>
        {
            entity.ToTable("Personas");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Dni)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(p => p.Dni)
                .IsUnique();

            entity.Property(p => p.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Activo)
                .HasDefaultValue(true);
        });
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("Clientes");

            entity.HasKey(c => c.Id);

            entity.HasOne(c => c.Persona)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.Id)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(u => u.Rol)
                .IsRequired();

            entity.Property(u => u.Activo)
                .HasDefaultValue(true);

            entity.HasOne(u => u.Persona)
                .WithMany()
                .HasForeignKey(u => u.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
        });


    }

}
