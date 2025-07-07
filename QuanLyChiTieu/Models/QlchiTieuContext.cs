using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QuanLyChiTieu.Models;

public partial class QlchiTieuContext : DbContext
{
    public QlchiTieuContext()
    {
    }

    public QlchiTieuContext(DbContextOptions<QlchiTieuContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseJar> ExpenseJars { get; set; }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<IncomeAllocation> IncomeAllocations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("PK__Expenses__1445CFD397BF0CB4");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.Jar).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.JarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Expenses__JarId__44FF419A");
        });

        modelBuilder.Entity<ExpenseJar>(entity =>
        {
            entity.HasKey(e => e.JarId).HasName("PK__ExpenseJ__A7EC00F9B716F2B6");

            entity.Property(e => e.JarName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.ExpenseJars)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExpenseJa__UserI__3E52440B");
        });

        modelBuilder.Entity<Income>(entity =>
        {
            entity.HasKey(e => e.IncomeId).HasName("PK__Income__60DFC60CB7911B6A");

            entity.ToTable("Income");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Incomes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Income__UserId__3B75D760");
        });

        modelBuilder.Entity<IncomeAllocation>(entity =>
        {
            entity.HasKey(e => e.AllocationId).HasName("PK__IncomeAl__B3C6D64B4A4905BA");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Income).WithMany(p => p.IncomeAllocations)
                .HasForeignKey(d => d.IncomeId)
                .HasConstraintName("FK__IncomeAll__Incom__412EB0B6");

            entity.HasOne(d => d.Jar).WithMany(p => p.IncomeAllocations)
                .HasForeignKey(d => d.JarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__IncomeAll__JarId__4222D4EF");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C7CC46C05");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E43D649825").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534F3843966").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
