using Microsoft.EntityFrameworkCore;

namespace ODrive.Models
{
    public class ODriveContext : DbContext
    {
        public ODriveContext(DbContextOptions<ODriveContext> options)
            : base(options)
        {
            // Create the database if it doesn't exist.
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UploadedFile>(entity =>
            {
                entity.ToTable("UploadedFiles");
                entity.HasKey(e => e.Fileid);
            });
        }

        public DbSet<UploadedFile> UploadedFiles { get; set; }
    }
}
