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
    public DbSet<Profesional> Profesionales { get; set; }
    public DbSet<Turno> Turnos { get; set; }
    public DbSet<Servicio> Servicios => Set<Servicio>();
    public DbSet<HorarioTrabajo> HorariosTrabajo { get; set; }
    public DbSet<BloqueoTiempo> BloqueosTiempo { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Turno>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Estado)
                  .HasConversion<int>();

            entity.HasOne(t => t.Persona)
                  .WithMany()
                  .HasForeignKey(t => t.PersonaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Servicio)
                  .WithMany()
                  .HasForeignKey(t => t.ServicioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.Profesional)
                  .WithMany()
                  .HasForeignKey(t => t.ProfesionalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HorarioTrabajo>(entity =>
        {
            entity.ToTable("HorariosTrabajo");

            entity.HasKey(h => h.Id);

            entity.Property(h => h.DiaSemana)
                .IsRequired();

            entity.Property(h => h.HoraInicio)
                .IsRequired();

            entity.Property(h => h.HoraFin)
                .IsRequired();

            entity.HasOne(h => h.Profesional)
                .WithMany(p => p.HorariosTrabajo)
                .HasForeignKey(h => h.ProfesionalId);
        });

        modelBuilder.Entity<BloqueoTiempo>(entity =>
        {
            entity.ToTable("BloqueosTiempo");

            entity.HasKey(b => b.Id);

            entity.Property(b => b.FechaHoraInicio)
                .IsRequired();

            entity.Property(b => b.FechaHoraFin)
                .IsRequired();
            
            entity.Property(b => b.Motivo)
                .HasMaxLength(255);

            entity.HasOne(b => b.Profesional)
                .WithMany(p => p.BloqueosTiempo)
                .HasForeignKey(b => b.ProfesionalId);
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
            
            entity.HasOne(p => p.Profesional)
                .WithOne(p => p.Persona)
                .HasForeignKey<Profesional>(p => p.PersonaId);
        });
        modelBuilder.Entity<Profesional>(entity =>
        {
            entity.ToTable("Profesionales");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Matricula)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(p => p.Matricula)
                .IsUnique();

            entity.Property(p => p.Activo)
                .HasDefaultValue(true);

            entity.HasMany(p => p.Servicios)
                .WithMany(s => s.Profesionales);
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
