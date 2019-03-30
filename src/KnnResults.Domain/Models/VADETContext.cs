using System;
using System.Configuration;
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
        public virtual DbSet<AttributeRejection> AttributeRejections { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var conn = Environment.GetEnvironmentVariable("VADETSQL") ;
                Console.WriteLine("Conn. string is " + conn);
                if (String.IsNullOrEmpty(conn))
                {
                    conn =  ConfigurationManager.ConnectionStrings[0].ConnectionString;
                    Console.WriteLine("Conn. string is " + conn);
                }
                optionsBuilder.UseSqlServer(conn );
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
                    .OnDelete(DeleteBehavior.Cascade)
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

            modelBuilder.Entity<AttributeRejection>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.OriginalProposalId).IsRequired();
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.AttributeSource).IsRequired();
                entity.Property(e => e.Time)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");
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
