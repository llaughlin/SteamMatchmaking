using System.Data.Entity;
using WebApp.Models;

namespace WebApp.Infrastructure
{
    public class SteamMatchmakingContext : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<PlayerIndex> Rankings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new PlayerConfiguration());

        }


    }

    public class SteamMatchmakingContextInitializer : DropCreateDatabaseAlways<SteamMatchmakingContext>
    {

    }
}