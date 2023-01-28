using Microsoft.EntityFrameworkCore;

namespace GUI.Data
{
    public class SQLConnector : DbContext
    {
        public DbSet<DataOrigin> DataOrigins { get; set; }
        public DbSet<DataTag> Tags { get; set; }

        public string DbPath { get; }

        public SQLConnector()
        {
            Environment.SpecialFolder folder = Environment.SpecialFolder.LocalApplicationData;
            string? path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "DataOriginReader.db");
            _ = Database.EnsureCreated();
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            _ = options.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.Entity<DataTag>()
            .HasOne(t => t.DataOrigin)
            .WithMany(o => o.Tags).OnDelete(DeleteBehavior.Cascade);
            _ = modelBuilder.Entity<DataOrigin>()
                .HasKey(o => new { o.Ip, o.Path });
            _ = modelBuilder.Entity<DataTag>()
               .HasKey(o => new { o.ID });
            _ = modelBuilder.Entity<DataTag>().Property(m => m.DefaultValue).IsRequired(false);
        }
    }

}
