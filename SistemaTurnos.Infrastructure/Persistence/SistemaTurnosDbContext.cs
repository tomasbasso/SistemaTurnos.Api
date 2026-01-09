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
    public DbSet<Profesional> Profesionales { get; set; }
    public DbSet<Turno> Turnos { get; set; }
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.ToTable("Servicios");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.DuracionMinutos)
                .IsRequired();

            entity.Property(e => e.Activo)
                .HasDefaultValue(true);
        });
        modelBuilder.Entity<Turno>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Estado)
                  .HasConversion<int>();

            entity.HasOne(t => t.Persona)
                  .WithMany()
                  .HasForeignKey(t => t.PersonaId);

            entity.HasOne(t => t.Servicio)
                  .WithMany()
                  .HasForeignKey(t => t.ServicioId);

            entity.HasOne<Profesional>()
                  .WithMany()
                  .HasForeignKey(t => t.ProfesionalId);
        });

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
        modelBuilder.Entity<Profesional>(entity =>
        {
            entity.ToTable("Profesionales");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Matricula)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(p => p.Matricula)
                .IsUnique();

            entity.Property(p => p.Activo)
                .HasDefaultValue(true);
        });
        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.ToTable("Servicios");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(s => s.Descripcion)
                .HasMaxLength(500);

            entity.Property(s => s.DuracionMinutos)
                .IsRequired();

            entity.Property(s => s.Precio)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(s => s.Activo)
                .HasDefaultValue(true);
        });

    }

}
