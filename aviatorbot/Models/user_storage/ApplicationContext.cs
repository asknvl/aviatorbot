using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aviatorbot.Models.user_storage
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = Path.Combine("C:", "bots");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, "bots.db");

            optionsBuilder.UseSqlite($"Data Source={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasIndex(u => new { u.geotag, u.tg_id })
            .HasDatabaseName("IX_User_geotag_tg_id").IsUnique();            
        }

    }
}
