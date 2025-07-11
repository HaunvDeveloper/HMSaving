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

    public virtual DbSet<Debt> Debts { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseJar> ExpenseJars { get; set; }

    public virtual DbSet<Income> Incomes { get; set; }

    public virtual DbSet<IncomeAllocation> IncomeAllocations { get; set; }

    public virtual DbSet<Partner> Partners { get; set; }

    public virtual DbSet<PayDebt> PayDebts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=168.231.122.98;Initial Catalog=QLChiTieu;User ID=sa;Password=NguyenH@u100304;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Debt>(entity =>
        {
            entity.ToTable("Debt");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DebtDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(100);

            entity.HasOne(d => d.Partner).WithMany(p => p.Debts)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK_Debt_Partner");

            entity.HasOne(d => d.User).WithMany(p => p.Debts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Debt_Users");
        });

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
            entity.HasKey(e => e.JarId).HasName("PK__ExpenseJ__A7EC00F9B0C5ED0C");

            entity.Property(e => e.JarName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.ExpenseJars)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ExpenseJa__UserI__4316F928");
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
            entity.HasKey(e => e.AllocationId).HasName("PK__IncomeAl__B3C6D64B9D43FC97");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Income).WithMany(p => p.IncomeAllocations)
                .HasForeignKey(d => d.IncomeId)
                .HasConstraintName("FK__IncomeAll__Incom__412EB0B6");

            entity.HasOne(d => d.Jar).WithMany(p => p.IncomeAllocations)
                .HasForeignKey(d => d.JarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__IncomeAll__JarId__46E78A0C");
        });

        modelBuilder.Entity<Partner>(entity =>
        {
            entity.ToTable("Partner");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.PartnerName).HasMaxLength(100);
            entity.Property(e => e.UserId).HasDefaultValue(0L);
        });

        modelBuilder.Entity<PayDebt>(entity =>
        {
            entity.ToTable("PayDebt");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Partner).WithMany(p => p.PayDebts)
                .HasForeignKey(d => d.PartnerId)
                .HasConstraintName("FK_PayDebt_Partner");

            entity.HasOne(d => d.User).WithMany(p => p.PayDebts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_PayDebt_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0A0ACDCD");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4D5A5C462").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534A3F98054").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
