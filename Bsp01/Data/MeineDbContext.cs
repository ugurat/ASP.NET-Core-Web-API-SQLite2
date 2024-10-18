using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Bsp01.Data;

public partial class MeineDbContext : DbContext
{
    public MeineDbContext()
    {
    }

    public MeineDbContext(DbContextOptions<MeineDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Benutzer> Benutzers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite("name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Benutzer>(entity =>
        {
            entity.Property(e => e.Rolle).HasDefaultValueSql("'user'");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
