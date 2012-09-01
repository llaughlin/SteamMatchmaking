using System.Data.Entity;
using SteamMatchmaking.Models;

namespace SteamMatchmaking.Infrastructure
{
    public class SteamMatchmakingContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<PlayerRank> Rankings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new PlayerConfiguration());

        }

    }
}
