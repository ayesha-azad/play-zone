using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlayZone.DAL.Models;

namespace PlayZone.DAL.Data.Contexts
{
    public class PlayZoneDbContext : DbContext
    {
        public PlayZoneDbContext(DbContextOptions<PlayZoneDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Game> Games { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<GameDevice> GameDevices { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Data Seeding
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = Guid.NewGuid(), Name = "Action" },
                new Category { Id = Guid.NewGuid(), Name = "Adventure" },
                new Category { Id = Guid.NewGuid(), Name = "Puzzle" },
                new Category { Id = Guid.NewGuid(), Name = "RPG" },
                new Category { Id = Guid.NewGuid(), Name = "Simulation" },
                new Category { Id = Guid.NewGuid(), Name = "Strategy" },
                new Category { Id = Guid.NewGuid(), Name = "Sports" },
                new Category { Id = Guid.NewGuid(), Name = "Racing" },
                new Category { Id = Guid.NewGuid(), Name = "Horror" }
            );

            modelBuilder.Entity<Device>().HasData(
                 new Device { Id = Guid.NewGuid(), Name = "PC", Icon = "bi bi-pc-display" },
                 new Device { Id = Guid.NewGuid(), Name = "Xbox", Icon = "bi bi-xbox" },
                 new Device { Id = Guid.NewGuid(), Name = "PlayStation", Icon = "bi bi-playstation" },
                 new Device { Id = Guid.NewGuid(), Name = "Nintendo Switch", Icon = "bi bi-nintendo-switch" }
            );

            modelBuilder.Entity<GameDevice>()
                .HasKey(gd => new { gd.GameId, gd.DeviceId });

            base.OnModelCreating(modelBuilder);
            
        }
    }
}
