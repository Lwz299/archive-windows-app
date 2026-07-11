using Archive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Archive.Infrastructure.Persistence
{
    public class ArchiveDbContext : DbContext
    {
        public ArchiveDbContext(DbContextOptions<ArchiveDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Category> Categories => Set<Category>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Username).IsUnique();
                entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(50);
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.Property(b => b.Title).IsRequired();
                entity.Property(b => b.Author).HasDefaultValue(string.Empty);
                entity.Property(b => b.Category).HasDefaultValue(string.Empty);
                entity.Property(b => b.Publisher).HasDefaultValue(string.Empty);
                entity.Property(b => b.Language).HasDefaultValue(string.Empty);
                entity.Property(b => b.Notes).HasDefaultValue(string.Empty);
                entity.Property(b => b.FilePath).HasMaxLength(1000);
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Name).IsUnique();
            });
        }
    }
}
