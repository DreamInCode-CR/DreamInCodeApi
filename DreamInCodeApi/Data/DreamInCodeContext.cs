using System;
using System.Collections.Generic;
using DreamInCodeApi.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DreamInCodeApi.Data;

public partial class DreamInCodeContext : DbContext
{
    public DreamInCodeContext()
    {
    }

    public DreamInCodeContext(DbContextOptions<DreamInCodeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChatThreads> ChatThreads { get; set; }

    public virtual DbSet<Enfermedades> Enfermedades { get; set; }

    public virtual DbSet<HistorialInteracciones> HistorialInteracciones { get; set; }

    public virtual DbSet<Messages> Messages { get; set; }

    public virtual DbSet<Perfiles> Perfiles { get; set; }

    public virtual DbSet<PreferenciasUsuario> PreferenciasUsuarios { get; set; }

    public virtual DbSet<RespuestasPersonalizadas> RespuestasPersonalizadas { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=Default");

   protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ChatThreads
    modelBuilder.Entity<ChatThreads>(entity =>
    {
        entity.ToTable("ChatThreads");
        entity.HasKey(e => e.ThreadID);

        entity.HasIndex(e => e.UsuarioID).HasDatabaseName("IX_ChatThreads_UsuarioID");

        entity.Property(e => e.Titulo).HasMaxLength(200);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

        entity.HasOne(e => e.Usuario)
              .WithMany()                              // no exponemos colección en Usuarios
              .HasForeignKey(e => e.UsuarioID)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_ChatThreads_Usuarios");
    });

    // Messages
    modelBuilder.Entity<Messages>(entity =>
    {
        entity.ToTable("Messages");
        entity.HasKey(e => e.MessageID);

        entity.HasIndex(e => new { e.ThreadID, e.CreatedAt })
              .HasDatabaseName("IX_Messages_Thread_Created");

        entity.Property(e => e.Role).HasMaxLength(20);
        entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");

        entity.HasOne(e => e.Thread)
              .WithMany(t => t.Messages)
              .HasForeignKey(e => e.ThreadID)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Messages_Threads");
    });

    // Perfiles
    modelBuilder.Entity<Perfiles>(entity =>
    {
        entity.ToTable("Perfiles");
        entity.HasKey(e => e.PerfilID);             

        entity.HasIndex(e => e.UsuarioID)
              .IsUnique()
              .HasDatabaseName("UQ__Perfiles__2B3DE79911CB56D9");

        entity.Property(e => e.Idioma).HasMaxLength(10);
        entity.Property(e => e.Voz).HasMaxLength(20);
        entity.Property(e => e.TamanoTexto).HasMaxLength(10);

        entity.HasOne(e => e.Usuario)
              .WithOne()                         
              .HasForeignKey<Perfiles>(e => e.UsuarioID)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK_Perfiles_Usuarios");
    });

    // HistorialInteracciones
    modelBuilder.Entity<HistorialInteracciones>(entity =>
    {
        entity.ToTable("HistorialInteracciones");
        entity.HasKey(e => e.InteraccionID);

        entity.HasIndex(e => e.UsuarioID).HasDatabaseName("IX_HistorialInteracciones_UsuarioID");

        entity.Property(e => e.EntradaVozTexto).HasMaxLength(500);
        entity.Property(e => e.Fecha).HasColumnType("datetime").HasDefaultValueSql("(getdate())");

        entity.HasOne(e => e.Usuario)
              .WithMany()                              // no exponemos colección en Usuarios
              .HasForeignKey(e => e.UsuarioID)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK__Historial__Usuar__6B24EA82");
    });

    // PreferenciasUsuario
    modelBuilder.Entity<PreferenciasUsuario>(entity =>
    {
        entity.ToTable("PreferenciasUsuario");
        entity.HasKey(e => e.PreferenciaID);

        entity.HasIndex(e => e.UsuarioID).HasDatabaseName("IX_PreferenciasUsuario_UsuarioID");

        entity.Property(e => e.TipoPreferencia).HasMaxLength(100);
        entity.Property(e => e.Valor).HasMaxLength(100);

        
        entity.HasOne(e => e.Usuario)
              .WithMany()                             
              .HasForeignKey(e => e.UsuarioID)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK__Preferenc__Usuar__6C190EBB");
    });

    // RespuestasPersonalizadas
    modelBuilder.Entity<RespuestasPersonalizadas>(entity =>
    {
        entity.ToTable("RespuestasPersonalizadas");
        entity.HasKey(e => e.RespuestaID);

        entity.HasIndex(e => e.UsuarioID).HasDatabaseName("IX_RespuestasPersonalizadas_UsuarioID");

        entity.Property(e => e.Pregunta).HasMaxLength(500);
        entity.Property(e => e.FechaRespuesta).HasColumnType("datetime").HasDefaultValueSql("(getdate())");

        // 👇 Navegación CONSISTENTE: RespuestasPersonalizadas.Usuario
        entity.HasOne(e => e.Usuario)
              .WithMany()                              // no exponemos colección en Usuarios
              .HasForeignKey(e => e.UsuarioID)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK__Respuesta__Usuar__6D0D32F4");
    });

    // Enfermedades (simple)
    modelBuilder.Entity<Enfermedades>(entity =>
    {
        entity.ToTable("Enfermedades");
        entity.HasKey(e => e.EnfermedadID);
        entity.Property(e => e.Nombre).HasMaxLength(255);
    });

    // Usuarios (simple)
    modelBuilder.Entity<Usuarios>(entity =>
    {
        entity.ToTable("Usuarios");
        entity.HasKey(e => e.UsuarioID);

        entity.HasIndex(e => e.Correo).IsUnique().HasDatabaseName("UQ__Usuarios__60695A19637CEDD0");

        entity.Property(e => e.Correo).HasMaxLength(255).IsUnicode(false);
        entity.Property(e => e.Nombre).HasMaxLength(100);
        entity.Property(e => e.PrimerApellido).HasMaxLength(100);
        entity.Property(e => e.SegundoApellido).HasMaxLength(100);
        entity.Property(e => e.Direccion).HasMaxLength(255);
        entity.Property(e => e.Telefono).HasMaxLength(30);
        entity.Property(e => e.Observaciones).HasMaxLength(255);
        entity.Property(e => e.PasswordHash).HasMaxLength(255);
        entity.Property(e => e.FechaRegistro).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
        entity.Property(e => e.TipoUsuario).HasDefaultValue(1);

    });

    OnModelCreatingPartial(modelBuilder);
}

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
