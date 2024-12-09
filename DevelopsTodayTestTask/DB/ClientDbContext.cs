using DevelopsTodayTestTask.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace DevelopsTodayTestTask.DB
{
    public class ClientDbContext : DbContext
    {
        public virtual DbSet<ImportData> ImportIntegration { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var config = JObject.Parse(File.ReadAllText("config.json"));

            optionsBuilder.UseSqlServer(config["ConnectionString"]?.ToString());

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ImportData>().ToTable("import_integration");
            modelBuilder.Entity<ImportData>().HasNoKey();
        }
    }
}
