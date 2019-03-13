using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace KnnResults.Domain.Models
{
    public partial class VADETContext : DbContext
    {
        public VADETContext()
        {
        }

        public VADETContext(DbContextOptions<VADETContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ProductVisualAttributes> ProductVisualAttributes { get; set; }
        public virtual DbSet<VisualAttributeDefinition> VisualAttributeDefinition { get; set; }
        public virtual DbSet<ZootBataProducts> ZootBataProducts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conn = Environment.GetEnvironmentVariable("VADETSQL");
                optionsBuilder.UseSqlServer(conn);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.2-servicing-10034");

            modelBuilder.Entity<ProductVisualAttributes>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Attribute)
                    .WithMany(p => p.ProductVisualAttributes)
                    .HasForeignKey(d => d.AttributeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductVisualAttributes_VisualAttributeDefinition");

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.ProductVisualAttributes)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ProductVisualAttributes_ZootBataProducts");
            });

            modelBuilder.Entity<VisualAttributeDefinition>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Candidates).IsRequired();

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Quality).HasMaxLength(50);
            });

            modelBuilder.Entity<ZootBataProducts>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasColumnName("ID")
                    .HasMaxLength(50)
                    .ValueGeneratedNever();

                entity.Property(e => e.Brand)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Categories).IsRequired();

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(255);
            });
        }
    }
}
